using UnityEngine;
using UnityEngine.UI;

// このファイルは InspPop の見た目（スケールと色帯）だけを更新する。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class InspPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform inspPop;
    [SerializeField] private SpriteRenderer inspSprite;
    [SerializeField] private Image inspImage;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;    // Insp=0 のときのスケール（面積ベース）。
    [SerializeField] private float scalePerInsp = 0.5f; // sqrt(Insp) をスケールに変換する係数。
    [SerializeField] private float inspPer100 = 1.0f;   // 100% を表す Insp 値。

    [Header("Gaman Overlay")]
    [SerializeField] private float gamanPer100 = 1.0f;  // 100% を表す Gaman 値。
    [SerializeField] private float scalePerGaman = 0.5f;// sqrt(Gaman) をスケールに変換する係数。

    [Header("Color Band")]
    [SerializeField] private Color color0 = new Color(0f, 1f, 0f); // 0%（緑）
    [SerializeField] private Color color50 = Color.white;          // 50%
    [SerializeField] private Color color100 = Color.yellow;        // 100%
    [SerializeField] private Color color200 = Color.red;           // 200%
    [SerializeField] private Color color500 = Color.black;         // 500%

    private void OnEnable()
    {
        // 仕様: 初期 localScale は 0（円を表示しない）。
        if (inspPop != null)
        {
            inspPop.localScale = Vector3.zero;
        }

        ApplyColor(0.0f);
    }

    // Presenter から Insp/Gaman 値を受け取り、見た目だけ更新する。
    public void ApplyInsp(double insp, double gamanValue)
    {
        if (inspPop == null)
        {
            return;
        }

        var safeInsp = Mathf.Max(0.0f, (float)insp);
        var inspScale = Mathf.Sqrt(safeInsp) * scalePerInsp;

        var safeGaman = Mathf.Max(0.0f, (float)gamanValue);
        var gamanDenom = Mathf.Max(0.0001f, gamanPer100);
        var gamanRatio = Mathf.Min(safeGaman / gamanDenom, 1.0f);
        var gamanScale = Mathf.Sqrt(gamanRatio) * scalePerGaman;

        var scale = Mathf.Max(0.0f, baseScale + inspScale + gamanScale);
        inspPop.localScale = Vector3.one * scale;

        ApplyColor(safeInsp);
    }

    private void ApplyColor(float insp)
    {
        var denom = Mathf.Max(0.0001f, inspPer100);
        var ratio100 = Mathf.Max(0.0f, insp / denom);
        var color = EvaluateColor(ratio100);

        if (inspSprite != null)
        {
            inspSprite.color = color;
        }

        if (inspImage != null)
        {
            inspImage.color = color;
        }
    }

    private Color EvaluateColor(float ratio100)
    {
        if (ratio100 <= 0.5f)
        {
            var t = ratio100 / 0.5f;
            return Color.Lerp(color0, color50, t);
        }

        if (ratio100 <= 1.0f)
        {
            var t = (ratio100 - 0.5f) / 0.5f;
            return Color.Lerp(color50, color100, t);
        }

        if (ratio100 <= 2.0f)
        {
            var t = ratio100 - 1.0f;
            return Color.Lerp(color100, color200, t);
        }

        if (ratio100 <= 5.0f)
        {
            var t = (ratio100 - 2.0f) / 3.0f;
            return Color.Lerp(color200, color500, t);
        }

        return color500;
    }
}
