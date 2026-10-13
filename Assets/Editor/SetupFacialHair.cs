using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time setup for the current CharacterLab scene. Never saves the scene automatically.
public static class SetupFacialHair
{
    const string Root = "Assets/Characters/Customization/";
    const string Report = "Library/FacialHairSetup.txt";

    [MenuItem("Tools/Character Customization/Set Up Eyebrows and Eyelashes")]
    public static void Run()
    {
        try
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.name != "CharacterLab" || EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Open CharacterLab outside Play Mode first.");
            var renderers = scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<Renderer>(true)).ToArray();
            var brows = renderers.Single(x => x.name == "Male_MPFB_Base.eyebrow001");
            var lashes = renderers.Single(x => x.name == "Male_MPFB_Base.eyelashes01");
            var shader = AssetDatabase.LoadAssetAtPath<Shader>(Root + "Shaders/CustomizableHair.shadergraph");
            if (shader == null) throw new InvalidOperationException("CustomizableHair shader not imported yet.");
            var browMaterial = MakeMaterial("CharacterEyebrows", "Eyebrows_Neutral", shader);
            var lashMaterial = MakeMaterial("CharacterEyelashes", "Eyelashes_Neutral", shader);
            Undo.RecordObjects(new UnityEngine.Object[] { brows, lashes }, "Assign facial hair materials");
            brows.sharedMaterial = browMaterial;
            lashes.sharedMaterial = lashMaterial;
            PrefabUtility.RecordPrefabInstancePropertyModifications(brows);
            PrefabUtility.RecordPrefabInstancePropertyModifications(lashes);
            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            if (brows.sharedMaterial != browMaterial || lashes.sharedMaterial != lashMaterial)
                throw new InvalidOperationException("Material assignment verification failed.");
            File.WriteAllText(Report, "SUCCESS: CharacterEyebrows and CharacterEyelashes created and assigned in open CharacterLab. Neutral textures imported with alpha. Shared CustomizableHair shader; independent colour and brightness. Scene marked dirty; save after visual review.");
            Debug.Log("Facial hair setup complete. Review CharacterLab and save the scene.");
        }
        catch (Exception ex)
        {
            File.WriteAllText(Report, "FAILED: " + ex);
            Debug.LogException(ex);
        }
    }

    static Material MakeMaterial(string name, string textureName, Shader shader)
    {
        string texturePath = Root + "Textures/" + textureName + ".png";
        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null) throw new InvalidOperationException("Missing texture: " + texturePath);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = true;
        importer.mipMapsPreserveCoverage = true;
        importer.alphaTestReferenceValue = 0.5f;
        importer.SaveAndReimport();
        string path = Root + "Materials/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material; // Preserve later user tuning.
        material = new Material(shader) { name = name };
        if (!material.HasProperty("_Hair_Texture") || !material.HasProperty("_Hair_Color") || !material.HasProperty("_Hair_Brightness"))
            throw new InvalidOperationException("Hair shader properties do not match the expected graph.");
        material.SetTexture("_Hair_Texture", AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));
        material.SetColor("_Hair_Color", new Color(128f / 255f, 128f / 255f, 128f / 255f, 1));
        material.SetFloat("_Hair_Brightness", 1);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }
}
