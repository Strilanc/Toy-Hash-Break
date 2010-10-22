using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Numerics;

public static class MathEx {
    public static IEnumerable<Int32> InvDivS32(Int32 q, Int32 d) {
        Contract.Requires(d > 0);
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<Int32>>(), i => i / d == q));
        if (q < Int32.MinValue / d || q > Int32.MaxValue / d)
            yield break;

        int n = q * d;
        if (n == 0)
            for (int i = -d + 1; i < d; i++)
                yield return i;
        else if (n > 0)
            for (int i = 0; i < d && n + i > 0; i++)
                yield return n + i;
        else
            for (int i = 0; i < d && n - i < 0; i++)
                yield return n - i;
    }

    public static Int32 TruncatedToMultipleOf(this int value, int factor) {
        Contract.Requires(factor > 0);
        Contract.Ensures(Contract.Result<Int32>() % factor == 0);
        Contract.Ensures(Math.Abs((Int64)Contract.Result<Int32>()) <= Math.Abs((Int64)value));
        Contract.Ensures(Math.Abs((Int64)Contract.Result<Int32>()) > Math.Abs((Int64)value) - factor);
        return value - value % factor;
    }
    public static GCDExResult ExtendedGreatestCommonDivisor(Int64 a, Int64 b) {
        Contract.Requires(a >= 0);
        Contract.Requires(b >= 0);
        Contract.Requires(a > 0 || b > 0);
        Contract.Ensures(a * Contract.Result<GCDExResult>().x + b * Contract.Result<GCDExResult>().y == Contract.Result<GCDExResult>().g);
        Contract.Ensures(a % Contract.Result<GCDExResult>().g == 0);
        Contract.Ensures(b % Contract.Result<GCDExResult>().g == 0);

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
        Contract.Requires(a >= 0);
        Contract.Requires(m > 0);
        Contract.Ensures(!Contract.Result<Int64?>().HasValue || (Contract.Result<Int64?>().Value * a) % m == 1);
        Contract.Ensures(!Contract.Result<Int64?>().HasValue || Contract.Result<Int64?>().Value > 0);
        var gcdEx = ExtendedGreatestCommonDivisor(a, m);

        if (gcdEx.g != 1) return null;
        var r = gcdEx.x % m;
        if (r < 0) r += m;
        return r;
    }

    public static Int32? MultiplicativeInverseS32(Int32 factor) {
        Contract.Ensures(Contract.Result<Int32?>().HasValue == (factor % 2 != 0));
        Contract.Ensures(!Contract.Result<Int32?>().HasValue || Contract.Result<Int32?>().Value * factor == 1);
        if (factor % 2 == 0) return null;
        var inv = MultiplicativeInverseMod((UInt32)factor, 1L << 32);
        if (inv == null) return null;
        return (Int32)(UInt32)inv.Value;
    }

    public static IEnumerable<Int32> InvMulS32(Int32 product, Int32 factor) {
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<Int32>>(), i => i * factor == product));
        if (factor % 2 == 0) {
            if (product % 2 != 0)
                yield break;
            foreach (Int32 item in InvMulS32(product / 2, factor / 2)) {
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
        Int32 result = 1;
        for (int i = 0; i < power; i++)
            result *= value;
        return result;
    }
    public static Int32 PowSum(this Int32 @base, int powerCount) {
        Contract.Requires(@base != 1);
        Contract.Requires(powerCount >= 0);

        int d = @base - 1;
        long modRange = (1L << 32) * d;
        var r = (BigInteger.ModPow(@base, powerCount, modRange) - 1) / d;
        return (int)(long)r;
    }
    public static Int32 DiagonalPowSum(Int32 v1, Int32 v2, Int32 n) {
        Contract.Requires(n >= 0);
        BigInteger a = v1;
        BigInteger b = v2;
        BigInteger m = (a - b) * (1L << 32);
        BigInteger numerator = BigInteger.ModPow(a, n, m)
                             - BigInteger.ModPow(b, n, m);
        return (Int32)(Int64)((numerator / (a - b)) % (1L << 32));
    }
    public static Int32 TrianglePowSum(Int32 v1, Int32 v2, Int32 n) {
        Contract.Requires(n >= 0);
        BigInteger a = v1;
        BigInteger b = v2;
        BigInteger ma = (a - 1) * (a - b) * (1L << 32);
        BigInteger mb = (b - 1) * (a - b) * (1L << 32);
        BigInteger numerator = BigInteger.ModPow(a, n + 1, ma) * (b - 1)
                             - BigInteger.ModPow(b, n + 1, mb) * (a - 1)
                             + a - b;
        BigInteger denominator = (a - 1) * (b - 1) * (a - b);
        return (Int32)(Int64)((numerator / denominator) % (1L << 32));
    }
    public static IEnumerable<Int32> DiagonalPowSums(Int32 v1, Int32 v2) {
        for (int n = 1; true; n++)
            yield return DiagonalPowSum(v1, v2, n);
    }
    public static IEnumerable<Int32> TrianglePowSums(Int32 v1, Int32 v2) {
        for (int n = 1; true; n++)
            yield return TrianglePowSum(v1, v2, n);
    }
    public static IEnumerable<Int32> InvPlusThird(Int32 n) {
        Contract.Ensures(Contract.ForAll(Contract.Result<IEnumerable<Int32>>(), i => i + i / 3 == n));
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
        Int32 total = 0;
        foreach (var value in values)
            total += value;
        return total;
    }
    public static IEnumerable<Int32> Powers(this Int32 b) {
        int t = 1;
        while (true) {
            yield return t;
            t *= b;
        }
    }
    public static bool DoesAdditionOverflowS32(Int32 v1, Int32 v2) {
        return v1 + v2 == (Int64)v1 + (Int64)v2;
    }
}
