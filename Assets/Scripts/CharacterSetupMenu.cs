using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scene-session character choices. Assets are never modified by this menu.
public sealed class CharacterSetupMenu : MonoBehaviour
{
    [Serializable]
    public class HairOption
    {
        public string id;
        public string title;
        public bool female;
        public Sprite thumbnail;
        [Tooltip("An existing child object under the matching character. Leave empty for a future style.")]
        public GameObject hairObject;
    }
    public CharacterColorPicker picker;
    public GameObject characterPanel, colorsPanel;
    public Button characterTab, colorsTab;
    public TMP_InputField nameInput;
    public Sprite malePortrait, femalePortrait;
    public Image malePicture, femalePicture;
    public TMP_Text malePlaceholder, femalePlaceholder;
    public HairOption[] hairOptions;
    public Button[] hairButtons;
    public Image[] hairPictures;
    public TMP_Text[] hairLabels, hairPlaceholders;
    public TMP_Text hairStatus;
    public string CharacterName { get; private set; } = "";
    public string SelectedHairId => selectedIds[isFemale ? 1 : 0];
    public bool IsFemale => isFemale;
    readonly string[] selectedIds = new string[2];
    bool isFemale;

    void Start()
    {
        CharacterName = nameInput.text;
        nameInput.onValueChanged.AddListener(value => CharacterName = value);
        characterTab.onClick.AddListener(() => ShowColors(false));
        colorsTab.onClick.AddListener(() => ShowColors(true));
        picker.maleButton.onClick.AddListener(() => SetGender(false));
        picker.femaleButton.onClick.AddListener(() => SetGender(true));
        for (int i=0;i<hairButtons.Length;i++)
        {
            int slot=i;
            hairButtons[i].onClick.AddListener(() => ChooseHair(slot));
        }
        SetPortrait(malePicture,malePlaceholder,malePortrait);
        SetPortrait(femalePicture,femalePlaceholder,femalePortrait);
        foreach(var option in hairOptions)
            if(option.hairObject && string.IsNullOrEmpty(selectedIds[option.female?1:0]))
                selectedIds[option.female?1:0]=option.id;
        SetGender(false);
        ShowColors(false);
    }

    static void SetPortrait(Image image,TMP_Text placeholder,Sprite sprite)
    {
        image.sprite=sprite; image.enabled=sprite;
        placeholder.gameObject.SetActive(!sprite);
    }

    public void ShowColors(bool colors)
    {
        // Close first: a hidden picker must never keep an old editing target.
        picker.ClosePicker();
        characterPanel.SetActive(!colors);
        colorsPanel.SetActive(colors);
        characterTab.interactable=colors;
        colorsTab.interactable=!colors;
    }

    public void SetGender(bool female)
    {
        isFemale=female;
        picker.SelectFemale(female);
        foreach(var option in hairOptions)
            if(option.female==female && option.hairObject)
                option.hairObject.SetActive(option.id==selectedIds[female?1:0]);
        RefreshHair();
    }

    HairOption OptionAt(int slot)
    {
        int index=0;
        foreach(var option in hairOptions)
            if(option.female==isFemale && index++==slot) return option;
        return null;
    }

    public void ChooseHair(int slot)
    {
        var option=OptionAt(slot);
        if(option==null || !option.hairObject)return;
        selectedIds[isFemale?1:0]=option.id;
        foreach(var item in hairOptions)
            if(item.female==isFemale && item.hairObject) item.hairObject.SetActive(item==option);
        RefreshHair();
    }

    void RefreshHair()
    {
        string selected="None";
        for(int i=0;i<hairButtons.Length;i++)
        {
            var option=OptionAt(i);
            hairButtons[i].gameObject.SetActive(option!=null);
            if(option==null)continue;
            bool ready=option.hairObject;
            bool active=option.id==SelectedHairId;
            hairButtons[i].interactable=ready && !active;
            hairLabels[i].text=option.title;
            hairPictures[i].sprite=option.thumbnail;
            hairPictures[i].enabled=option.thumbnail;
            hairPlaceholders[i].gameObject.SetActive(!option.thumbnail);
            hairPlaceholders[i].text=ready?"Preview\npending":"Coming\nlater";
            if(active)selected=option.title;
        }
        hairStatus.text="Selected: "+selected;
    }
}
