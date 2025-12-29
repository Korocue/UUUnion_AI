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
    private double _pumpLevel;      // いま消費している割合（0.0〜1.0）。
    private double _dragScale = 1.0;// ドラッグによる半径スケール（1.0=100%）。
    private double _dragPixelsPerHalfRadius = 100.0;

    public double Charge => _charge;
    public double PumpLevel => _pumpLevel;
    public double DragScale => _dragScale;
    public double DragPixelsPerHalfRadius
    {
        get => _dragPixelsPerHalfRadius;
        set => _dragPixelsPerHalfRadius = value > 0.0 ? value : 100.0;
    }

    // dragPixels: 上方向へのドラッグ距離（px）。mixStrength: 1回のミックス強度（1% = 0.01）。
    public double Tick(double dragPixels, bool isPumping, double mixStrength, double dt)
    {
        var safeMixStrength = mixStrength > 0.0 ? mixStrength : 0.0;
        var safeDragPixels = dragPixels > 0.0 ? dragPixels : 0.0;

        // ドラッグ量から半径スケールを計算（べき乗減衰）。
        _dragScale = isPumping ? ComputeDragScale(safeDragPixels) : 1.0;

        // 面積ベースでポンプ量を決める。
        var targetPump = isPumping ? 1.0 - _dragScale * _dragScale : 0.0;
        var deltaPump = targetPump - _pumpLevel;
        var mixAdded = 0.0;

        // ポンプ量の増加分だけチャージを消費し、Mixを加算する。
        if (deltaPump > 0.0 && _charge > 0.0)
        {
            var actual = Math.Min(deltaPump, _charge);
            _charge -= actual;
            mixAdded = actual * safeMixStrength;
        }

        _pumpLevel = targetPump;

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
