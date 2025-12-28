using System;

// Gaman calculation core (limit speed, load ratio A, gamanValue).
// Spec: Assets/Script/Core/GamanCore.md
public sealed class GamanCore
{
    private const double DefaultInspLimit = 1.0;       // 100%
    private const double DefaultHalfLifeSeconds = 60.0;

    private readonly EmaHalfLife _ema;
    private readonly double _inspLimit;

    private double _limitSpeed;
    private double _loadRatioA;

    public GamanCore(double inspLimit = DefaultInspLimit, double halfLifeSeconds = DefaultHalfLifeSeconds)
    {
        _inspLimit = Math.Max(0.0, inspLimit);
        _ema = new EmaHalfLife(halfLifeSeconds);
    }

    public double LimitSpeed => _limitSpeed;
    public double LoadRatioA => _loadRatioA;
    public double GamanValue => _ema.Value;

    // mixStrengthPerMix: amount of Mix per input (ex: 0.01 for 1%).
    public double Tick(double insp, double mixAvgSpeed, double mixStrengthPerMix, double dt)
    {
        var safeMixAvgSpeed = mixAvgSpeed > 0.0 ? mixAvgSpeed : 0.0;
        var safeMixStrength = mixStrengthPerMix > 0.0 ? mixStrengthPerMix : 0.0;

        if (safeMixStrength <= 0.0 || insp <= 0.0 || _inspLimit <= 0.0)
        {
            _limitSpeed = double.PositiveInfinity;
            _loadRatioA = 0.0;
            if (dt > 0.0)
            {
                _ema.Step(0.0, dt);
            }
            return _ema.Value;
        }

        _limitSpeed = safeMixStrength * (_inspLimit / insp);
        if (_limitSpeed <= 0.0 || double.IsInfinity(_limitSpeed) || double.IsNaN(_limitSpeed))
        {
            _loadRatioA = 0.0;
        }
        else
        {
            _loadRatioA = safeMixAvgSpeed / _limitSpeed;
        }

        if (dt > 0.0)
        {
            // がまんの限界に対する「ミックス速度」の割合(= A)は
            // 半減期1分の指数移動平均に、(A/60)を外力として加えます(= がまん値)
            _ema.Step(_loadRatioA / 60.0, dt);
        }

        return _ema.Value;
    }
}
