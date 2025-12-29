using System;

// MixPop のポンプ量と補充（指数回復）を扱う純C#コア。
// 仕様: Assets/Script/Core/MixCore.md
public sealed class MixPopCore
{
    private const double IdleRechargeTauSeconds = 0.2;
    private const double PumpRechargeTauSeconds = 0.02;
    private const double DragPixelsPerHalfRadius = 100.0;

    private double _charge = 1.0;
    private double _pumpLevel;
    private double _dragScale = 1.0;

    public double Charge => _charge;
    public double PumpLevel => _pumpLevel;
    public double DragScale => _dragScale;

    // dragPixels: 上方向へのドラッグ距離（px）。mixStrength: 1回のミックス強度（1% = 0.01）。
    public double Tick(double dragPixels, bool isPumping, double mixStrength, double dt)
    {
        var safeMixStrength = mixStrength > 0.0 ? mixStrength : 0.0;
        var safeDragPixels = dragPixels > 0.0 ? dragPixels : 0.0;

        _dragScale = isPumping ? ComputeDragScale(safeDragPixels) : 1.0;

        var targetPump = isPumping ? 1.0 - _dragScale * _dragScale : 0.0;
        var deltaPump = targetPump - _pumpLevel;
        var mixAdded = 0.0;

        if (deltaPump > 0.0 && _charge > 0.0)
        {
            var actual = Math.Min(deltaPump, _charge);
            _charge -= actual;
            mixAdded = actual * safeMixStrength;
        }

        _pumpLevel = targetPump;

        if (dt > 0.0)
        {
            var tau = isPumping ? PumpRechargeTauSeconds : IdleRechargeTauSeconds;
            var alpha = 1.0 - Math.Exp(-dt / tau);
            _charge += (1.0 - _charge) * alpha;
            _charge = Math.Max(0.0, Math.Min(1.0, _charge));
        }

        return mixAdded;
    }

    public void Reset(double charge = 1.0)
    {
        _charge = Math.Max(0.0, Math.Min(1.0, charge));
        _pumpLevel = 0.0;
        _dragScale = 1.0;
    }

    private static double ComputeDragScale(double dragPixels)
    {
        var shrink = (dragPixels / DragPixelsPerHalfRadius) * 0.5;
        var scale = 1.0 - shrink;
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
