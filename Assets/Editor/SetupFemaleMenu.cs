using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupFemaleMenu
{
    public static void Run()
    {
        const string scenePath = "Assets/Scenes/CharacterLab.unity";
        const string backup = @"H:\ChatGPT - IT\CharacterSource\FemaleCustomization\CharacterLab-before-female.unity";
        if (!File.Exists(backup)) File.Copy(scenePath, backup);
        var scene = EditorSceneManager.OpenScene(scenePath);
        var picker = UnityEngine.Object.FindFirstObjectByType<CharacterColorPicker>();
        if (picker.femaleCharacter) throw new InvalidOperationException("Female already connected; preserving setup.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/FemaleCharacter.prefab");
        var female = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        female.name = "FemaleCharacter";
        female.transform.rotation = Quaternion.Euler(0,180,0); // Face CharacterLab's camera.
        var maleBounds = BoundsOf(picker.character);
        var femaleBounds = BoundsOf(female.transform);
        female.transform.position += new Vector3(maleBounds.center.x - femaleBounds.center.x,
            maleBounds.min.y - femaleBounds.min.y, maleBounds.center.z - femaleBounds.center.z);
        picker.femaleCharacter = female.transform;
        string[] materials = { "FemaleSkin", "FemaleSkin", "FemaleEyes", "FemaleHair", "FemaleEyebrows", "FemaleEyelashes" };
        picker.femaleParts = new CharacterColorPicker.Part[picker.parts.Length];
        for (int i=0;i<picker.parts.Length;i++)
        {
            var source = picker.parts[i];
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Characters/Customization/Materials/" + materials[i] + ".mat");
            if (!mat || !mat.HasProperty(source.colorProperty)) throw new InvalidOperationException(materials[i]);
            picker.femaleParts[i] = new CharacterColorPicker.Part { title=source.title,source=mat,
                colorProperty=source.colorProperty,brightnessProperty=source.brightnessProperty,button=source.button };
        }
        picker.maleButton = Button(picker, "MaleButton", "Male", -60);
        picker.femaleButton = Button(picker, "FemaleButton", "Female", 60);
        female.SetActive(false);
        PrefabUtility.RecordPrefabInstancePropertyModifications(female.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(female);
        EditorUtility.SetDirty(picker);
        if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
        File.WriteAllText("Library/FemaleMenuSetup.txt", "PASS: Female prefab aligned to male preview, six bindings and two selector buttons saved. Runtime validation pending.");
    }

    static Bounds BoundsOf(Transform target)
    {
        var rs=target.GetComponentsInChildren<Renderer>(true);
        var bounds=rs[0].bounds;
        foreach(var r in rs) bounds.Encapsulate(r.bounds);
        return bounds;
    }

    static Button Button(CharacterColorPicker picker,string name,string label,float x)
    {
        var obj=UnityEngine.Object.Instantiate(picker.parts[0].button.gameObject,picker.transform,false);
        obj.name=name;
        var rect=(RectTransform)obj.transform;
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);
        rect.pivot=new Vector2(.5f,1);
        rect.anchoredPosition=new Vector2(x,-20);
        rect.sizeDelta=new Vector2(110,34);
        var button=obj.GetComponent<Button>();
        button.onClick=new Button.ButtonClickedEvent();
        button.image.color=Color.white;
        var text=obj.GetComponentInChildren<TMP_Text>();
        text.text=label; text.color=Color.black; text.fontSize=18;
        return button;
    }
}
