using System;

// Gaman calculation core (limit speed, load ratio A, gamanValue).
// Spec: Assets/Script/Core/GamanCore.md
public sealed class GamanCore
{
    private const double DefaultInspLimit = 1.0;       // 100%
    private const double DefaultHalfLifeSeconds = 60.0;
    private const double MixResistanceMaxDefault = 100.0;
    private const double MixResistanceMaxA = 1000.0;
    private const double MixResistanceMinDefault = 0.1;
    private const double MixResistanceMinA = 1.0;
    private const double MinGamanForResistance = 0.01;

    private readonly EmaHalfLife _ema;
    private readonly double _inspLimit;

    private double _limitSpeed;
    private double _loadRatioA;
    private double _mixResistanceB;
    private double _inspRate;
    private double _mixResistanceBase;
    private double _mixResistanceAdd;
    private double _mixResistanceRate;
    private double _gamanValue;
    private double _dokiEmaValue;

    public enum GamanMode
    {
        ZiwaZiwa,
        DokiDoki
    }

    public enum InspModifierMode
    {
        Off,
        Inverse,
        Linear,
        Saturation
    }

    public GamanCore(double inspLimit = DefaultInspLimit, double halfLifeSeconds = DefaultHalfLifeSeconds)
    {
        _inspLimit = Math.Max(0.0, inspLimit);
        _ema = new EmaHalfLife(halfLifeSeconds);
    }

    public double LimitSpeed => _limitSpeed;
    public double LoadRatioA => _loadRatioA;
    public double MixResistanceB => _mixResistanceB;
    public double MixResistanceBase => _mixResistanceBase;
    public double MixResistanceAdd => _mixResistanceAdd;
    public double MixResistanceRate => _mixResistanceRate;
    public double GamanValue => _gamanValue;
    public double InspRate => _inspRate;
    public double DokiEmaValue => _dokiEmaValue;
    public GamanMode Mode { get; set; } = GamanMode.ZiwaZiwa;
    public InspModifierMode ModifierMode { get; set; } = InspModifierMode.Off;
    public bool EnableGamanRate { get; set; } = true;
    public bool EnableInspModifier { get; set; } = true;
    public bool EnableGamanA { get; set; } = false;

    // mixStrengthPerMix: amount of Mix per input (ex: 0.01 for 1%).
    public double Tick(double insp, double mixAvgSpeed, double mixStrengthPerMix, double dt)
    {
        var safeMixAvgSpeed = mixAvgSpeed > 0.0 ? mixAvgSpeed : 0.0;
        var safeMixStrength = mixStrengthPerMix > 0.0 ? mixStrengthPerMix : 0.0;

        if (safeMixStrength <= 0.0 || insp <= 0.0 || _inspLimit <= 0.0)
        {
            _limitSpeed = double.PositiveInfinity;
            _loadRatioA = 0.0;
            _mixResistanceB = GetResistanceMax();
            _mixResistanceBase = GetResistanceMax();
            _mixResistanceAdd = 0.0;
            _mixResistanceRate = 0.0;
            _inspRate = 0.0;
            _gamanValue = 0.0;
            _dokiEmaValue = 0.0;
            if (dt > 0.0)
            {
                _ema.SetHalfLife(DefaultHalfLifeSeconds);
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

        _inspRate = _inspLimit > 0.0 ? Math.Max(0.0, insp / _inspLimit) : 0.0;

        if (dt > 0.0)
        {
            // がまんの限界に対する「ミックス速度」の割合(= A)は
            // 半減期1分の指数移動平均に、(A/60)を外力として加えます(= がまん値)
            _ema.SetHalfLife(DefaultHalfLifeSeconds);
            _ema.Step(_loadRatioA / 60.0, dt);
        }

        if (Mode == GamanMode.DokiDoki)
        {
            if (dt > 0.0)
            {
                var k = Math.Log(2.0) / 1.0;
                var decay = Math.Exp(-k * dt);
                _dokiEmaValue = _dokiEmaValue * decay + _loadRatioA * (1.0 - decay);
            }

            _gamanValue = _loadRatioA - _dokiEmaValue;
        }
        else
        {
            _gamanValue = _ema.Value;
        }
        _mixResistanceBase = ResolveBaseResistance(_gamanValue, _loadRatioA);
        _mixResistanceB = ApplyModeModifier(_mixResistanceBase, _inspRate);

        if (dt > 0.0)
        {
            // B秒あたりのミックス速度として、A/B を加える。
            _ema.SetHalfLife(_mixResistanceB);
            _ema.Step(_loadRatioA / _mixResistanceB, dt);
        }

        return _ema.Value;
    }

    private double ComputeMixResistance(double gamanValue)
    {
        if (gamanValue <= MinGamanForResistance)
        {
            return GetResistanceMax();
        }

        var value = 1.0 / gamanValue;
        return Clamp(value, GetResistanceMin(), GetResistanceMax());
    }

    private double ResolveBaseResistance(double gamanValue, double loadRatioA)
    {
        if (EnableGamanA)
        {
            return ComputeResistanceFromA(loadRatioA);
        }

        if (!EnableGamanRate)
        {
            return 1.0;
        }

        return ComputeMixResistance(gamanValue);
    }

    private double ApplyModeModifier(double baseResistance, double inspRate)
    {
        if (EnableGamanA || !EnableInspModifier || ModifierMode == InspModifierMode.Off)
        {
            _mixResistanceAdd = 0.0;
            _mixResistanceRate = 0.0;
            return Clamp(baseResistance, GetResistanceMin(), GetResistanceMax());
        }

        var max = GetResistanceMax();
        var missing = max - baseResistance;
        if (missing <= 0.0)
        {
            _mixResistanceAdd = 0.0;
            _mixResistanceRate = 1.0;
            return max;
        }

        var rate = ComputeModifierRate(inspRate, ModifierMode);
        _mixResistanceRate = rate;
        _mixResistanceAdd = missing * rate;
        var adjusted = baseResistance + _mixResistanceAdd;
        return Clamp(adjusted, GetResistanceMin(), GetResistanceMax());
    }

    private double ComputeResistanceFromA(double loadRatioA)
    {
        var safeA = Math.Max(0.0, loadRatioA);
        var resistance = safeA * 100.0;
        if (resistance < 1.0)
        {
            resistance = 1.0;
        }

        return Clamp(resistance, GetResistanceMin(), GetResistanceMax());
    }

    private double GetResistanceMin()
    {
        return EnableGamanA ? MixResistanceMinA : MixResistanceMinDefault;
    }

    private double GetResistanceMax()
    {
        return EnableGamanA ? MixResistanceMaxA : MixResistanceMaxDefault;
    }

    private static double ComputeModifierRate(double inspRate, InspModifierMode mode)
    {
        var r = Math.Max(0.0, inspRate);
        switch (mode)
        {
            case InspModifierMode.Inverse:
            {
                var denom = Math.Max(1e-6, 1.0 - r);
                return Clamp(0.01 / denom, 0.0, 1.0);
            }
            case InspModifierMode.Linear:
                return Clamp(r, 0.0, 1.0);
            case InspModifierMode.Saturation:
            {
                return Clamp(UUSat.Evaluate(r), 0.0, 1.0);
            }
            default:
                return 0.0;
        }
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min)
        {
            return min;
        }

        if (value > max)
        {
            return max;
        }

        return value;
    }

    public void Reset()
    {
        _limitSpeed = 0.0;
        _loadRatioA = 0.0;
        _mixResistanceB = GetResistanceMax();
        _mixResistanceBase = GetResistanceMax();
        _mixResistanceAdd = 0.0;
        _mixResistanceRate = 0.0;
        _inspRate = 0.0;
        _gamanValue = 0.0;
        _dokiEmaValue = 0.0;
        _ema.SetHalfLife(DefaultHalfLifeSeconds);
        _ema.Reset(0.0);
    }
}
