// Mix の平均速度を追跡する（half-life EMA）。
// 仕様: Assets/Script/Core/InspCore.md
public sealed class MixRateTracker
{
    private const double DefaultHalfLifeSeconds = 1.0;

    private readonly EmaHalfLife _ema;

    public MixRateTracker(double halfLifeSeconds = DefaultHalfLifeSeconds)
    {
        _ema = new EmaHalfLife(halfLifeSeconds);
    }

    public double MixAvgSpeed => _ema.Value;

    // mixAdded: このフレームで増えた Mix、dt: 秒。
    public double Tick(double mixAdded, double dt)
    {
        if (dt <= 0.0)
        {
            return _ema.Value;
        }

        var safeMixAdded = mixAdded > 0.0 ? mixAdded : 0.0;
        // 実際のミックス速度の半分を EMA 入力として使う（仕様更新）。
        var inputRate = (safeMixAdded / dt) * 0.5;
        return _ema.Step(inputRate, dt);
    }

    public void Reset(double value = 0.0)
    {
        _ema.Reset(value);
    }
}
