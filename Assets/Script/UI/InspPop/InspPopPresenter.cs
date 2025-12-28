using UnityEngine;

// このファイルは InspCore（数値ロジック）を保持し、毎フレーム Tick して表示へ反映する。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class InspPopPresenter : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private InspPopView view;
    [SerializeField] private InspPopInput input;

    [Header("Tuning")]
    [SerializeField] private float mixPerClick = 0.01f; // 右クリック 1 回で増える Mix（= 1%）

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
        // 入力が未設定でも最低限動くよう、右クリックを直接検知するフォールバック。
        if (input == null && Input.GetMouseButtonDown(1))
        {
            OnRightClicked();
        }

        // 毎フレーム、Mix→Insp 変換を進める。
        var moved = _core.Tick(Time.deltaTime);
        _movedSinceLastLog += moved;

        // 見た目は Insp で更新する（View は表示だけ）。
        if (view != null)
        {
            view.ApplyInsp(_core.Insp);
        }

        // デバッグ表示用に主要値を登録する（表示自体は GameDebugOverlay が担当）。
        RegisterDebugValues();

        // チュートリアル用ログ（1 秒に 1 回）。
        _logTimer += Time.deltaTime;
        if (_logTimer >= 1.0f)
        {
            _logTimer -= 1.0f;
            Debug.Log($"[InspPop] Insp={_core.Insp:F3} Mix={_core.Mix:F3} moved={_movedSinceLastLog:F3}");
            _movedSinceLastLog = 0.0;
        }
    }

    private void RegisterDebugValues()
    {
        GameDebug.Set("Insp", $"{_core.Insp:F3}");
        GameDebug.Set("Mix", $"{_core.Mix:F3}");
    }

    private void OnRightClicked()
    {
        // 仕様: 右クリックで Mix が増える。
        _core.AddMix(mixPerClick);
    }
}