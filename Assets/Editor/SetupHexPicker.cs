using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupHexPicker
{
    [MenuItem("Tools/Character Customization/Add Hex Color Field")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        var picker = UnityEngine.Object.FindFirstObjectByType<CharacterColorPicker>();
        if (!picker || picker.gameObject.scene.name != "CharacterLab") throw new InvalidOperationException("Open CharacterLab first.");
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add hex color field");
        var panel = picker.pickerPanel.transform;
        var existing = panel.Find("HexInput");
        TMP_InputField input;
        if (existing) input = existing.GetComponent<TMP_InputField>();
        else
        {
            var go = new GameObject("HexInput", typeof(RectTransform), typeof(Image));
            go.layer = panel.gameObject.layer;
            go.transform.SetParent(panel, false);
            Undo.RegisterCreatedObjectUndo(go, "Create hex input");
            var background = go.GetComponent<Image>();
            background.color = Color.white;
            var area = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            area.layer = go.layer;
            area.transform.SetParent(go.transform, false);
            var areaRect = (RectTransform)area.transform;
            areaRect.anchorMin = Vector2.zero; areaRect.anchorMax = Vector2.one;
            areaRect.offsetMin = new Vector2(10, 3); areaRect.offsetMax = new Vector2(-10, -3);
            var text = MakeText(area.transform, picker.pickerTitle, "Text", "#808080", 18);
            text.color = Color.black;
            var tr = text.rectTransform;
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            text.richText = false;
            input = go.AddComponent<TMP_InputField>();
            input.textViewport = areaRect;
            input.textComponent = text;
            input.targetGraphic = background;
            input.contentType = TMP_InputField.ContentType.Standard;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 0; // Validate the full paste rather than silently truncating it.
            input.SetTextWithoutNotify("#808080");
        }
        Place((RectTransform)input.transform, 25, -115, 210, 34);
        var label = GetText(panel, picker.pickerTitle, "HexLabel", "Hex", 18);
        Place(label.rectTransform, -112, -115, 40, 34);
        var error = GetText(panel, picker.pickerTitle, "HexError", "", 14);
        error.color = new Color(.35f, 0, 0, 1);
        Place(error.rectTransform, 0, -151, 280, 25);
        Move((RectTransform)picker.red.transform, -185);
        Move((RectTransform)picker.green.transform, -230);
        Move((RectTransform)picker.blue.transform, -275);
        Move(picker.brightnessLabel.rectTransform, -310);
        Move((RectTransform)picker.brightness.transform, -345);
        Move((RectTransform)picker.closeButton.transform, -395);
        var panelRect = (RectTransform)panel;
        Undo.RecordObject(panelRect, "Expand picker");
        panelRect.sizeDelta = new Vector2(300, 460);
        Undo.RecordObject(picker, "Wire hex field");
        picker.hexInput = input; picker.hexError = error;
        EditorUtility.SetDirty(picker);
        EditorSceneManager.MarkSceneDirty(picker.gameObject.scene);
        Undo.CollapseUndoOperations(group);
        if (!EditorSceneManager.SaveScene(picker.gameObject.scene)) throw new InvalidOperationException("Scene save failed.");
        File.WriteAllText("Library/HexPickerSetup.txt", "PASS: Hex field and validation message created, layout expanded, references assigned, scene saved.");
    }
    static TMP_Text MakeText(Transform parent, TMP_Text template, string name, string value, float size)
    {
        var go = UnityEngine.Object.Instantiate(template.gameObject, parent, false);
        go.name = name;
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        var text = go.GetComponent<TMP_Text>();
        text.text = value; text.fontSize = size; text.enableAutoSizing = false;
        text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        return text;
    }
    static TMP_Text GetText(Transform parent, TMP_Text template, string name, string value, float size)
    {
        var child = parent.Find(name);
        return child ? child.GetComponent<TMP_Text>() : MakeText(parent, template, name, value, size);
    }
    static void Move(RectTransform rect, float y)
    { Undo.RecordObject(rect, "Move picker control"); rect.anchoredPosition = new Vector2(rect.anchoredPosition.x,y); }
    static void Place(RectTransform rect, float x, float y, float width, float height)
    {
        Undo.RecordObject(rect, "Position hex field");
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
        rect.pivot = new Vector2(.5f, 1);
        rect.anchoredPosition = new Vector2(x,y); rect.sizeDelta = new Vector2(width,height);
    }
}
