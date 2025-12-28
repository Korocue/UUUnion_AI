using UnityEngine;
using UnityEngine.UI;

// Visual update for GamanPop (area + color).
// Spec: Assets/Script/Core/GamanCore.md
public sealed class GamanPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform gamanPop;
    [SerializeField] private SpriteRenderer gamanSprite;
    [SerializeField] private Image gamanImage;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;     // Scale at Gaman=0 (area-based).
    [SerializeField] private float scalePerGaman = 0.5f; // sqrt(Gaman) -> scale.
    [SerializeField] private float gamanPer100 = 1.0f;   // Gaman value that represents 100%.

    [Header("Color Band")]
    [SerializeField] private Color color0 = new Color(0.5f, 0.5f, 0.5f);     // 0% gray (128)
    [SerializeField] private Color color50 = new Color(0.25f, 0.25f, 0.25f); // 50% dark gray (64)
    [SerializeField] private Color color80 = Color.black;                   // 80% black
    [SerializeField] private Color color90 = new Color(0f, 0.25f, 0.25f);    // 90% navy-ish (0,64,64)
    [SerializeField] private Color color100 = Color.red;                    // 100% red
    [SerializeField] private Color color200 = Color.white;                  // 200% white

    private void OnEnable()
    {
        if (gamanPop != null)
        {
            gamanPop.localScale = Vector3.zero;
        }

        ApplyColor(0.0f);
    }

    // Presenter passes Gaman value; this class only updates visuals.
    public void ApplyGaman(double gamanValue)
    {
        if (gamanPop == null)
        {
            return;
        }

        var denom = Mathf.Max(0.0001f, gamanPer100);
        var ratio100 = Mathf.Max(0.0f, (float)gamanValue / denom);
        var scale = Mathf.Max(0.0f, baseScale + Mathf.Sqrt(ratio100) * scalePerGaman);
        gamanPop.localScale = Vector3.one * scale;

        ApplyColor(ratio100);
    }

    private void ApplyColor(float ratio100)
    {
        var color = EvaluateColor(ratio100);

        if (gamanSprite != null)
        {
            gamanSprite.color = color;
        }

        if (gamanImage != null)
        {
            gamanImage.color = color;
        }
    }

    private Color EvaluateColor(float ratio100)
    {
        if (ratio100 <= 0.5f)
        {
            var t = ratio100 / 0.5f;
            return Color.Lerp(color0, color50, t);
        }

        if (ratio100 <= 0.8f)
        {
            var t = (ratio100 - 0.5f) / 0.3f;
            return Color.Lerp(color50, color80, t);
        }

        if (ratio100 <= 0.9f)
        {
            var t = (ratio100 - 0.8f) / 0.1f;
            return Color.Lerp(color80, color90, t);
        }

        if (ratio100 <= 1.0f)
        {
            var t = (ratio100 - 0.9f) / 0.1f;
            return Color.Lerp(color90, color100, t);
        }

        if (ratio100 <= 2.0f)
        {
            var t = ratio100 - 1.0f;
            return Color.Lerp(color100, color200, t);
        }

        return color200;
    }
}
