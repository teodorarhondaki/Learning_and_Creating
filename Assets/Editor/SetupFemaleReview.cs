using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class SetupFemaleReview
{
    const string Root = "Assets/Characters/Customization/";
    const string ScenePath = "Assets/Scenes/FemaleCharacterReview.unity";
    const string PrefabPath = "Assets/Prefabs/FemaleCharacter.prefab";
    const string Output = @"H:\ChatGPT - IT\CharacterSource\FemaleCustomization\Review";

    public static void Run()
    {
        Directory.CreateDirectory(Output);
        if (File.Exists(ScenePath) || File.Exists(PrefabPath))
            throw new InvalidOperationException("Review assets already exist; preserving them.");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var source = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Models/FemaleCharacter_Base.fbx");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        instance.name = "FemaleCharacter";
        var mapping = new Dictionary<string, string> {
            {"Female_MPFB_Base", "FemaleSkin"}, {"Female_MPFB_Base.low-poly", "FemaleEyes"},
            {"Female_MPFB_Base.ponytail01", "FemaleHair"}, {"Female_MPFB_Base.eyebrow001", "FemaleEyebrows"},
            {"Female_MPFB_Base.eyelashes01", "FemaleEyelashes"}, {"Female_MPFB_Base.female_casualsuit01", "FemaleClothing"}
        };
        var renderers = instance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length != mapping.Count) throw new InvalidOperationException("Unexpected renderer count.");
        var report = new List<string>();
        foreach (var renderer in renderers)
        {
            if (!mapping.TryGetValue(renderer.name, out string name)) throw new InvalidOperationException(renderer.name);
            var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/" + name + ".mat");
            if (material == null || ShaderUtil.ShaderHasError(material.shader)) throw new InvalidOperationException(name);
            renderer.sharedMaterial = material;
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            report.Add(renderer.name + " -> " + material.name + "; bounds " + renderer.bounds);
        }
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        PrefabUtility.SaveAsPrefabAssetAndConnect(instance, PrefabPath, InteractionMode.AutomatedAction);
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(.5f,.53f,.58f);
        RenderSettings.ambientEquatorColor = new Color(.4f,.4f,.4f);
        RenderSettings.ambientGroundColor = new Color(.25f,.23f,.22f);
        var light = new GameObject("Review Key Light").AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(35,155,0);
        var camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.18f,.2f,.24f);
        camera.nearClipPlane = .01f;
        camera.farClipPlane = 100;
        camera.fieldOfView = 35;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;
        var eyes = renderers.Single(r => r.name.EndsWith(".low-poly")).bounds;
        var head = eyes.center + Vector3.up * bounds.size.y * .025f;
        Aim(camera, head, new Vector3(0,0,1), bounds.size.y * .55f);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Capture(camera, renderers, bounds, head);
        report.Add("Six explicit renderer/material assignments verified. Five rendered views saved. Review scene/prefab created; existing CharacterLab and menu untouched. Animation not tested.");
        File.WriteAllLines(Output + "/assignment-check.txt", report);
    }

    public static void CaptureSaved()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var camera = UnityEngine.Object.FindFirstObjectByType<Camera>();
        UnityEngine.Object.FindFirstObjectByType<Light>().transform.rotation = Quaternion.Euler(35,155,0);
        var renderers = GameObject.Find("FemaleCharacter").GetComponentsInChildren<Renderer>(true);
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        var head = renderers.Single(r => r.name.EndsWith(".low-poly")).bounds.center + Vector3.up * bounds.size.y * .025f;
        Aim(camera, head, new Vector3(0,0,1), bounds.size.y * .55f);
        EditorSceneManager.SaveScene(scene, ScenePath);
        Capture(camera, renderers, bounds, head);
    }

    static void Capture(Camera camera, Renderer[] renderers, Bounds bounds, Vector3 head)
    {
        Render(camera, "01-front.png");
        Aim(camera, head, new Vector3(.8f,.08f,1), bounds.size.y * .6f);
        Render(camera, "02-three-quarter.png");
        Aim(camera, head, new Vector3(.8f,.1f,-1), bounds.size.y * .65f);
        Render(camera, "03-back.png");
        Aim(camera, bounds.center, new Vector3(0,0,1), bounds.size.y * 1.8f);
        Render(camera, "04-full.png");
        // Diagnostic tints use temporary material copies and never touch saved material assets.
        var originals = renderers.Select(r => r.sharedMaterial).ToArray();
        try
        {
            foreach (var renderer in renderers)
            {
                var material = new Material(renderer.sharedMaterial);
                renderer.sharedMaterial = material;
                if (material.HasProperty("_Hair_Color")) material.SetColor("_Hair_Color", Color.white);
                if (material.HasProperty("_Iris_Color")) material.SetColor("_Iris_Color", Color.blue);
                if (material.HasProperty("_Lips_Color")) material.SetColor("_Lips_Color", Color.red);
            }
            Aim(camera, head, new Vector3(0,0,1), bounds.size.y * .55f);
            Render(camera, "05-tint-check.png");
        }
        finally
        {
            for (int i=0;i<renderers.Length;i++)
            {
                var temporary = renderers[i].sharedMaterial;
                renderers[i].sharedMaterial = originals[i];
                UnityEngine.Object.DestroyImmediate(temporary);
            }
        }
    }

    static void Aim(Camera camera, Vector3 target, Vector3 direction, float distance)
    {
        camera.transform.position = target + direction.normalized * distance;
        camera.transform.LookAt(target);
    }

    static void Render(Camera camera, string name)
    {
        var target = new RenderTexture(640,640,24);
        var previous = RenderTexture.active;
        camera.targetTexture = target;
        try
        {
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(640,640,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,640,640),0,0);
            image.Apply();
            File.WriteAllBytes(Output + "/" + name,image.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(image);
        }
        finally
        {
            camera.targetTexture = null;
            RenderTexture.active = previous;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
