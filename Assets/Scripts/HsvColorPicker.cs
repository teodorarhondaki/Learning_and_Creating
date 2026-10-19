using UnityEngine;
using UnityEngine.UI;

public sealed class HsvColorPicker : MonoBehaviour
{
    public CharacterColorPicker controller;
    public RawImage square, strip;
    public RectTransform shadeMarker, hueMarker;
    Texture2D squareTexture, hueTexture;
    float hue, saturation, value;
    bool applying;
    const int Resolution = 128;

    void EnsureTextures()
    {
        if (squareTexture) return;
        squareTexture = NewTexture(Resolution, Resolution);
        hueTexture = NewTexture(1, Resolution);
        var pixels = new Color[Resolution];
        for (int y = 0; y < Resolution; y++) pixels[y] = Color.HSVToRGB(y / (float)(Resolution - 1), 1, 1);
        hueTexture.SetPixels(pixels); hueTexture.Apply(false);
        square.texture = squareTexture; strip.texture = hueTexture;
        DrawSquare();
    }
    static Texture2D NewTexture(int width, int height) => new Texture2D(width, height, TextureFormat.RGBA32, false)
        { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, hideFlags = HideFlags.DontSave };

    public void SetColorWithoutNotify(Color color)
    {
        EnsureTextures();
        if (applying) return; // Keep the hue handle stable at red and at grayscale endpoints.
        Color.RGBToHSV(color, out float h, out float s, out float v);
        if (s > .0001f && v > .0001f) hue = h;
        saturation = s; value = v;
        DrawSquare(); MoveMarkers();
    }
    public void PickHue(float position)
    {
        EnsureTextures(); hue = Mathf.Clamp01(position);
        DrawSquare(); Apply();
    }
    public void PickShade(float s, float v)
    {
        EnsureTextures(); saturation = Mathf.Clamp01(s); value = Mathf.Clamp01(v);
        Apply();
    }
    void Apply()
    {
        MoveMarkers();
        applying = true;
        try { controller.ApplyPickedColor(Color.HSVToRGB(hue, saturation, value)); }
        finally { applying = false; }
    }
    void DrawSquare()
    {
        var pixels = new Color[Resolution * Resolution];
        for (int y = 0; y < Resolution; y++)
            for (int x = 0; x < Resolution; x++)
                pixels[y * Resolution + x] = Color.HSVToRGB(hue, x / (float)(Resolution - 1), y / (float)(Resolution - 1));
        squareTexture.SetPixels(pixels); squareTexture.Apply(false);
    }
    void MoveMarkers()
    {
        shadeMarker.anchorMin = shadeMarker.anchorMax = new Vector2(saturation, value);
        shadeMarker.anchoredPosition = Vector2.zero;
        hueMarker.anchorMin = hueMarker.anchorMax = new Vector2(.5f, hue);
        hueMarker.anchoredPosition = Vector2.zero;
    }
    void OnDestroy()
    {
        if (squareTexture) Destroy(squareTexture);
        if (hueTexture) Destroy(hueTexture);
    }
}
