using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SetupCharacterTabs
{
    public static void FixFemalePreview()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        var picker=UnityEngine.Object.FindFirstObjectByType<CharacterColorPicker>();
        // The female FBX faces +Z; CharacterLab's preview camera is on -Z.
        picker.femaleCharacter.rotation=Quaternion.Euler(0,180,0);
        var male=BoundsOf(picker.character);var female=BoundsOf(picker.femaleCharacter);
        picker.femaleCharacter.position+=new Vector3(male.center.x-female.center.x,male.min.y-female.min.y,male.center.z-female.center.z);
        PrefabUtility.RecordPrefabInstancePropertyModifications(picker.femaleCharacter);
        EditorSceneManager.SaveScene(scene);
    }
    static Bounds BoundsOf(Transform root)
    {
        var renderers=root.GetComponentsInChildren<Renderer>(true);var bounds=renderers[0].bounds;
        foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
    static TMP_Text textTemplate;
    static Button buttonTemplate;
    public static void Run()
    {
        const string scenePath="Assets/Scenes/CharacterLab.unity";
        const string backup=@"H:\ChatGPT - IT\CharacterSource\FemaleCustomization\CharacterLab-before-tabs.unity";
        if(!File.Exists(backup))File.Copy(scenePath,backup);
        var scene=EditorSceneManager.OpenScene(scenePath);
        var picker=UnityEngine.Object.FindFirstObjectByType<CharacterColorPicker>();
        if(picker.GetComponent<CharacterSetupMenu>())throw new InvalidOperationException("Tabs already installed.");
        var colors=picker.parts[0].button.transform.parent.parent.GetComponent<RectTransform>();
        textTemplate=colors.Find("Title").GetComponent<TMP_Text>();
        buttonTemplate=picker.parts[0].button;
        var container=new GameObject("CharacterMenuPanel",typeof(RectTransform),typeof(Image));
        container.transform.SetParent(colors.parent,false);
        var outer=(RectTransform)container.transform;
        outer.anchorMin=colors.anchorMin;outer.anchorMax=colors.anchorMax;outer.pivot=colors.pivot;
        outer.anchoredPosition=colors.anchoredPosition;outer.sizeDelta=colors.sizeDelta;
        var background=container.GetComponent<Image>();
        background.sprite=colors.GetComponent<Image>().sprite;
        background.type=Image.Type.Sliced;background.color=new Color(.13f,.16f,.21f,.97f);
        colors.SetParent(outer,false);Stretch(colors);
        colors.GetComponent<Image>().enabled=false;
        textTemplate.text="Colors";Rect(textTemplate.rectTransform,0,55,280,30);
        for(int i=0;i<picker.parts.Length;i++)Rect((RectTransform)picker.parts[i].button.transform.parent,0,100+i*55,280,50);
        var details=new GameObject("CharacterDetails",typeof(RectTransform));
        details.transform.SetParent(outer,false);Stretch((RectTransform)details.transform);
        var menu=picker.gameObject.AddComponent<CharacterSetupMenu>();
        menu.picker=picker;menu.characterPanel=details;menu.colorsPanel=colors.gameObject;
        menu.characterTab=Button(outer,"CharacterTab","Character",-72,12,136,34);
        menu.colorsTab=Button(outer,"ColorsTab","Colors",72,12,136,34);
        Label(details.transform,"NameLabel","Name",0,58,280,24);
        var input=UnityEngine.Object.Instantiate(picker.hexInput,details.transform,false);
        input.name="CharacterNameInput";Rect(input.GetComponent<RectTransform>(),0,84,280,34);
        input.onEndEdit=new TMP_InputField.SubmitEvent();input.onValueChanged=new TMP_InputField.OnChangeEvent();
        input.contentType=TMP_InputField.ContentType.Standard;input.characterValidation=TMP_InputField.CharacterValidation.None;
        input.lineType=TMP_InputField.LineType.SingleLine;input.characterLimit=40;input.SetTextWithoutNotify("");
        if(input.placeholder)UnityEngine.Object.DestroyImmediate(input.placeholder.gameObject);
        var placeholder=Label(input.textViewport,"Placeholder","Enter character name",0,0,260,32);
        Stretch(placeholder.rectTransform);placeholder.color=new Color(.4f,.4f,.4f);placeholder.alignment=TextAlignmentOptions.MidlineLeft;
        input.placeholder=placeholder; menu.nameInput=input;
        Label(details.transform,"GenderLabel","Gender",0,128,280,24);
        menu.malePicture=Portrait(picker.maleButton,details.transform,"Male",-70,out menu.malePlaceholder);
        menu.femalePicture=Portrait(picker.femaleButton,details.transform,"Female",70,out menu.femalePlaceholder);
        Label(details.transform,"HairLabel","Hair",0,278,280,24);
        menu.hairButtons=new Button[3];menu.hairPictures=new Image[3];
        menu.hairLabels=new TMP_Text[3];menu.hairPlaceholders=new TMP_Text[3];
        for(int i=0;i<3;i++)
        {
            var b=Button(details.transform,"HairSlot"+i,"",(i-1)*94,307,86,104);
            menu.hairButtons[i]=b;
            UnityEngine.Object.DestroyImmediate(b.GetComponentInChildren<TMP_Text>().gameObject);
            menu.hairPictures[i]=Picture(b.transform,"Thumbnail",66);
            menu.hairLabels[i]=Label(b.transform,"StyleName","Style",0,77,84,24);
            menu.hairLabels[i].fontSize=13;menu.hairLabels[i].color=Color.black;
            menu.hairPlaceholders[i]=Label(b.transform,"Placeholder","Coming\nlater",0,8,80,65);
            menu.hairPlaceholders[i].fontSize=13;menu.hairPlaceholders[i].color=new Color(.25f,.25f,.25f);
        }
        menu.hairStatus=Label(details.transform,"HairStatus","",0,425,280,26);
        menu.hairStatus.fontSize=16;
        menu.hairOptions=new[] {
            Hair("male_short","Short",false,picker.character,"Male_MPFB_Base.short02"),
            Hair("male_medium","Medium",false,null,null),Hair("male_long","Long",false,null,null),
            Hair("female_ponytail","Ponytail",true,picker.femaleCharacter,"Female_MPFB_Base.ponytail01"),
            Hair("female_bob","Bob",true,null,null),Hair("female_long","Long",true,null,null)
        };
        colors.gameObject.SetActive(false);
        picker.pickerPanel.SetActive(false);
        EditorUtility.SetDirty(picker);EditorUtility.SetDirty(menu);
        if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Save failed.");
        File.WriteAllText("Library/CharacterTabsSetup.txt","PASS: Character/Colors tabs, name input, two gender image slots, six hair data entries and three filtered gallery slots saved.");
    }
    static CharacterSetupMenu.HairOption Hair(string id,string name,bool female,Transform root,string mesh)
    {
        return new CharacterSetupMenu.HairOption {id=id,title=name,female=female,
            hairObject=root?root.GetComponentsInChildren<Transform>(true).Single(t=>t.name==mesh).gameObject:null};
    }
    static Image Portrait(Button button,Transform parent,string title,float x,out TMP_Text placeholder)
    {
        button.transform.SetParent(parent,false);Rect((RectTransform)button.transform,x,156,124,108);
        var label=button.GetComponentInChildren<TMP_Text>();Rect(label.rectTransform,0,82,120,24);
        label.text=title;label.fontSize=17;
        placeholder=Label(button.transform,"PortraitPlaceholder","Portrait\npending",0,8,116,72);
        placeholder.color=new Color(.25f,.25f,.25f);placeholder.fontSize=15;
        return Picture(button.transform,"Portrait",72);
    }
    static Image Picture(Transform parent,string name,float size)
    {
        var obj=new GameObject(name,typeof(RectTransform),typeof(Image));obj.transform.SetParent(parent,false);
        Rect((RectTransform)obj.transform,0,6,size,size);
        var image=obj.GetComponent<Image>();image.preserveAspect=true;image.raycastTarget=false;image.enabled=false;
        return image;
    }
    static Button Button(Transform parent,string name,string text,float x,float y,float w,float h)
    {
        var button=UnityEngine.Object.Instantiate(buttonTemplate,parent,false);button.name=name;
        button.onClick=new Button.ButtonClickedEvent();Rect((RectTransform)button.transform,x,y,w,h);
        button.image.color=Color.white;
        var label=button.GetComponentInChildren<TMP_Text>();label.text=text;label.fontSize=18;label.color=Color.black;
        return button;
    }
    static TMP_Text Label(Transform parent,string name,string value,float x,float y,float w,float h)
    {
        var label=UnityEngine.Object.Instantiate(textTemplate,parent,false);label.name=name;label.text=value;
        label.fontSize=18;label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;
        Rect(label.rectTransform,x,y,w,h);return label;
    }
    static void Rect(RectTransform rect,float x,float y,float w,float h)
    {
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);
        rect.anchoredPosition=new Vector2(x,-y);rect.sizeDelta=new Vector2(w,h);
    }
    static void Stretch(RectTransform rect)
    {rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;}
}
