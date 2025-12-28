using System;

// Half-life EMA utility (continuous-time decay + external input rate).
// Spec: Assets/Script/Main.md, Assets/Script/Core/InspCore.md, Assets/Script/Core/GamanCore.md
public sealed class EmaHalfLife
{
    private const double MinHalfLifeSeconds = 1e-6;

    private double _halfLifeSeconds;
    private double _value;

    public EmaHalfLife(double halfLifeSeconds, double initialValue = 0.0)
    {
        _halfLifeSeconds = Math.Max(halfLifeSeconds, MinHalfLifeSeconds);
        _value = initialValue;
    }

    public double HalfLifeSeconds => _halfLifeSeconds;
    public double Value => _value;

    public void SetHalfLife(double halfLifeSeconds)
    {
        _halfLifeSeconds = Math.Max(halfLifeSeconds, MinHalfLifeSeconds);
    }

    public void Reset(double value = 0.0)
    {
        _value = value;
    }

    // Step with inputRate (per second) during dt.
    public double Step(double inputRate, double dt)
    {
        if (dt <= 0.0)
        {
            return _value;
        }

        var k = Math.Log(2.0) / _halfLifeSeconds;
        var decay = Math.Exp(-k * dt);
        _value = _value * decay + (inputRate / k) * (1.0 - decay);
        return _value;
    }
}
