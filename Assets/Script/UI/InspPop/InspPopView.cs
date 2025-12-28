using UnityEngine;

// このファイルは InspPop の見た目更新だけを担当する（数値ロジックは持たない）。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class InspPopView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Transform inspPop;

    [Header("Tuning")]
    [SerializeField] private float baseScale = 0.0f;      // Insp=0のときの基本スケール（面積0を想定）
    [SerializeField] private float scalePerInsp = 0.5f;   // Inspに応じた膨らみ係数（面積∝Inspになるようsqrtでスケール）

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
    }
}
