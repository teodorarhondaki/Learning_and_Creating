using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Asset preparation only: never opens, edits, or saves a scene.
public static class SetupFemaleMaterials
{
    const string Root = "Assets/Characters/Customization/";
    const string Source = @"H:\ChatGPT_Bible_Game\Blender\User\extensions\.user\user_default\mpfb\data\";
    static readonly List<string> Report = new List<string>();

    [MenuItem("Tools/Character Customization/Prepare Female Materials")]
    public static void Run()
    {
        Report.Clear();
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Exit Play Mode before preparing assets.");
            string[] names = { "FemaleSkin", "FemaleEyes", "FemaleHair", "FemaleEyebrows", "FemaleEyelashes", "FemaleClothing" };
            foreach (string name in names)
                if (File.Exists(Root + "Materials/" + name + ".mat"))
                    throw new InvalidOperationException("Already exists; preserving user tuning: " + name);
            foreach (string name in new[] { "FemaleSkin_TintBase", "FemaleSkin_LipMask", "FemaleHair_Neutral" })
                if (File.Exists(Root + "Textures/" + name + ".png"))
                    throw new InvalidOperationException("Already exists; preserving texture: " + name);

            float ignored;
            var skinTexture = Neutral(Source + @"skins\young_caucasian_female\young_lightskinned_female_diffuse.png", "FemaleSkin_TintBase", false, out ignored);
            var hairTexture = Neutral(Source + @"hair\ponytail01\ponytail01_diffuse.png", "FemaleHair_Neutral", true, out float exponent);
            var mask = LipMask();
            var skin = Clone("Materials/CharacterSkin.mat", "FemaleSkin");
            skin.SetTexture("_Skin_Texture", skinTexture);
            skin.SetTexture("_Lip_Mask", mask);
            var eyes = Clone("Materials/CharacterEyes.mat", "FemaleEyes");
            var hair = Clone("Materials/CharacterHair.mat", "FemaleHair");
            hair.SetTexture("_Hair_Texture", hairTexture);
            hair.SetFloat("_Texture_Exponent", exponent);
            Clone("Materials/CharacterEyebrows.mat", "FemaleEyebrows");
            Clone("Materials/CharacterEyelashes.mat", "FemaleEyelashes");
            var lit = Shader.Find("Universal Render Pipeline/Lit");
            if (lit == null) throw new InvalidOperationException("URP Lit unavailable.");
            var clothes = new Material(lit) { name = "FemaleClothing" };
            // Original Blender diffuse is linear; Unity's color input is sRGB.
            clothes.SetColor("_BaseColor", new Color(.7f, .66f, .52f, 1).gamma);
            clothes.SetFloat("_Smoothness", .2f);
            AssetDatabase.CreateAsset(clothes, Root + "Materials/FemaleClothing.mat");
            foreach (string name in names)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/" + name + ".mat");
                EditorUtility.SetDirty(material);
                if (material.shader == null || ShaderUtil.ShaderHasError(material.shader))
                    throw new InvalidOperationException("Shader error on " + name);
                Report.Add(name + ": " + material.shader.name);
            }
            AssetDatabase.SaveAssets();
            // Verify the relevant texture/property links before handing off for visual review.
            if (skin.GetTexture("_Skin_Texture") != skinTexture || skin.GetTexture("_Lip_Mask") != mask ||
                hair.GetTexture("_Hair_Texture") != hairTexture || !eyes.HasProperty("_Iris_Color"))
                throw new InvalidOperationException("Material reference validation failed.");
            var brows = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/FemaleEyebrows.mat");
            if (!Mathf.Approximately(brows.GetFloat("_Alpha_Cutoff"), .25f))
                throw new InvalidOperationException("Unexpected eyebrow cutoff.");
            Report.Add("PASS: six independent materials, required links and shaders checked. Scene and male assets untouched. Model appearance pending stage 4.");
            File.WriteAllLines("Library/FemaleMaterialsCheck.txt", Report);
        }
        catch (Exception ex)
        {
            Report.Add("FAILED: " + ex);
            File.WriteAllLines("Library/FemaleMaterialsCheck.txt", Report);
            throw;
        }
    }

    static Material Clone(string source, string name)
    {
        var original = AssetDatabase.LoadAssetAtPath<Material>(Root + source);
        if (original == null) throw new InvalidOperationException("Missing " + source);
        var copy = new Material(original) { name = name };
        AssetDatabase.CreateAsset(copy, Root + "Materials/" + name + ".mat");
        return copy;
    }

    static Texture2D Load(string path)
    {
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!texture.LoadImage(File.ReadAllBytes(path))) throw new InvalidOperationException("Invalid PNG: " + path);
        return texture;
    }

    static Texture2D Neutral(string source, string name, bool cutout, out float exponent)
    {
        var input = Load(source);
        var pixels = input.GetPixels32();
        var originalAlpha = pixels.Select(p => p.a).ToArray();
        var visibleLuminance = new List<float>();
        for (int i = 0; i < pixels.Length; i++)
        {
            Color linear = ((Color)pixels[i]).linear;
            float luminance = .2126f * linear.r + .7152f * linear.g + .0722f * linear.b;
            byte gray = (byte)Mathf.RoundToInt(Mathf.LinearToGammaSpace(luminance) * 255);
            pixels[i] = new Color32(gray, gray, gray, pixels[i].a);
            if (pixels[i].a >= 128 && luminance > .00001f) visibleLuminance.Add(luminance);
        }
        visibleLuminance.Sort();
        float median = visibleLuminance[visibleLuminance.Count / 2];
        exponent = cutout ? Mathf.Clamp(Mathf.Log(.64f) / Mathf.Log(median), .05f, 1) : 1;
        input.SetPixels32(pixels);
        string path = Root + "Textures/" + name + ".png";
        File.WriteAllBytes(path, input.EncodeToPNG());
        var verify = Load(path);
        var actual = verify.GetPixels32();
        if (actual.Length != pixels.Length || actual.Where((p, i) => p.a != originalAlpha[i] || p.r != p.g || p.g != p.b).Any())
            throw new InvalidOperationException("Neutral texture verification failed.");
        Report.Add(name + ": " + input.width + "x" + input.height + ", grayscale and exact alpha verified; linear median=" + median + ", exponent=" + exponent);
        UnityEngine.Object.DestroyImmediate(input);
        UnityEngine.Object.DestroyImmediate(verify);
        return Import(path, true, cutout);
    }

    static Texture2D LipMask()
    {
        var input = Load(Root + "Textures/MaleSkin_LipMask.png");
        if (input.width != 2048 || input.height != 2048) throw new InvalidOperationException("Unexpected mask size.");
        var pixels = input.GetPixels32();
        int retained = 0;
        for (int y = 0; y < input.height; y++)
        for (int x = 0; x < input.width; x++)
        {
            int i = y * input.width + x;
            // Top-origin image region inspected against the female diffuse mouth.
            int topY = input.height - 1 - y;
            byte value = x >= 1800 && x <= 1910 && topY >= 990 && topY <= 1130 ? pixels[i].r : (byte)0;
            pixels[i] = new Color32(value, value, value, 255);
            if (value > 0) retained++;
        }
        if (retained < 1000) throw new InvalidOperationException("Lip region empty or unexpected.");
        input.SetPixels32(pixels);
        string path = Root + "Textures/FemaleSkin_LipMask.png";
        File.WriteAllBytes(path, input.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(input);
        Report.Add("Female lip mask: mouth region retained; stray marks outside it removed; " + retained + " nonzero pixels. Model-level edge review pending.");
        return Import(path, false, false);
    }

    static Texture2D Import(string path, bool srgb, bool cutout)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Default;
        importer.sRGBTexture = srgb;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = cutout;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = true;
        importer.mipMapsPreserveCoverage = cutout;
        importer.alphaTestReferenceValue = .5f;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
