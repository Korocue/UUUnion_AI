using UnityEngine;

// このファイルは InspCore（数値ロジック）を保持し、毎フレーム Tick して表示へ反映する。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class InspPopPresenter : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private InspPopView view;
    [SerializeField] private InspPopInput input;

    [Header("Tuning")]
    [SerializeField] private float mixPerClick = 0.01f; // 右クリック1回で増えるMix（= 1%）

    private readonly InspCore _core = new InspCore();

    private float _logTimer;
    private double _movedSinceLastLog;

    private void OnEnable()
    {
        if (input != null)
        {
            input.RightClicked += OnRightClicked;
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.RightClicked -= OnRightClicked;
        }
    }

    private void Update()
    {
        // 入力が未設定でも動くように、最低限のフォールバック（右クリック）を持つ。
        if (input == null && Input.GetMouseButtonDown(1))
        {
            OnRightClicked();
        }

        // 毎フレーム、Mix→Insp変換を進める。
        var moved = _core.Tick(Time.deltaTime);
        _movedSinceLastLog += moved;

        // 見た目はInspで更新する（Viewは表示だけ）。
        if (view != null)
        {
            view.ApplyInsp(_core.Insp);
        }

        // テスト用ログ（1秒に1回）。
        _logTimer += Time.deltaTime;
        if (_logTimer >= 1.0f)
        {
            _logTimer -= 1.0f;
            Debug.Log($"[InspPop] Insp={_core.Insp:F3} Mix={_core.Mix:F3} moved={_movedSinceLastLog:F3}");
            _movedSinceLastLog = 0.0;
        }
    }

    private void OnRightClicked()
    {
        // 仕様: 右クリックでMixが増える。
        _core.AddMix(mixPerClick);
    }
}
