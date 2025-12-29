using UnityEngine;

// このファイルは InspPop の Presenter としてコア状態を持ち、毎フレーム表示を更新する。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class InspPopPresenter : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private InspPopView view;
    [SerializeField] private GamanPopView gamanView;
    [SerializeField] private InspPopInput input;
    [SerializeField] private MixPopView mixView;
    [SerializeField] private MixPopInput mixInput;

    [Header("Tuning")]
    [SerializeField] private float mixPerClick = 0.01f; // 1回のポンプ強度（1% = 0.01）。
    [SerializeField] private float dragPixelsPerHalfRadius = 100f; // 100pxで半径が50%になる基準距離。

    private readonly InspCore _core = new InspCore();
    private readonly MixRateTracker _mixRateTracker = new MixRateTracker();
    private readonly GamanCore _gamanCore = new GamanCore();
    private readonly MixPopCore _mixPopCore = new MixPopCore();

    private float _logTimer;
    private double _movedSinceLastLog;
    private double _mixAddedThisFrame;

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
        // 入力未設定時のフォールバックとして右クリックを検知する。
        if (input == null && mixInput == null && Input.GetMouseButtonDown(1))
        {
            OnRightClicked();
        }

        var dt = Time.deltaTime;

        // ミックスポップのポンプ入力。
        if (mixInput != null)
        {
            var pixelsPerHalf = mixInput.DragPixelsPerHalfRadius > 0f
                ? mixInput.DragPixelsPerHalfRadius
                : dragPixelsPerHalfRadius;
            _mixPopCore.DragPixelsPerHalfRadius = pixelsPerHalf;
            var pumpAdded = _mixPopCore.Tick(mixInput.DragPixels, mixInput.IsPumping, mixPerClick, dt);
            if (pumpAdded > 0.0)
            {
                _core.AddMix(pumpAdded);
                _mixAddedThisFrame += pumpAdded;
            }

            if (mixView != null)
            {
                mixView.ApplyMix(_mixPopCore.Charge, _mixPopCore.DragScale, mixPerClick);
            }
        }
        else
        {
            _mixPopCore.DragPixelsPerHalfRadius = dragPixelsPerHalfRadius;
            _mixPopCore.Tick(0.0, false, mixPerClick, dt);
        }

        // Mix -> Insp 変換。
        var moved = _core.Tick(dt);
        _movedSinceLastLog += moved;

        var mixAdded = _mixAddedThisFrame;
        _mixAddedThisFrame = 0.0;

        var mixAvgSpeed = _mixRateTracker.Tick(mixAdded, dt);
        _gamanCore.Tick(_core.Insp, mixAvgSpeed, mixPerClick, dt);

        // View を更新する。
        if (view != null)
        {
            view.ApplyInsp(_core.Insp, _gamanCore.GamanValue);
        }

        if (gamanView != null)
        {
            gamanView.ApplyGaman(_gamanCore.GamanValue);
        }

        // デバッグ値（GameDebugOverlay が描画）。
        RegisterDebugValues(mixAvgSpeed);
    }

    private void RegisterDebugValues(double mixAvgSpeed)
    {
        GameDebug.Set("Insp", $"{_core.Insp:F3}");
        GameDebug.Set("Mix", $"{_core.Mix:F6}");
        GameDebug.Set("MixAvg", $"{mixAvgSpeed:F6}");
        GameDebug.Set("Limit", $"{_gamanCore.LimitSpeed:F4}");
        GameDebug.Set("GamanA", $"{_gamanCore.LoadRatioA:F3}");
        GameDebug.Set("Gaman", $"{_gamanCore.GamanValue:F3}");
        GameDebug.Set("MixCharge", $"{_mixPopCore.Charge:F6}");
        GameDebug.Set("MixPump", $"{_mixPopCore.PumpLevel:F6}");
        GameDebug.Set("MixRadius", $"{_mixPopCore.DragScale:F6}");
        if (mixInput != null)
        {
            GameDebug.Set("MixClickY", $"{mixInput.ClickY:F2}");
            GameDebug.Set("MixAnchorY", $"{mixInput.AnchorY:F2}");
            GameDebug.Set("MixCurrentY", $"{mixInput.CurrentY:F2}");
            GameDebug.Set("MixDragPx", $"{mixInput.DragPixels:F6}");
        }
    }

    private void OnRightClicked()
    {
        if (mixInput != null)
        {
            return;
        }

        // 右クリックで Mix が増える（仕様: InspCore.md）。
        _core.AddMix(mixPerClick);
        _mixAddedThisFrame += mixPerClick;
    }
}
