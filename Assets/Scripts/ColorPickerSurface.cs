using UnityEngine;
using UnityEngine.EventSystems;

// Converts screen coordinates through the Canvas camera, including scaled canvases.
public sealed class ColorPickerSurface : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public HsvColorPicker picker;
    public bool hueStrip;
    public void OnPointerDown(PointerEventData e) { Pick(e); }
    public void OnDrag(PointerEventData e) { Pick(e); }
    void Pick(PointerEventData e)
    {
        if (e.button != PointerEventData.InputButton.Left) return;
        var rect = (RectTransform)transform;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var point)) return;
        float x = Mathf.InverseLerp(rect.rect.xMin, rect.rect.xMax, point.x);
        float y = Mathf.InverseLerp(rect.rect.yMin, rect.rect.yMax, point.y);
        if (hueStrip) picker.PickHue(y); else picker.PickShade(x, y);
    }
}
