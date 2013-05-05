using System;
using System.Collections.Generic;
using System.Linq;
using Strilanc.LinqToCollections;

static class Hash4 {
    public static int[] Break(HashState result, int assumedLength, HashState start = default(HashState)) {
        if (assumedLength == 10)
        {
            foreach (var e in MainHash.DataRange)
            {
                var r = Break(result, assumedLength - 1, start.Advance(e));
                if (r != null)
                {
                    return new[] {e}.Concat(r).ToArray();
                }
            }
            return null;
        }

        var inv3 = MathEx.MultiplicativeInverseS32(3);
        var numRounds = 17;

        var numExpandOutward = Math.Max(0, Math.Min(Math.Min(5, assumedLength - 1), (assumedLength * 2) / 3));
        var filter = HashStateBloomFilter.Gen(start, numExpandOutward, 0.001);

        long printTicker = 0;
        var matches = new Dictionary<HashState, int[]>();
        Action<int, HashState> advanceData = null;
        Action<int, int, int, HashState> advanceRounds = null;
        var assignments = new int[assumedLength + 1 + assumedLength*numRounds];
        advanceRounds = (dataIndex, dataValue, round, h) => {
            if (round == -1) {
                advanceData(dataIndex - 1, h);
                return;
            }

            int[] prevBs;
            {
                var q = h.B - h.A - 0x81BE + dataValue;
                if (q < Int32.MinValue / 3 || q > Int32.MaxValue / 3) return;

                var n = q * 3;
                if (n == 0) {
                    prevBs = new[] { 0, -1, +1, -2, +2 };
                }
                else if (q == Int32.MaxValue / 3) {
                    prevBs = new[] { n, n + 1 };
                }
                else if (n > 0) {
                    prevBs = new[] { n, n + 1, n + 2 };
                }
                else {
                    prevBs = new[] { n, n - 1, n - 2 };
                }
            }
            try {
                foreach (var prevB in prevBs) {
                    assignments[1 + assumedLength + dataIndex * numRounds + round] = prevB % 3;

                    int[] prevAs;
                    {
                        var ax = h.A - 0x74FA + dataValue - prevB;
                        if (ax % 2 != 0) continue;
                        ax /= 2;
                        ax *= -inv3;
                        prevAs = new[] { ax, ax ^ (1 << 31) };
                    }

                    foreach (var prevA in prevAs) {
                        advanceRounds(dataIndex, dataValue, round - 1, new HashState(prevA, prevB));
                    }
                }
            }
            finally {
                assignments[1 + assumedLength + dataIndex * numRounds + round] = 0;
            }
        };
        advanceData = (dataIndex, h) => {
                if (dataIndex < numExpandOutward) {
                    if (filter.MayContain(h)) {
                        matches[h] = assignments.ToArray();
                    }
                    return;
                }

                foreach (var dataValue in MainHash.DataRange) {
                    assignments[1 + dataIndex] = dataValue;
                    advanceRounds(dataIndex, dataValue, numRounds - 1, h);

                    printTicker += 1;
                    if (printTicker == 1000000) {
                        printTicker = 0;
                        var s3 = assumedLength.Range().Skip(numExpandOutward).Reverse().Select(i => {
                            var s1 = assignments[i + 1];
                            var s2 = string.Join("", assignments.Skip(1 + assumedLength + i * 17).Take(17).Reverse().Select(Math.Abs).Reverse());
                            return s1 + ":" + s2;
                        });
                        Console.WriteLine(string.Join(", ", s3));
                    }
                }
                assignments[1 + dataIndex] = 0;
            };

        advanceData(assumedLength - 1, result);

        if (matches.Count == 0) return null;
        foreach (var match in matches)
        {
            var r = Break(match.Key, numExpandOutward, start);
            if (r != null) return r;
        }
        return null;
    }
}
class HashStateBloomFilter {
    private readonly ulong[] _bits;
    public double falsePositiveRateEstimate;
    private static HashState? _genState;
    private static Dictionary<int, HashStateBloomFilter> _lastGenned;
    public HashStateBloomFilter(int power, double pFalsePositive) {
        var n = (long)Math.Pow(MainHash.DataRange.Length, Math.Max(power, 2));
        var m = -(long)((n * Math.Log(pFalsePositive)) / (Math.Log(2) * Math.Log(2)));
        var k = m * Math.Log(2) / n;
        m = 1L << (int)Math.Ceiling(Math.Log(m, 2));
        m = Math.Min(m, 1L << 33);
        if (m < 32) m = 32;
        this._bits = new ulong[m >> 6];
    }
    public static HashStateBloomFilter Gen(HashState start, int pow, double pFalsePositive) {
        if (!Equals(_genState, start)) {
            _genState = start;
            _lastGenned = new Dictionary<int, HashStateBloomFilter>();
        }
        if (_lastGenned.ContainsKey(pow))
            return _lastGenned[pow];
        HashStateBloomFilter x;
        if (pow == 5) {
            x = Gen5(start, pFalsePositive);
        }
        else if (pow == 4) {
            x = Gen4(start, pFalsePositive);
        }
        else {
            x = new HashStateBloomFilter(pow, pFalsePositive);
            foreach (var e in start.Explore(pow)) {
                x.Add(e);
            }
        }
        _lastGenned[pow] = x;
        return x;
    }
    public static HashStateBloomFilter Gen4(HashState start, double pFalsePositive) {
        var x = new HashStateBloomFilter(4, pFalsePositive);
        foreach (var d1 in MainHash.DataRange) {
            var start1 = start.Advance(d1);
            foreach (var d2 in MainHash.DataRange) {
                var start2 = start1.Advance(d2);
                foreach (var d3 in MainHash.DataRange) {
                    var start3 = start2.Advance(d3);
                    foreach (var d4 in MainHash.DataRange) {
                        x.Add(start3.Advance(d4));
                    }
                }
            }
        }
        return x;
    }
    public static HashStateBloomFilter Gen5(HashState start, double pFalsePositive) {
        var x = new HashStateBloomFilter(5, pFalsePositive);
        long n = 0;
        long h = 0;
        foreach (var d1 in MainHash.DataRange) {
            var start1 = start.Advance(d1);
            foreach (var d2 in MainHash.DataRange) {
                var start2 = start1.Advance(d2);
                foreach (var d3 in MainHash.DataRange) {
                    var start3 = start2.Advance(d3);
                    foreach (var d4 in MainHash.DataRange) {
                        var start4 = start3.Advance(d4);
                        foreach (var d5 in MainHash.DataRange)
                        {
                            var start5 = start4.Advance(d5);
                            if (x.MayContain(start5)) h += 1;
                            else x.Add(start5);
                            n += 1;
                        }
                    }
                }
            }
        }
        x.falsePositiveRateEstimate = h/(double)n; 
        return x;
    }
    private bool this[ulong index]
    {
        get
        {
            ulong m = ((ulong)_bits.Length << 6) - 1;
            return ((_bits[(index & m) >> 6] >> (int)(index & 63)) & 1) != 0;
        }
        set
        {
            ulong m = ((ulong)_bits.Length << 6) - 1;
            _bits[(index & m) >> 6] |= 1ul << (int)(index & 63);            
        }
    }
    private static ulong Comb(int v1, int v2) {
        return (uint)v1 | ((ulong)(uint)v2 << 32);
    }
    private static ulong Rot(ulong v, int r)
    {
        return (v << r) | (v >> r);
    }
    public bool MayContain(HashState h) {
        var v = Comb(h.A, h.B);
        var v2 = Comb(h.A ^ h.B, h.A + h.B);
        var v3 = Comb(h.A - h.B, (h.A * 0x5bd1e995) ^ h.B);
        return this[v]
            && this[Rot(v, 32)]
            && this[v2]
            && this[Rot(v2, 32)]
            && this[v3]
            && this[Rot(v3, 32)];
    }
    public void Add(HashState h) {
        var v = Comb(h.A, h.B);
        var v2 = Comb(h.A ^ h.B, h.A + h.B);
        var v3 = Comb(h.A - h.B, (h.A * 0x5bd1e995) ^ h.B);
        this[v] = true;
        this[Rot(v, 32)] = true;
        this[v2] = true;
        this[Rot(v2, 32)] = true;
        this[v3] = true;
        this[Rot(v3, 32)] = true;
    }
}
static class Util {
    public static bool BitAt(this int i, int offset) {
        return ((i >> offset) & 1) != 0;
    }
    public static int MaskBitsAbove(this int i, int offset) {
        return i & ((1 << (offset + 1)) - 1);
    }
    public static int MaskBitsExcept(this int i, int offset) {
        return i & (1 << offset);
    }
    public static bool HasSetBitsUpTo(this int i, int offset) {
        return i.MaskBitsAbove(offset) != 0;
    }
    public static string Inter(this string format, params object[] args) {
        return string.Format(format, args);
    }
    public static string Join<T>(this IEnumerable<T> e, string joiner) {
        return string.Join(joiner, e);
    }
    public static KeyValuePair<K, VOut> SelectVal<K, VIn, VOut>(this KeyValuePair<K, VIn> v, Func<VIn, VOut> p) {
        return new KeyValuePair<K, VOut>(v.Key, p(v.Value));
    }
    public static IEnumerable<KeyValuePair<K, VOut>> LiftSelectVal<K, VIn, VOut>(this IEnumerable<KeyValuePair<K, VIn>> v, Func<VIn, VOut> p) {
        return v.Select(e => e.SelectVal(p));
    }
    public static string Bin(this int v) {
        unchecked {
            var u = (uint)v;
            var s = Enumerable.Range(0, 32).Select(e => "" + ((u >> e) & 1));
            var n = s.TakeWhile(e => e == "0").Take(31).Count();
            s = Enumerable.Repeat(".", n).Concat(s.Skip(n));
            s = s.Reverse();
            n = s.TakeWhile(e => e == "0").Take(31).Count();
            s = Enumerable.Repeat(".", n).Concat(s.Skip(n));
            return string.Join("", s);
        }
    }
    public static T ChooseRandom<T>(this IReadOnlyList<T> list, Random rng) {
        return list[rng.Next(list.Count)];
    }
    public static T ChooseScanRandom<T>(this IEnumerable<T> list, Random rng) {
        return list.ToArray().ChooseRandom(rng);
    }
    public static HashSet<T> ToSet<T>(this IEnumerable<T> x) {
        return new HashSet<T>(x);
    }
    public static Stack<T> ToStack<T>(this IEnumerable<T> x) {
        return new Stack<T>(x);
    }
}