using UnityEngine;
using UnityEngine.UI;

// このファイルは MixResistPop の見た目（面積と色帯）だけを更新する。
// 仕様: Assets/Script/Core/MixResist.md
public sealed class MixResistPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform mixResistPop;
    [SerializeField] private SpriteRenderer mixResistSprite;
    [SerializeField] private Image mixResistImage;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;    // 値が0のときのスケール（sqrt(面積)）。
    [InspectorName("Insp Area Sqrt")]
    [Tooltip("Insp面積の係数の平方根。最終スケールは sqrt( base^2 + inspArea + gamanArea + correction )。")]
    [SerializeField] private float scalePerInsp = 0.5f;
    [InspectorName("Gaman Area Sqrt")]
    [Tooltip("Gaman面積の係数の平方根。最終スケールは面積合算後に sqrt で算出。")]
    [SerializeField] private float scalePerGaman = 0.5f;
    [SerializeField] private float inspPer100 = 1.0f;   // 100% を表す Insp 値。
    [SerializeField] private float gamanPer100 = 1.0f;  // 100% を表す Gaman 値。

    [Header("Color Band")]
    [SerializeField] private Color color0 = Color.white;   // 0%
    [SerializeField] private Color color25 = Color.yellow; // 25%
    [SerializeField] private Color color50 = Color.red;    // 50%
    [SerializeField] private Color color100 = Color.black; // 100%

    private void OnEnable()
    {
        if (mixResistPop != null)
        {
            mixResistPop.localScale = Vector3.zero;
        }

        ApplyColor(0.0f);
    }

    // Presenter から Insp/Gaman/MixResistance を受け取り、見た目だけ更新する。
    public void ApplyMixResist(double insp, double gamanValue, double mixResistance)
    {
        if (mixResistPop == null)
        {
            return;
        }

        var safeInsp = Mathf.Max(0.0f, (float)insp);
        var inspDenom = Mathf.Max(0.0001f, inspPer100);
        var inspRatio = Mathf.Max(0.0f, safeInsp / inspDenom);
        var inspArea = inspRatio * scalePerInsp * scalePerInsp;

        var safeGaman = Mathf.Max(0.0f, (float)gamanValue);
        var gamanDenom = Mathf.Max(0.0001f, gamanPer100);
        var gamanRatio = Mathf.Max(0.0f, safeGaman / gamanDenom);
        var gamanArea = gamanRatio * scalePerGaman * scalePerGaman;

        var correction = Mathf.Clamp01((100.0f - (float)mixResistance) / 100.0f);
        var baseArea = baseScale * baseScale;
        var totalArea = Mathf.Max(0.0f, baseArea + inspArea + gamanArea + correction);
        var scale = Mathf.Sqrt(totalArea);
        mixResistPop.localScale = Vector3.one * scale;

        ApplyColor(correction);
    }

    private void ApplyColor(float ratio100)
    {
        var color = EvaluateColor(ratio100);

        if (mixResistSprite != null)
        {
            mixResistSprite.color = color;
        }

        if (mixResistImage != null)
        {
            mixResistImage.color = color;
        }
    }

    private Color EvaluateColor(float ratio100)
    {
        if (ratio100 <= 0.25f)
        {
            var t = ratio100 / 0.25f;
            return Color.Lerp(color0, color25, t);
        }

        if (ratio100 <= 0.5f)
        {
            var t = (ratio100 - 0.25f) / 0.25f;
            return Color.Lerp(color25, color50, t);
        }

        var t50 = (ratio100 - 0.5f) / 0.5f;
        return Color.Lerp(color50, color100, t50);
    }
}
