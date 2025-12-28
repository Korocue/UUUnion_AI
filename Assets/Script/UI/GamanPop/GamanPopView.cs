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
    [SerializeField] private Color color0 = Color.yellow; // 0%
    [SerializeField] private Color color50 = Color.red;   // 50%
    [SerializeField] private Color color100 = Color.black;// 100%

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
        var clamped = Mathf.Min(ratio100, 1.0f);

        var scale = Mathf.Max(0.0f, baseScale + Mathf.Sqrt(clamped) * scalePerGaman);
        gamanPop.localScale = Vector3.one * scale;

        ApplyColor(clamped);
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

        var t2 = Mathf.Clamp01((ratio100 - 0.5f) / 0.5f);
        return Color.Lerp(color50, color100, t2);
    }
}
