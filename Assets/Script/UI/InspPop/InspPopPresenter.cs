using UnityEngine;

// Presenter for InspPop: owns core data and updates view each frame.
// Spec: Assets/Script/Core/InspCore.md
public sealed class InspPopPresenter : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private InspPopView view;
    [SerializeField] private GamanPopView gamanView;
    [SerializeField] private InspPopInput input;

    [Header("Tuning")]
    [SerializeField] private float mixPerClick = 0.01f; // Mix per input (1% = 0.01).

    private readonly InspCore _core = new InspCore();
    private readonly MixRateTracker _mixRateTracker = new MixRateTracker();
    private readonly GamanCore _gamanCore = new GamanCore();

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
        // Fallback: detect right click when no input binding is set.
        if (input == null && Input.GetMouseButtonDown(1))
        {
            OnRightClicked();
        }

        var dt = Time.deltaTime;

        // Mix -> Insp conversion.
        var moved = _core.Tick(dt);
        _movedSinceLastLog += moved;

        var mixAdded = _mixAddedThisFrame;
        _mixAddedThisFrame = 0.0;

        var mixAvgSpeed = _mixRateTracker.Tick(mixAdded, dt);
        _gamanCore.Tick(_core.Insp, mixAvgSpeed, mixPerClick, dt);

        // View update.
        if (view != null)
        {
            view.ApplyInsp(_core.Insp, _gamanCore.GamanValue);
        }

        if (gamanView != null)
        {
            gamanView.ApplyGaman(_gamanCore.GamanValue);
        }

        // Debug values (rendered by GameDebugOverlay).
        RegisterDebugValues(mixAvgSpeed);
    }

    private void RegisterDebugValues(double mixAvgSpeed)
    {
        GameDebug.Set("Insp", $"{_core.Insp:F3}");
        GameDebug.Set("Mix", $"{_core.Mix:F3}");
        GameDebug.Set("MixAvg", $"{mixAvgSpeed:F4}");
        GameDebug.Set("Limit", $"{_gamanCore.LimitSpeed:F4}");
        GameDebug.Set("GamanA", $"{_gamanCore.LoadRatioA:F3}");
        GameDebug.Set("Gaman", $"{_gamanCore.GamanValue:F3}");
    }

    private void OnRightClicked()
    {
        // Mix increases on right click (spec: InspCore.md).
        _core.AddMix(mixPerClick);
        _mixAddedThisFrame += mixPerClick;
    }
}
