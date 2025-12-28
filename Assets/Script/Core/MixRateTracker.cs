// Mix average speed tracker (half-life EMA).
// Spec: Assets/Script/Core/InspCore.md
public sealed class MixRateTracker
{
    private const double DefaultHalfLifeSeconds = 1.0;

    private readonly EmaHalfLife _ema;

    public MixRateTracker(double halfLifeSeconds = DefaultHalfLifeSeconds)
    {
        _ema = new EmaHalfLife(halfLifeSeconds);
    }

    public double MixAvgSpeed => _ema.Value;

    // mixAdded: delta Mix within this frame, dt: seconds.
    public double Tick(double mixAdded, double dt)
    {
        if (dt <= 0.0)
        {
            return _ema.Value;
        }

        var safeMixAdded = mixAdded > 0.0 ? mixAdded : 0.0;
        // Half of the real mix rate is used as EMA input (spec update).
        var inputRate = (safeMixAdded / dt) * 0.5;
        return _ema.Step(inputRate, dt);
    }

    public void Reset(double value = 0.0)
    {
        _ema.Reset(value);
    }
}
