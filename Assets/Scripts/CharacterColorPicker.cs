using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Owns temporary material instances. Leaving the scene restores the original assets.
public sealed class CharacterColorPicker : MonoBehaviour
{
    [Serializable]
    public class Part
    {
        public string title;
        public Material source;
        public string colorProperty;
        public string brightnessProperty;
        public Button button;
        [NonSerialized] public Material instance;
    }

    public Transform character;
    public GameObject pickerPanel;
    public TMP_Text pickerTitle;
    public Image preview;
    public Slider red, green, blue, brightness;
    public TMP_Text brightnessLabel;
    public TMP_InputField hexInput;
    public TMP_Text hexError;
    public HsvColorPicker visualPicker;
    public Button closeButton;
    public Part[] parts;
    public Transform femaleCharacter;
    public Part[] femaleParts;
    public Button maleButton, femaleButton;
    Part[] maleParts;
    Transform maleCharacter;

    readonly Dictionary<Material, Material> copies = new Dictionary<Material, Material>();
    readonly Dictionary<Renderer, Material[]> originals = new Dictionary<Renderer, Material[]>();
    int selected = -1;

    void Awake()
    {
        maleParts = parts;
        maleCharacter = character;
        InitializeCharacter(character, parts);
        if (femaleCharacter) InitializeCharacter(femaleCharacter, femaleParts);
        for (int i = 0; i < parts.Length; i++)
        {
            int index = i;
            parts[i].button.onClick.AddListener(() => Open(index));
            UpdateButton(parts[i]);
        }
        red.onValueChanged.AddListener(_ => ApplyColor());
        green.onValueChanged.AddListener(_ => ApplyColor());
        blue.onValueChanged.AddListener(_ => ApplyColor());
        brightness.onValueChanged.AddListener(ApplyBrightness);
        if (hexInput) hexInput.onEndEdit.AddListener(ApplyHex);
        closeButton.onClick.AddListener(() => pickerPanel.SetActive(false));
        if (maleButton) maleButton.onClick.AddListener(() => SelectFemale(false));
        if (femaleButton) femaleButton.onClick.AddListener(() => SelectFemale(true));
        pickerPanel.SetActive(false);
        if (femaleCharacter) SelectFemale(false);
    }

    void InitializeCharacter(Transform target, Part[] bindings)
    {
        foreach (var part in bindings)
        {
            if (!part.source || !part.source.HasProperty(part.colorProperty))
                throw new InvalidOperationException("Missing color material/property: " + part.title);
            if (!copies.TryGetValue(part.source, out var copy))
            {
                copy = new Material(part.source) { name = part.source.name + " (Customization)" };
                copies.Add(part.source, copy);
            }
            part.instance = copy;
        }
        foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            var original = (Material[])materials.Clone();
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
                if (materials[i] && copies.TryGetValue(materials[i], out var copy))
                { materials[i] = copy; changed = true; }
            if (!changed) continue;
            originals.Add(renderer, original);
            renderer.sharedMaterials = materials;
        }
    }

    public void SelectFemale(bool female)
    {
        if (!femaleCharacter) return;
        // Clear the active edit before switching so stale input cannot tint the other character.
        selected = -1;
        pickerPanel.SetActive(false);
        parts = female ? femaleParts : maleParts;
        character = female ? femaleCharacter : maleCharacter;
        maleCharacter.gameObject.SetActive(!female);
        femaleCharacter.gameObject.SetActive(female);
        foreach (var part in parts) UpdateButton(part);
        if (maleButton) maleButton.interactable = female;
        if (femaleButton) femaleButton.interactable = !female;
    }

    public void Open(int index)
    {
        selected = index;
        var part = parts[index];
        var color = part.instance.GetColor(part.colorProperty);
        red.SetValueWithoutNotify(color.r);
        green.SetValueWithoutNotify(color.g);
        blue.SetValueWithoutNotify(color.b);
        preview.color = new Color(color.r, color.g, color.b, 1);
        RefreshHex(color);
        pickerTitle.text = part.title + " Color";
        bool supported = !string.IsNullOrEmpty(part.brightnessProperty) && part.instance.HasProperty(part.brightnessProperty);
        brightness.interactable = supported;
        brightness.SetValueWithoutNotify(supported ? part.instance.GetFloat(part.brightnessProperty) : 1);
        brightnessLabel.text = supported ? "Brightness " + brightness.value.ToString("0.00") : "Brightness — unavailable for eyes";
        pickerPanel.SetActive(true);
    }

    public void ClosePicker()
    {
        selected = -1;
        pickerPanel.SetActive(false);
    }

    void ApplyColor()
    {
        if (selected < 0) return;
        var part = parts[selected];
        var color = new Color(red.value, green.value, blue.value, 1);
        part.instance.SetColor(part.colorProperty, color);
        preview.color = color;
        UpdateButton(part);
        RefreshHex(color);
    }

    void RefreshHex(Color color)
    {
        if (visualPicker) visualPicker.SetColorWithoutNotify(color);
        if (hexInput) hexInput.SetTextWithoutNotify("#" + ColorUtility.ToHtmlStringRGB(color));
        if (hexError) hexError.text = "";
    }

    public void ApplyHex(string value)
    {
        if (selected < 0) return;
        string digits = (value ?? "").Trim();
        if (digits.StartsWith("#")) digits = digits.Substring(1);
        bool valid = digits.Length == 6;
        foreach (char c in digits)
            valid &= (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        if (!valid || !ColorUtility.TryParseHtmlString("#" + digits, out var color))
        {
            if (hexError) hexError.text = "Use 6 hex digits, e.g. #808080";
            return;
        }
        ApplyPickedColor(color);
    }

    public void ApplyPickedColor(Color color)
    {
        red.SetValueWithoutNotify(color.r);
        green.SetValueWithoutNotify(color.g);
        blue.SetValueWithoutNotify(color.b);
        ApplyColor();
    }

    void ApplyBrightness(float value)
    {
        if (selected < 0) return;
        var part = parts[selected];
        if (string.IsNullOrEmpty(part.brightnessProperty)) return;
        part.instance.SetFloat(part.brightnessProperty, value);
        brightnessLabel.text = "Brightness " + value.ToString("0.00");
    }

    void UpdateButton(Part part)
    {
        var color = part.instance.GetColor(part.colorProperty);
        color.a = 1;
        part.button.image.color = color;
        var text = part.button.GetComponentInChildren<TMP_Text>();
        text.text = "Choose Color";
        text.color = color.grayscale > .55f ? Color.black : Color.white;
    }

    void OnDestroy()
    {
        foreach (var pair in originals)
            if (pair.Key) pair.Key.sharedMaterials = pair.Value;
        foreach (var copy in copies.Values) Destroy(copy);
    }
}
