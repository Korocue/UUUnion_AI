using System;

// うにうに飽和関数 (UUSat) の共通ヘルパー。
public static class UUSat
{
    // k = log2(3)
    private const double K = 1.5849625007211563; // Math.Log(3.0) / Math.Log(2.0)

    // r = インスピレーション率 (100% = 1.0) を想定。
    public static double Evaluate(double r)
    {
        if (r <= 0.0)
        {
            return 0.0;
        }

        var rk = Math.Pow(r, K);
        return rk / (1.0 + rk);
    }
}
