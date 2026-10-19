using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class SetupColorPicker
{
    const string Report = "Library/ColorPickerSetup.txt";

    [MenuItem("Tools/Character Customization/Set Up Color Picker")]
    public static void Run()
    {
        try
        {
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "CharacterLab" || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open CharacterLab outside Play Mode.");
            var all = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Transform>(true)).ToArray();
            var panel = all.Single(x => x.name == "ColorPickerPanel");
            var menu = all.Single(x => x.name == "CharacterCustomizationPanel");
            var canvas = panel.GetComponentInParent<Canvas>();
            if (!canvas) throw new InvalidOperationException("Picker must be under Canvas.");
            var red = panel.Find("RedSlider").GetComponent<Slider>();
            var title = panel.Find("PickerTitle").GetComponent<TMP_Text>();
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Set up RGB and brightness picker");
            int group = Undo.GetCurrentGroup();
            var green = CloneSlider(panel, red, "GreenSlider", "G", -170);
            var blue = CloneSlider(panel, red, "BlueSlider", "B", -215);
            ConfigureSlider(red, "R", -125);
            var bright = CloneSlider(panel, red, "BrightnessSlider", "", -285);
            bright.minValue = 0;
            bright.maxValue = 2;
            bright.SetValueWithoutNotify(1);
            var brightnessText = Text(panel, title, "BrightnessLabel", "Brightness 1.00", -250, 18);
            var rect = (RectTransform)panel;
            Undo.RecordObject(rect, "Resize picker");
            rect.sizeDelta = new Vector2(300, 400);

            var closeTransform = panel.Find("DoneButton");
            if (!closeTransform)
            {
                var source = menu.Find("SkinColorRow/ColorButton");
                var obj = UnityEngine.Object.Instantiate(source.gameObject, panel, false);
                obj.name = "DoneButton";
                Undo.RegisterCreatedObjectUndo(obj, "Create Done button");
                closeTransform = obj.transform;
            }
            var closeRect = (RectTransform)closeTransform;
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(.5f, 1);
            closeRect.pivot = new Vector2(.5f, 1);
            closeRect.anchoredPosition = new Vector2(0, -335);
            closeRect.sizeDelta = new Vector2(140, 40);
            closeTransform.GetComponentInChildren<TMP_Text>().text = "Done";

            var controller = canvas.GetComponent<CharacterColorPicker>();
            if (!controller) controller = Undo.AddComponent<CharacterColorPicker>(canvas.gameObject);
            Undo.RecordObject(controller, "Connect picker");
            controller.character = all.Single(x => x.name == "MaleCharacter_Base");
            controller.pickerPanel = panel.gameObject;
            controller.pickerTitle = title;
            controller.preview = panel.Find("ColorPreview").GetComponent<Image>();
            controller.red = red; controller.green = green; controller.blue = blue;
            controller.brightness = bright; controller.brightnessLabel = brightnessText;
            controller.closeButton = closeTransform.GetComponent<Button>();
            string[] names = { "Skin", "Lips", "Eyes", "Hair", "Eyebrows", "Eyelashes" };
            string[] mats = { "CharacterSkin", "CharacterSkin", "CharacterEyes", "CharacterHair", "CharacterEyebrows", "CharacterEyelashes" };
            string[] colors = { "_Skin_Color", "_Lips_Color", "_Iris_Color", "_Hair_Color", "_Hair_Color", "_Hair_Color" };
            string[] brightness = { "_Skin_Brightness", "_Lip_Brightness", "", "_Hair_Brightness", "_Hair_Brightness", "_Hair_Brightness" };
            controller.parts = new CharacterColorPicker.Part[names.Length];
            var used = controller.character.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Where(m => m).Distinct().ToArray();
            for (int i = 0; i < names.Length; i++)
            {
                var material = used.Single(m => m.name == mats[i]);
                if (!material.HasProperty(colors[i]) || (brightness[i] != "" && !material.HasProperty(brightness[i])))
                    throw new InvalidOperationException("Missing shader property for " + names[i]);
                controller.parts[i] = new CharacterColorPicker.Part {
                    title = names[i], source = material, colorProperty = colors[i], brightnessProperty = brightness[i],
                    button = menu.Find(names[i] + "ColorRow/ColorButton").GetComponent<Button>() };
            }
            Undo.CollapseUndoOperations(group);
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Save failed.");
            File.WriteAllText(Report, "SUCCESS: RGB and brightness controls created, six material bindings verified, scene saved. Runtime visual/input test still required.");
        }
        catch (Exception ex) { File.WriteAllText(Report, "FAILED: " + ex); Debug.LogException(ex); }
    }

    static Slider CloneSlider(Transform parent, Slider source, string name, string label, float y)
    {
        var child = parent.Find(name);
        if (!child)
        {
            var obj = UnityEngine.Object.Instantiate(source.gameObject, parent, false);
            obj.name = name;
            Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
            child = obj.transform;
        }
        var slider = child.GetComponent<Slider>();
        ConfigureSlider(slider, label, y);
        return slider;
    }
    static void ConfigureSlider(Slider slider, string text, float y)
    {
        Undo.RecordObjects(new UnityEngine.Object[] {slider, slider.transform}, "Configure slider");
        var rect = (RectTransform)slider.transform;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(15, y);
        rect.sizeDelta = new Vector2(220, 20);
        slider.minValue = 0; slider.maxValue = 1; slider.wholeNumbers = false;
        var label = slider.transform.Find("Label").GetComponent<TMP_Text>();
        Undo.RecordObject(label, "Set slider label");
        label.text = text; label.fontSize = 18; label.alignment = TextAlignmentOptions.Center;
        label.raycastTarget = false;
    }
    static TMP_Text Text(Transform panel, TMP_Text template, string name, string value, float y, float size)
    {
        var child = panel.Find(name);
        if (!child)
        {
            var obj = UnityEngine.Object.Instantiate(template.gameObject, panel, false);
            obj.name = name;
            Undo.RegisterCreatedObjectUndo(obj, "Create label");
            child = obj.transform;
        }
        var rect = (RectTransform)child;
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(0, y);
        rect.sizeDelta = new Vector2(280, 30);
        var label = child.GetComponent<TMP_Text>();
        label.text = value; label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center; label.raycastTarget = false;
        return label;
    }
}
