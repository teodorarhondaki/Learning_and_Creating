using System.IO;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CaptureHairHandoff
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/CharacterLab.unity");
        var picker=Object.FindFirstObjectByType<CharacterColorPicker>();
        var male=picker.character;var female=picker.femaleCharacter;
        foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include,FindObjectsSortMode.None))canvas.gameObject.SetActive(false);
        var camera=Camera.main;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.18f,.2f,.24f);
        camera.fieldOfView=32;camera.nearClipPlane=.01f;
        var output=@"H:\ChatGPT - IT\CharacterSource\HairstyleHandoff-2026-10-16\Reference";
        Directory.CreateDirectory(output);
        foreach(bool isFemale in new[]{false,true})
        {
            male.gameObject.SetActive(!isFemale);female.gameObject.SetActive(isFemale);
            var target=isFemale?female:male;
            var eyes=target.GetComponentsInChildren<Renderer>().Single(r=>r.name.EndsWith(".low-poly"));
            var center=eyes.bounds.center-Vector3.up*.04f;
            var directions=new[]{Vector3.back,Vector3.right,Vector3.forward};
            var views=new[]{"front","side","back"};
            for(int i=0;i<3;i++)
            {
                camera.transform.position=center+directions[i]*.95f;
                camera.transform.LookAt(center);
                Save(camera,Path.Combine(output,(isFemale?"female_":"male_")+views[i]+".png"));
            }
        }
    }
    static void Save(Camera camera,string path)
    {
        var rt=new RenderTexture(1024,1024,24);camera.targetTexture=rt;
        var previous=RenderTexture.active;
        camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(1024,1024,TextureFormat.RGB24,false);
        image.ReadPixels(new Rect(0,0,1024,1024),0,0);image.Apply();
        File.WriteAllBytes(path,image.EncodeToPNG());
        camera.targetTexture=null;RenderTexture.active=previous;
        rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);
    }
}
