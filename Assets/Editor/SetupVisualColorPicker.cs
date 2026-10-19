using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupVisualColorPicker
{
    [MenuItem("Tools/Character Customization/Add Visual Color Picker")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        var p = UnityEngine.Object.FindFirstObjectByType<CharacterColorPicker>();
        if (!p || p.gameObject.scene.name != "CharacterLab" || !p.hexInput) throw new InvalidOperationException("Open the completed CharacterLab hex picker first.");
        var panel = (RectTransform)p.pickerPanel.transform;
        Undo.IncrementCurrentGroup(); int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Add visual color picker");
        var hsv = panel.GetComponent<HsvColorPicker>() ?? Undo.AddComponent<HsvColorPicker>(panel.gameObject);
        Undo.RecordObjects(new UnityEngine.Object[] {p,hsv,panel}, "Connect visual picker");
        p.visualPicker = hsv; hsv.controller = p;
        hsv.square = Surface(panel, "ColorSquare", -90, -60, 170, 170, hsv, false);
        hsv.strip = Surface(panel, "HueStrip", 20, -60, 22, 170, hsv, true);
        hsv.shadeMarker = Marker(hsv.square.transform, "ShadeMarker", 9, 9);
        hsv.hueMarker = Marker(hsv.strip.transform, "HueMarker", 28, 3);
        panel.sizeDelta = new Vector2(380, 490);
        Place(p.preview.rectTransform, 100, -60, 95, 170);
        Place(p.hexInput.GetComponent<RectTransform>(), 25, -244, 280, 32);
        Place(panel.Find("HexLabel").GetComponent<RectTransform>(), -150, -244, 40, 32);
        Place(p.hexError.rectTransform, 0, -277, 350, 22);
        Place((RectTransform)p.red.transform, 15, -305, 300, 18);
        Place((RectTransform)p.green.transform, 15, -335, 300, 18);
        Place((RectTransform)p.blue.transform, 15, -365, 300, 18);
        Place(p.brightnessLabel.rectTransform, 0, -390, 350, 25);
        Place((RectTransform)p.brightness.transform, 15, -418, 300, 18);
        Place((RectTransform)p.closeButton.transform, 0, -448, 140, 30);
        EditorUtility.SetDirty(p); EditorUtility.SetDirty(hsv);
        Undo.CollapseUndoOperations(group);
        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
        if (!EditorSceneManager.SaveScene(panel.gameObject.scene)) throw new InvalidOperationException("Save failed.");
    }
    static RawImage Surface(RectTransform parent, string name, float x,float y,float w,float h,HsvColorPicker picker,bool hue)
    {
        var child=parent.Find(name);
        if (!child)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(RawImage),typeof(ColorPickerSurface));
            go.layer=parent.gameObject.layer; go.transform.SetParent(parent,false);
            Undo.RegisterCreatedObjectUndo(go,"Create "+name); child=go.transform;
        }
        Place((RectTransform)child,x,y,w,h);
        var input=child.GetComponent<ColorPickerSurface>(); input.picker=picker; input.hueStrip=hue;
        return child.GetComponent<RawImage>();
    }
    static RectTransform Marker(Transform parent,string name,float w,float h)
    {
        var child=parent.Find(name);
        if(!child)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Outline));
            go.layer=parent.gameObject.layer; go.transform.SetParent(parent,false);
            Undo.RegisterCreatedObjectUndo(go,"Create marker"); child=go.transform;
            go.GetComponent<Image>().raycastTarget=false;
            var outline=go.GetComponent<Outline>(); outline.effectColor=Color.black; outline.effectDistance=new Vector2(1,-1);
        }
        var rect=(RectTransform)child; rect.pivot=new Vector2(.5f,.5f); rect.sizeDelta=new Vector2(w,h);
        return rect;
    }
    static void Place(RectTransform rect,float x,float y,float w,float h)
    {
        Undo.RecordObject(rect,"Position picker control");
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,1); rect.pivot=new Vector2(.5f,1);
        rect.anchoredPosition=new Vector2(x,y); rect.sizeDelta=new Vector2(w,h);
    }
}
