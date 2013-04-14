using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public static class MathEx {
    public static IEnumerable<Int32> InvDivS32(Int32 q, Int32 d) {
        if (d <= 0) throw new ArgumentOutOfRangeException("d", "d <= 0");
        if (q < Int32.MinValue / d || q > Int32.MaxValue / d)
            yield break;

        var n = q * d;
        if (n == 0)
            for (var i = -d + 1; i < d; i++)
                yield return i;
        else if (n > 0)
            for (var i = 0; i < d && n + i > 0; i++)
                yield return n + i;
        else
            for (var i = 0; i < d && n - i < 0; i++)
                yield return n - i;
    }

    public static Int32 TruncatedToMultipleOf(this int value, int factor) {
        if (factor <= 0) throw new ArgumentOutOfRangeException("factor", "factor <= 0");
        return value - value % factor;
    }
    public static GCDExResult ExtendedGreatestCommonDivisor(Int64 a, Int64 b) {
        if (a < 0) throw new ArgumentOutOfRangeException("a", "a < 0");
        if (b < 0) throw new ArgumentOutOfRangeException("b", "b < 0");
        if (a == 0 && b == 0) throw new ArgumentException("a == 0 && b == 0");

        Int64 x = 0, y = 1;
        Int64 u = 1, v = 0;

        while (a != 0) {
            var q = b / a;
            var r = b % a;
            var m = x - u * q;
            var n = y - v * q;
            
            b = a;
            x = u;
            y = v;

            a = r;
            u = m;
            v = n;
        }
        return new GCDExResult(b, x, y);
    }
    public static Int64? MultiplicativeInverseMod(Int64 a, Int64 m) {
        if (a < 0) throw new ArgumentOutOfRangeException("a", "a < 0");
        if (m <= 0) throw new ArgumentOutOfRangeException("m", "m < 0");
        var gcdEx = ExtendedGreatestCommonDivisor(a, m);

        if (gcdEx.g != 1) return null;
        var r = gcdEx.x % m;
        if (r < 0) r += m;
        return r;
    }

    public static Int32 MultiplicativeInverseS32(Int32 factor) {
        var r = TryMultiplicativeInverseS32(factor);
        if (r == null) throw new InvalidOperationException();
        return r.Value;
    }
    public static Int32? TryMultiplicativeInverseS32(Int32 factor) {
        if (factor % 2 == 0) return null;
        var inv = MultiplicativeInverseMod((UInt32)factor, 1L << 32);
        if (inv == null) return null;
        return (Int32)(UInt32)inv.Value;
    }

    public static IEnumerable<Int32> InvMulS32(Int32 product, Int32 factor) {
        if (factor % 2 == 0) {
            if (product % 2 != 0)
                yield break;
            foreach (var item in InvMulS32(product / 2, factor / 2)) {
                yield return item & ~(1 << 31);
                yield return item | (1 << 31);
            }
        } else {
            var inv = MultiplicativeInverseMod((UInt32)factor, 1L << 32);
            if (inv == null) yield break;
            yield return product * (Int32)(UInt32)inv;
        }
    }

    public static Int32 Pow(this Int32 value, Int32 power) {
        var result = 1;
        for (var i = 0; i < power; i++)
            result *= value;
        return result;
    }
    public static Int32 PowSum(this Int32 @base, int powerCount) {
        if (@base == 1) throw new ArgumentOutOfRangeException("base", "base == 1");
        if (powerCount < 0) throw new ArgumentOutOfRangeException("powerCount", "powerCount < 0");

        var d = @base - 1;
        var modRange = (1L << 32) * d;
        var r = (BigInteger.ModPow(@base, powerCount, modRange) - 1) / d;
        return (int)(long)r;
    }
    public static Int32 DiagonalPowSum(Int32 v1, Int32 v2, Int32 n) {
        if (n < 0) throw new ArgumentOutOfRangeException("n", "n < 0");
        BigInteger a = v1;
        BigInteger b = v2;
        var m = (a - b) * (1L << 32);
        var numerator = BigInteger.ModPow(a, n, m)
                      - BigInteger.ModPow(b, n, m);
        return (Int32)(Int64)((numerator / (a - b)) % (1L << 32));
    }
    public static Int32 TrianglePowSum(Int32 v1, Int32 v2, Int32 n) {
        if (n < 0) throw new ArgumentOutOfRangeException("n", "n < 0");
        BigInteger a = v1;
        BigInteger b = v2;
        var ma = (a - 1) * (a - b) * (1L << 32);
        var mb = (b - 1) * (a - b) * (1L << 32);
        var numerator = BigInteger.ModPow(a, n + 1, ma) * (b - 1)
                             - BigInteger.ModPow(b, n + 1, mb) * (a - 1)
                             + a - b;
        var denominator = (a - 1) * (b - 1) * (a - b);
        return (Int32)(Int64)((numerator / denominator) % (1L << 32));
    }
    public static IEnumerable<Int32> DiagonalPowSums(Int32 v1, Int32 v2) {
        for (var n = 1;; n++)
            yield return DiagonalPowSum(v1, v2, n);
    }
    public static IEnumerable<Int32> TrianglePowSums(Int32 v1, Int32 v2) {
        for (var n = 1;; n++)
            yield return TrianglePowSum(v1, v2, n);
    }
    public static IEnumerable<Int32> InvPlusThird(Int32 n) {
        unchecked {
            if (n == 0)
                yield return 0;
            if (n > 0 || n <= Int32.MaxValue + Int32.MaxValue / 3)
                if (n % 4 != 3 && n % 4 != -1)
                    yield return n - (Int32)((UInt32)n / 4);
            if (n < 0 || n >= Int32.MinValue + Int32.MinValue / 3)
                if (n % 4 != -3 && n % 4 != 1)
                    yield return n + (Int32)((UInt32)(-(Int64)n) / 4);
        }
    }

    public static Int32 Dot(this IEnumerable<Int32> values1, IEnumerable<Int32> values2) {
        return values1.Zip(values2, (v1, v2) => v1 * v2).SumWrap();
    }
    public static Int32 RevDot(this IEnumerable<Int32> values1, IEnumerable<Int32> values2) {
        return values1.Reverse().Dot(values2);
    }
    public static Int32 SumWrap(this IEnumerable<Int32> values) {
        var total = 0;
        foreach (var value in values) {
            unchecked {
                total += value;
            }
        }
        return total;
    }
    public static Int32 FactorTrianglePowerSum(IEnumerable<Int32> factors, Int32 v1, Int32 v2) {
        BigInteger a = v1;
        BigInteger b = v2;
        BigInteger t = 0;
        var i = 0;
        foreach (var factor in factors.Reverse()) {
            i += 1;
            t += factor * (BigInteger.Pow(a, i) - BigInteger.Pow(b, i));
        }
        if (t % (a - b) != 0) throw new Exception();
        return (Int32)(Int64)((t / (a - b)) % (1L << 32));
    }
    public static IEnumerable<Int32> Powers(this Int32 b) {
        var t = 1;
        while (true) {
            yield return t;
            t *= b;
        }
    }
    public static bool DoesAdditionOverflowS32(Int32 v1, Int32 v2) {
        return v1 + v2 == v1 + (Int64)v2;
    }
}
