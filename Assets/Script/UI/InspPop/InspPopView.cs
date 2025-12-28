using UnityEngine;

// このファイルは InspPop の見た目更新だけを担当する（数値ロジックは持たない）。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class InspPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform inspPop;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;    // Scale at Insp=0 (area-based).
    [SerializeField] private float scalePerInsp = 0.5f; // sqrt(Insp) -> scale.
    [SerializeField] private float inspPer100 = 1.0f;   // Insp value that represents 100%.

    [Header("Gaman Overlay")]
    [SerializeField] private float gamanPer100 = 1.0f;  // Gaman value that represents 100%.
    [SerializeField] private float scalePerGaman = 0.5f;// sqrt(Gaman) -> scale.

    [Header("Color Band")]
    [SerializeField] private Color color0 = new Color(0f, 1f, 0f); // 0% (green)
    [SerializeField] private Color color50 = Color.white;          // 50%
    [SerializeField] private Color color100 = Color.yellow;        // 100%
    [SerializeField] private Color color200 = Color.red;           // 200%
    [SerializeField] private Color color500 = Color.black;         // 500%

    private void OnEnable()
    {
        // 仕様: localScaleの初期値は0
        // （開始直後から「丸が存在しない」状態）。
        if (inspPop != null)
        {
            inspPop.localScale = Vector3.zero;
        }
    }

    // Presenterから渡されたInsp値でスケールだけ更新する。
    public void ApplyInsp(double insp)
    {
        if (inspPop == null)
        {
            return;
        }

        // 仕様: 「面積がInspに応じて決まる」ため、見た目のスケールは sqrt(Insp) を用いる。
        // Transform.localScale の面積は概ね scale^2 に比例する想定。
        var safeInsp = Mathf.Max(0.0f, (float)insp);
        var scale = Mathf.Max(0.0f, baseScale + Mathf.Sqrt(safeInsp) * scalePerInsp);
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
