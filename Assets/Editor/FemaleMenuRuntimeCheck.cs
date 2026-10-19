using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FemaleMenuRuntimeCheck
{
    const string Key="FemaleMenuRuntimeCheck.Pending";
    static int frames;
    static double deadline;
    static FemaleMenuRuntimeCheck()
    {
        deadline=EditorApplication.timeSinceStartup+90;
        EditorApplication.update+=Tick;
    }
    public static void Begin()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        SessionState.SetBool(Key,true);
        EditorApplication.EnterPlaymode();
    }
    static void Require(bool condition,string message)
    { if(!condition) throw new Exception(message); }
    static bool Same(Color a,Color b) => Mathf.Abs(a.r-b.r)+Mathf.Abs(a.g-b.g)+Mathf.Abs(a.b-b.b)<.005f;
    static void Tick()
    {
        if(!SessionState.GetBool(Key,false))return;
        if(EditorApplication.timeSinceStartup>deadline) { Finish(false,"Play Mode timed out."); return; }
        if(!EditorApplication.isPlaying || ++frames<20)return;
        try
        {
            var p=UnityEngine.Object.FindFirstObjectByType<CharacterColorPicker>();
            var menu=p.GetComponent<CharacterSetupMenu>();
            if(menu)
            {
                Require(menu.characterPanel.activeSelf && !menu.colorsPanel.activeSelf,"Initial tab");
                menu.nameInput.text="Alex Жан";
                Require(menu.CharacterName=="Alex Жан","Name input");
                Require(menu.SelectedHairId=="male_short","Initial male hair");
                menu.ChooseHair(1);
                Require(menu.SelectedHairId=="male_short","Placeholder selected");
                Require(!menu.hairButtons[1].interactable && !menu.hairButtons[2].interactable,"Placeholders enabled");
                Require(menu.hairButtons.Length==6 && menu.hairOptions.Length==12,"Expanded gallery size");
                for(int slot=3;slot<6;slot++)
                {
                    Require(menu.hairLabels[slot].text==new[]{"Wavy","Curly","Straight"}[slot-3],"Male style label");
                    Require(!menu.hairButtons[slot].interactable,"Future style enabled");
                    menu.ChooseHair(slot);Require(menu.SelectedHairId=="male_short","Future style changed hair");
                }
                Capture(p,"character-tab.png");
                var gallery=menu.characterPanel.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
                if(gallery)
                {
                    Canvas.ForceUpdateCanvases();
                    Require(gallery.content.rect.height>gallery.viewport.rect.height,"Gallery does not overflow");
                    gallery.verticalNormalizedPosition=1;
                    gallery.OnScroll(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){scrollDelta=new Vector2(0,-2)});
                    Require(gallery.verticalNormalizedPosition<.99f,"Mouse wheel did not scroll");
                    gallery.verticalScrollbar.value=0;
                    Require(gallery.verticalNormalizedPosition<.01f,"Scrollbar did not reach bottom");
                    Capture(p,"hair-gallery-scrolled.png");
                    gallery.verticalNormalizedPosition=1;
                }
                menu.colorsTab.onClick.Invoke();
                Require(menu.colorsPanel.activeSelf && !menu.characterPanel.activeSelf,"Colors tab");
                p.Open(0);
                menu.characterTab.onClick.Invoke();
                Require(!p.pickerPanel.activeSelf,"Picker leaked across tabs");
                p.femaleButton.onClick.Invoke();
                Require(menu.IsFemale && menu.SelectedHairId=="female_ponytail","Female gallery");
                Require(menu.hairLabels[0].text=="Ponytail","Gallery label");
                for(int slot=3;slot<6;slot++)
                {
                    Require(menu.hairLabels[slot].text==new[]{"Wavy","Curly","Straight"}[slot-3],"Female style label");
                    menu.ChooseHair(slot);Require(menu.SelectedHairId=="female_ponytail","Future female style changed hair");
                }
                menu.colorsTab.onClick.Invoke();
                Require(menu.CharacterName=="Alex Жан","Name lost across tabs");
                Capture(p,"colors-tab.png");
                p.maleButton.onClick.Invoke();
            }
            var male=p.parts;
            var female=p.femaleParts;
            var originalMale=new Color[6]; var originalFemale=new Color[6];
            for(int i=0;i<6;i++)
            {
                originalMale[i]=male[i].source.GetColor(male[i].colorProperty);
                originalFemale[i]=female[i].source.GetColor(female[i].colorProperty);
                Require(male[i].instance!=female[i].instance,"Shared runtime material across characters");
            }
            Require(male[0].instance==male[1].instance && female[0].instance==female[1].instance,"Skin/lip material split");
            p.femaleButton.onClick.Invoke();
            Require(p.character==p.femaleCharacter && p.character.gameObject.activeSelf,"Female selector");
            for(int i=0;i<6;i++)
            {
                p.parts[i].button.onClick.Invoke();
                Require(p.pickerTitle.text==female[i].title+" Color","Wrong category listener");
                p.hexInput.onEndEdit.Invoke("#336699");
                var expected=new Color(.2f,.4f,.6f,1);
                Require(Same(female[i].instance.GetColor(female[i].colorProperty),expected),"Female hex target "+i);
                Require(Same(p.parts[i].button.image.color,expected),"Swatch mismatch");
                p.red.value=.7f;
                Require(Mathf.Abs(female[i].instance.GetColor(female[i].colorProperty).r-.7f)<.001f,"RGB target");
                p.visualPicker.PickHue(.5f); p.visualPicker.PickShade(.8f,.7f);
                expected=Color.HSVToRGB(.5f,.8f,.7f);
                Require(Same(female[i].instance.GetColor(female[i].colorProperty),expected),"HSV target");
                Require(p.hexInput.text=="#"+ColorUtility.ToHtmlStringRGB(expected),"HSV hex sync");
                Require(Same(male[i].instance.GetColor(male[i].colorProperty),originalMale[i]),"Female changed male");
                if(i!=2) { p.brightness.value=1.7f; Require(Mathf.Abs(female[i].instance.GetFloat(female[i].brightnessProperty)-1.7f)<.001f,"Brightness target"); }
                else Require(!p.brightness.interactable,"Eye brightness enabled");
            }
            for(int repeat=0;repeat<3;repeat++)
            {
                p.maleButton.onClick.Invoke();
                Require(!p.pickerPanel.activeSelf && !p.femaleCharacter.gameObject.activeSelf,"Stale picker/visibility");
                p.ApplyHex("FFFFFF");
                for(int i=0;i<6;i++)
                {
                    p.parts[i].button.onClick.Invoke();
                    p.ApplyHex("AA5533");
                    Require(Same(male[i].instance.GetColor(male[i].colorProperty),new Color(170f/255,85f/255,51f/255,1)),"Male target after switch");
                }
                p.femaleButton.onClick.Invoke();
                for(int i=0;i<6;i++) Require(Same(female[i].instance.GetColor(female[i].colorProperty),Color.HSVToRGB(.5f,.8f,.7f)),"Female choice lost");
            }
            for(int i=0;i<6;i++)
            {
                Require(Same(male[i].source.GetColor(male[i].colorProperty),originalMale[i]),"Male source modified");
                Require(Same(female[i].source.GetColor(female[i].colorProperty),originalFemale[i]),"Female source modified");
            }
            p.Open(0); p.closeButton.onClick.Invoke(); Require(!p.pickerPanel.activeSelf,"Done failed");
            Finish(true,"PASS: real Play Mode; Character/Colors tabs, name with Unicode, gender-filtered gallery, disabled placeholders, picker closes on tab switch, selected hair retained; six female targets via buttons, hex, RGB, HSV and brightness; swatch/hex sync; three switch roundtrips; male edits; independent retained colors; source materials unchanged; Done. Physical pointer review and save/load persistence not tested. See captured tab images.");
        }
        catch(Exception ex){Finish(false,ex.ToString());}
    }
    static void Finish(bool success,string report)
    {
        SessionState.SetBool(Key,false);
        File.WriteAllText("Library/FemaleMenuRuntimeCheck.txt",report);
        EditorApplication.Exit(success?0:1);
    }

    static void Capture(CharacterColorPicker picker,string name)
    {
        var canvas=picker.GetComponent<Canvas>();
        var camera=Camera.main;
        var previousMode=canvas.renderMode;
        var previousCamera=canvas.worldCamera;
        var previousDistance=canvas.planeDistance;
        var previousTarget=camera.targetTexture;
        var previousActive=RenderTexture.active;
        var rt=new RenderTexture(1280,720,24);
        try
        {
            camera.targetTexture=rt;
            canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
            Canvas.ForceUpdateCanvases();
            camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
            File.WriteAllBytes(@"H:\ChatGPT - IT\CharacterSource\FemaleCustomization\Review\"+name,image.EncodeToPNG());
            UnityEngine.Object.Destroy(image);
        }
        finally
        {
            canvas.renderMode=previousMode;canvas.worldCamera=previousCamera;canvas.planeDistance=previousDistance;
            camera.targetTexture=previousTarget;RenderTexture.active=previousActive;rt.Release();UnityEngine.Object.Destroy(rt);
        }
    }
}
