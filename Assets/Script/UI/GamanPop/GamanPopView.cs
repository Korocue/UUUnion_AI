using UnityEngine;
using UnityEngine.UI;

// このファイルは GamanPop の見た目（面積と色）だけを更新する。
// 仕様: Assets/Script/Core/GamanCore.md
public sealed class GamanPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform gamanPop;
    [SerializeField] private SpriteRenderer gamanSprite;
    [SerializeField] private Image gamanImage;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;     // Gaman=0 のときのスケール（sqrt(面積)）。
    [InspectorName("Gaman Area Sqrt")]
    [Tooltip("Gaman面積の係数の平方根。最終スケールは sqrt( base^2 + ratio*coeff^2 ) で算出。")]
    [SerializeField] private float scalePerGaman = 0.5f;
    [SerializeField] private float gamanPer100 = 1.0f;   // 100% を表す Gaman 値。

    [Header("Color Band")]
    [SerializeField] private Color color0 = new Color(0.5f, 0.5f, 0.5f);     // 0%のグレー（128）
    [SerializeField] private Color color50 = new Color(0.25f, 0.25f, 0.25f); // 50%のダークグレー（64）
    [SerializeField] private Color color80 = Color.black;                   // 80%の黒
    [SerializeField] private Color color90 = new Color(0f, 0.25f, 0.25f);    // 90%のネイビー寄り（0,64,64）
    [SerializeField] private Color color100 = Color.red;                    // 100%の赤
    [SerializeField] private Color color200 = Color.white;                  // 200%の白
    [SerializeField] private Color color300 = new Color(1f, 0f, 1f);         // 300%のピンク（255,0,255）

    private void OnEnable()
    {
        if (gamanPop != null)
        {
            gamanPop.localScale = Vector3.zero;
        }

        ApplyColor(0.0f);
    }

    // Presenter から Gaman 値を受け取り、見た目だけ更新する。
    public void ApplyGaman(double gamanValue)
    {
        if (gamanPop == null)
        {
            return;
        }

        var denom = Mathf.Max(0.0001f, gamanPer100);
        var ratio100 = Mathf.Max(0.0f, (float)gamanValue / denom);
        var baseArea = baseScale * baseScale;
        var area = baseArea + ratio100 * scalePerGaman * scalePerGaman;
        var scale = Mathf.Sqrt(Mathf.Max(0.0f, area));
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

        if (ratio100 <= 3.0f)
        {
            var t = ratio100 - 2.0f;
            return Color.Lerp(color200, color300, t);
        }

        return color300;
    }
}
