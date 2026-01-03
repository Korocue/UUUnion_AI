using UnityEngine;
using UnityEngine.UI;

// このファイルは MixPop の見た目（面積と色）だけを更新する。
// 仕様: Assets/Script/Core/MixCore.md
public sealed class MixPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform mixPop;
    [SerializeField] private SpriteRenderer mixSprite;
    [SerializeField] private Image mixImage;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;       // Mix=0 のときのスケール（sqrt(面積)）。
    [InspectorName("Mix Area Sqrt")]
    [Tooltip("Mix面積の係数の平方根。最終スケールは sqrt( base^2 + areaRatio*coeff^2 ) で算出。")]
    [SerializeField] private float scalePerMix = 0.4f;
    [SerializeField] private float mixStrengthPer100 = 0.01f; // 100% を表す Mix 強度。
    [SerializeField] private float maxAreaScale = 1.5f;    // 面積上限（150%）。

    [Header("Color Band")]
    [SerializeField] private Color color0 = Color.white;   // 0%
    [SerializeField] private Color color100 = Color.yellow;// 1%
    [SerializeField] private Color color1000 = Color.red;  // 10%

    private void OnEnable()
    {
        if (mixPop != null)
        {
            mixPop.localScale = Vector3.zero;
        }

        ApplyColor(0.0f);
    }

    // Core から受け取った状態で見た目だけ更新する。
    public void ApplyMix(double charge, double pumpLevel, float mixStrength)
    {
        if (mixPop == null)
        {
            return;
        }

        var targetArea = 1.0f - (float)pumpLevel;
        var safeCharge = Mathf.Clamp((float)charge, 0f, maxAreaScale);
        var areaRatio = Mathf.Clamp(Mathf.Min(safeCharge, targetArea), 0f, maxAreaScale);
        var baseArea = baseScale * baseScale;
        var area = baseArea + areaRatio * scalePerMix * scalePerMix;
        var scale = Mathf.Sqrt(Mathf.Max(0.0f, area));
        mixPop.localScale = Vector3.one * scale;

        var denom = Mathf.Max(0.0001f, mixStrengthPer100);
        var ratio = Mathf.Max(0.0f, mixStrength / denom);
        ApplyColor(ratio);
    }

    private void ApplyColor(float ratio)
    {
        Color color;
        if (ratio <= 1.0f)
        {
            color = Color.Lerp(color0, color100, ratio);
        }
        else
        {
            var t = Mathf.Clamp01((ratio - 1.0f) / 9.0f);
            color = Color.Lerp(color100, color1000, t);
        }

        if (mixSprite != null)
        {
            mixSprite.color = color;
        }

        if (mixImage != null)
        {
            mixImage.color = color;
        }
    }

    public void SetMaxAreaScale(float value)
    {
        maxAreaScale = Mathf.Max(1.0f, value);
    }

    public float GetMaxAreaScale()
    {
        return maxAreaScale;
    }
}
