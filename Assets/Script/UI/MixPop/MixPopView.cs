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
    [SerializeField] private float baseScale = 0.0f;       // Mix=0 のときのスケール（面積ベース）。
    [SerializeField] private float scalePerMix = 0.4f;     // sqrt(Mix) をスケールに変換する係数。
    [SerializeField] private float mixStrengthPer100 = 0.01f; // 100% を表す Mix 強度。

    [Header("Color Band")]
    [SerializeField] private Color color0 = Color.white;   // 0%
    [SerializeField] private Color color100 = Color.yellow;// 1%

    private void OnEnable()
    {
        if (mixPop != null)
        {
            mixPop.localScale = Vector3.zero;
        }

        ApplyColor(0.0f);
    }

    // Core から受け取った状態で見た目だけ更新する。
    public void ApplyMix(double charge, double dragScale, float mixStrength)
    {
        if (mixPop == null)
        {
            return;
        }

        var safeCharge = Mathf.Clamp01((float)charge);
        var safeDragScale = Mathf.Clamp01((float)dragScale);
        var areaRatio = safeCharge * safeDragScale * safeDragScale;
        var scale = Mathf.Max(0.0f, baseScale + Mathf.Sqrt(areaRatio) * scalePerMix);
        mixPop.localScale = Vector3.one * scale;

        var denom = Mathf.Max(0.0001f, mixStrengthPer100);
        var ratio = Mathf.Clamp01(mixStrength / denom);
        ApplyColor(ratio);
    }

    private void ApplyColor(float ratio)
    {
        var color = Color.Lerp(color0, color100, ratio);

        if (mixSprite != null)
        {
            mixSprite.color = color;
        }

        if (mixImage != null)
        {
            mixImage.color = color;
        }
    }
}
