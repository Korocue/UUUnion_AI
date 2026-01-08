using System;

// このファイルは Insp（蓄積値）と Mix（燃料）の変換ロジックだけを担当する（Unity依存なし）。
// 仕様: Assets/Script/Core/InspCore.md
//
// 制約:
// - UnityEngine参照禁止 / MonoBehaviour禁止
// - Mix→Inspの変換は Tick() の1箇所のみで行う（他メソッドで値を書き換えない）
public sealed class InspCore
{
    // 既定の吸収タイムスケール（秒）。「約0.2秒でMixが減る」挙動の基準。
    private const double MixToInspTauSeconds = 0.2;

    private double _insp;
    private double _mix;

    public double Insp => _insp;
    public double Mix => _mix;

    // 右クリック等の入力でMixを増減する。
    public void AddMix(double amount)
    {
        _mix = Math.Max(0.0, _mix + amount);
    }

    // 毎フレーム呼ぶ。dt（秒）に応じてMixを指数減衰でInspへ移す。
    // 戻り値: 今回Inspへ変換した moved 量。
    public double Tick(double dt)
    {
        if (dt <= 0.0 || _mix <= 0.0)
        {
            return 0.0;
        }

        // InspCore.md の指定:
        // alpha = 1 - exp(-dt / tau)
        // moved = Mix * alpha
        var alpha = 1.0 - Math.Exp(-dt / MixToInspTauSeconds);
        var moved = _mix * alpha;

        _mix -= moved;
        _insp += moved;

        // 端数や数値誤差で-0に寄るのを防ぐ。
        if (_mix < 0.0)
        {
            _mix = 0.0;
        }

        return moved;
    }

    // バクハツ時に Insp を全量回収して0にする。
    public double ConsumeInsp()
    {
        var amount = _insp;
        _insp = 0.0;
        return amount;
    }

    // Insp/Mix を初期化する。
    public void Reset()
    {
        _insp = 0.0;
        _mix = 0.0;
    }
}
