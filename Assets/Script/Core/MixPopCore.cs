using System;

// MixPop のポンプ量と補充（指数回復）を扱う純C#コア。
// 仕様: Assets/Script/Core/MixCore.md
public sealed class MixPopCore
{
    // 非ポンプ時の回復時定数（秒）。
    private const double IdleRechargeTauSeconds = 0.2;
    // ポンプ中の回復時定数（秒）。
    private const double PumpRechargeTauSeconds = 2.0;

    private double _charge = 1.0;   // 充填率（1.0=100%）。
    private double _pumpLevel;      // いま消費している割合（min〜1.0）。
    private double _dragScale = 1.0;// ドラッグによる半径スケール（1.0=100%）。
    private double _dragPixelsPerHalfRadius = 100.0;
    private double _maxAreaScale = 1.5;

    public double Charge => _charge;
    public double PumpLevel => _pumpLevel;
    public double DragScale => _dragScale;
    public double MaxAreaScale
    {
        get => _maxAreaScale;
        set => _maxAreaScale = value >= 1.0 ? value : 1.0;
    }
    public double DragPixelsPerHalfRadius
    {
        get => _dragPixelsPerHalfRadius;
        set => _dragPixelsPerHalfRadius = value > 0.0 ? value : 100.0;
    }

    // deltaY: マウスの上下移動量（px）。正=上方向、負=下方向。
    // mixStrength: 1回のミックス強度（1% = 0.01）。
    public double Tick(double deltaY, bool isPumping, double mixStrength, double dt)
    {
        var safeMixStrength = mixStrength > 0.0 ? mixStrength : 0.0;
        var prevPumpLevel = _pumpLevel;

        if (isPumping && deltaY != 0.0)
        {
            var scaleDelta = ComputeDragScale(Math.Abs(deltaY));
            var area = 1.0 - _pumpLevel;
            if (deltaY > 0.0)
            {
                // 上方向: 面積が逓減。
                area *= scaleDelta;
            }
            else
            {
                // 下方向: 収縮した面積が逓減（基準は200%面積）。
                area = _maxAreaScale - (_maxAreaScale - area) * scaleDelta;
            }

            area = Math.Max(0.0, Math.Min(_maxAreaScale, area));
            _pumpLevel = 1.0 - area;
        }

        if (!isPumping && dt > 0.0)
        {
            var alphaIdle = 1.0 - Math.Exp(-dt / IdleRechargeTauSeconds);
            _pumpLevel += (0.0 - _pumpLevel) * alphaIdle;
        }

        var minPumpLevel = 1.0 - _maxAreaScale;
        _pumpLevel = Math.Max(minPumpLevel, Math.Min(1.0, _pumpLevel));
        _dragScale = Math.Sqrt(Math.Max(0.0, 1.0 - _pumpLevel));

        var deltaPump = _pumpLevel - prevPumpLevel;
        var mixAdded = 0.0;

        // ポンプ量の増加分だけチャージを消費し、Mixを加算する。
        if (isPumping && deltaPump > 0.0 && _charge > 0.0)
        {
            var actual = Math.Min(deltaPump, _charge);
            _charge -= actual;
            mixAdded = actual * safeMixStrength;
        }

        if (dt > 0.0)
        {
            // (A) ポンプしていない分は時定数0.2秒で回復（上限は未ポンプ面積）。
            var alphaIdle = 1.0 - Math.Exp(-dt / IdleRechargeTauSeconds);
            var maxCharge = _dragScale * _dragScale;
            _charge += (maxCharge - _charge) * alphaIdle;
            _charge = Math.Max(0.0, Math.Min(maxCharge, _charge));

            // (B) ポンプ中の分は時定数2秒で回復し、即座に消費される。
            if (isPumping && _pumpLevel > 0.0)
            {
                var alphaPump = 1.0 - Math.Exp(-dt / PumpRechargeTauSeconds);
                var pumpRecovered = _pumpLevel * alphaPump;
                mixAdded += pumpRecovered * safeMixStrength;
            }
            else if (!isPumping && _charge > 1.0)
            {
                // ポンプ停止中に100%を超えて貯まった分は、時定数0.2秒でMixへ変換する。
                var overflow = _charge - 1.0;
                var convert = overflow * alphaIdle;
                _charge -= convert;
                mixAdded += convert * safeMixStrength;
            }
        }

        return mixAdded;
    }

    public void Reset(double charge = 1.0)
    {
        _charge = Math.Max(0.0, Math.Min(1.0, charge));
        _pumpLevel = 0.0;
        _dragScale = 1.0;
    }

    private double ComputeDragScale(double dragPixels)
    {
        var normalized = dragPixels / _dragPixelsPerHalfRadius;
        var scale = Math.Pow(0.5, normalized);

        if (scale < 0.0)
        {
            return 0.0;
        }

        if (scale > 1.0)
        {
            return 1.0;
        }

        return scale;
    }
}






