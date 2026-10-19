using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ExpandHairGallery
{
    public static void AddScrolling()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        var menu=UnityEngine.Object.FindFirstObjectByType<CharacterSetupMenu>();
        if(menu.characterPanel.transform.Find("HairGallery"))throw new InvalidOperationException("Gallery already scrolls.");
        var gallery=new GameObject("HairGallery",typeof(RectTransform),typeof(Image),typeof(ScrollRect));
        gallery.transform.SetParent(menu.characterPanel.transform,false);
        var rect=(RectTransform)gallery.transform;
        rect.anchorMin=rect.anchorMax=new Vector2(.5f,1);rect.pivot=new Vector2(.5f,1);
        rect.anchoredPosition=new Vector2(0,-303);rect.sizeDelta=new Vector2(280,130);
        gallery.GetComponent<Image>().color=new Color(0,0,0,.08f);
        var viewport=new GameObject("Viewport",typeof(RectTransform),typeof(RectMask2D));
        viewport.transform.SetParent(gallery.transform,false);
        var vr=(RectTransform)viewport.transform;vr.anchorMin=Vector2.zero;vr.anchorMax=Vector2.one;vr.offsetMin=vr.offsetMax=Vector2.zero;
        var content=new GameObject("Content",typeof(RectTransform));content.transform.SetParent(viewport.transform,false);
        var cr=(RectTransform)content.transform;cr.anchorMin=new Vector2(0,1);cr.anchorMax=Vector2.one;cr.pivot=new Vector2(.5f,1);
        cr.sizeDelta=new Vector2(0,164);cr.anchoredPosition=Vector2.zero;
        for(int i=0;i<menu.hairButtons.Length;i++)
        {
            var card=(RectTransform)menu.hairButtons[i].transform;card.SetParent(cr,false);
            card.anchoredPosition=new Vector2((i%3-1)*94,-(i/3*82));
        }
        var barObj=new GameObject("HairScrollbar",typeof(RectTransform),typeof(Image),typeof(Scrollbar));
        barObj.transform.SetParent(gallery.transform,false);
        var br=(RectTransform)barObj.transform;br.anchorMin=new Vector2(1,0);br.anchorMax=Vector2.one;br.pivot=new Vector2(0,.5f);
        br.anchoredPosition=new Vector2(4,0);br.sizeDelta=new Vector2(10,0);
        barObj.GetComponent<Image>().color=new Color(.25f,.28f,.32f);
        var handle=new GameObject("Handle",typeof(RectTransform),typeof(Image));handle.transform.SetParent(br,false);
        var hr=(RectTransform)handle.transform;hr.anchorMin=Vector2.zero;hr.anchorMax=Vector2.one;hr.offsetMin=hr.offsetMax=Vector2.zero;
        handle.GetComponent<Image>().color=new Color(.7f,.75f,.82f);
        var bar=barObj.GetComponent<Scrollbar>();bar.handleRect=hr;bar.targetGraphic=handle.GetComponent<Image>();bar.direction=Scrollbar.Direction.BottomToTop;
        var scroll=gallery.GetComponent<ScrollRect>();scroll.viewport=vr;scroll.content=cr;scroll.horizontal=false;scroll.vertical=true;
        scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=22;scroll.verticalScrollbar=bar;
        scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;scroll.verticalNormalizedPosition=1;
        menu.hairStatus.rectTransform.anchoredPosition=new Vector2(0,-446);
        EditorSceneManager.SaveScene(scene);
    }
    public static void Run()
    {
        const string path="Assets/Scenes/CharacterLab.unity";
        const string backup=@"H:\ChatGPT - IT\CharacterSource\FemaleCustomization\CharacterLab-before-more-hair.unity";
        if(!File.Exists(backup))File.Copy(path,backup);
        var scene=EditorSceneManager.OpenScene(path);
        var menu=UnityEngine.Object.FindFirstObjectByType<CharacterSetupMenu>();
        var options=menu.hairOptions.ToList();
        foreach(bool female in new[]{false,true})
        foreach(string style in new[]{"Wavy","Curly","Straight"})
        {
            string id=(female?"female_":"male_")+style.ToLowerInvariant();
            if(!options.Any(o=>o.id==id))options.Add(new CharacterSetupMenu.HairOption{id=id,title=style,female=female});
        }
        menu.hairOptions=options.OrderBy(o=>o.female).ToArray();
        var buttons=menu.hairButtons.ToList();
        while(buttons.Count<6)
        {
            var button=UnityEngine.Object.Instantiate(buttons[0],buttons[0].transform.parent,false);
            button.name="HairSlot"+buttons.Count;button.onClick=new Button.ButtonClickedEvent();buttons.Add(button);
        }
        menu.hairButtons=buttons.ToArray();
        menu.hairPictures=new Image[6];menu.hairLabels=new TMP_Text[6];menu.hairPlaceholders=new TMP_Text[6];
        for(int i=0;i<6;i++)
        {
            var button=buttons[i];var rect=(RectTransform)button.transform;
            rect.anchoredPosition=new Vector2((i%3-1)*94,-(303+i/3*82));rect.sizeDelta=new Vector2(86,76);
            var picture=button.transform.Find("Thumbnail").GetComponent<Image>();
            picture.rectTransform.sizeDelta=new Vector2(46,46);
            var label=button.transform.Find("StyleName").GetComponent<TMP_Text>();
            label.rectTransform.anchoredPosition=new Vector2(0,-53);label.rectTransform.sizeDelta=new Vector2(84,22);
            var placeholder=button.transform.Find("Placeholder").GetComponent<TMP_Text>();
            placeholder.rectTransform.sizeDelta=new Vector2(80,44);placeholder.fontSize=12;
            menu.hairPictures[i]=picture;menu.hairLabels[i]=label;menu.hairPlaceholders[i]=placeholder;
        }
        menu.hairStatus.rectTransform.anchoredPosition=new Vector2(0,-469);
        EditorUtility.SetDirty(menu);
        if(!EditorSceneManager.SaveScene(scene))throw new InvalidOperationException("Save failed.");
        File.WriteAllText("Library/ExpandedHairGallery.txt","PASS: six slots per gender, twelve entries total; existing styles preserved; Wavy/Curly/Straight placeholders added.");
    }
}
