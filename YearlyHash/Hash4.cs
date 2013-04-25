﻿using System;
using System.Collections.Generic;
using System.Linq;
using Strilanc.LinqToCollections;
using System.Collections.Immutable;

class Finear {
    public readonly int[] Values;
    public Finear(int[] values, bool optimize = true) {
        this.Values = values;
        if (!optimize) return;
        var n = values.Length;
        while (n > 0 && values[n - 1] == 0) {
            n -= 1;
        }
        if (n != values.Length) {
            Values = values.Take(n).ToArray();
        }
    }
    public Finear(int offset, int value) {
        if (value == 0) {
            this.Values = new int[0];
        } else {
            this.Values = new int[offset + 1];
            this.Values[offset] = value;
        }
    }
    public Finear() {
        this.Values = new int[0];
    }
    public int this[int offset] {
        get {
            if (offset >= Values.Length) return 0;
            return Values[offset];
        }
    }
    public static Finear operator +(Finear v1, Finear v2) {
        var r = new int[Math.Max(v1.Values.Length, v2.Values.Length)];
        for (var i = 0; i < v1.Values.Length; i++)
            r[i] += v1.Values[i];
        for (var i = 0; i < v2.Values.Length; i++)
            r[i] += v2.Values[i];
        return new Finear(r);
    }
    public static Finear operator -(Finear v1, Finear v2) {
        var r = new int[Math.Max(v1.Values.Length, v2.Values.Length)];
        for (var i = 0; i < v1.Values.Length; i++)
            r[i] += v1.Values[i];
        for (var i = 0; i < v2.Values.Length; i++)
            r[i] -= v2.Values[i];
        return new Finear(r);
    }
    public static Finear operator *(Finear v, int factor) {
        var r = v.Values.ToArray();
        for (var i = 0; i < r.Length; i++)
            r[i] *= factor;
        return new Finear(r);
    }
    public static Finear operator *(int factor, Finear v) {
        return v * factor;
    }
    public int Dot(Finear other) {
        var r = 0;
        var n = Math.Min(Values.Length, other.Values.Length);
        for (var i = 0; i < n; i++)
            r += Values[i]*other[i];
        return r;
    }
    public static implicit operator Finear(int v) {
        return new Finear(0, v);
    }
    public override string ToString() {
        return Values.Length.Range().Where(i => Values[i] != 0).Select(e => "[{0}]*{1}".Inter(e, Values[e].Bin())).Join(" + ");
    }
}
class Linear {
    public readonly IReadOnlyDictionary<string, int> Values;
    public readonly int Offset;
    public Linear(IEnumerable<KeyValuePair<string, int>> values, int offset) {
        if (values == null) throw new ArgumentNullException("values");
        this.Values = values.Where(e => e.Value != 0).ToDictionary(e => e.Key, e => e.Value);
        this.Offset = offset;
    }
    public Linear(IReadOnlyDictionary<string, int> values, int offset) {
        if (values == null) throw new ArgumentNullException("values");
        this.Values = values;
        this.Offset = offset;
    }
    public Linear() {
        this.Values = new Dictionary<string, int>();
        this.Offset = 0;
    }
    public Linear(string key, int value) {
        this.Values = new Dictionary<string, int> {
            {key, value}
        };
    }
    public int this[string key] {
        get {
            int r;
            if (!Values.TryGetValue(key, out r)) return 0;
            return r;
        }
    }
    public static Linear operator +(Linear v1, Linear v2) {
        return new Linear(v1.Values.Keys.Union(v2.Values.Keys).ToDictionary(k => k, k => v1[k] + v2[k]), v1.Offset + v2.Offset);
    }
    public static Linear operator -(Linear v1, Linear v2) {
        return new Linear(v1.Values.Keys.Union(v2.Values.Keys).ToDictionary(k => k, k => v1[k] - v2[k]), v1.Offset - v2.Offset);
    }
    public static Linear operator *(Linear v, int factor) {
        return new Linear(v.Values.Keys.ToDictionary(k => k, k => v[k] * factor), v.Offset * factor);
    }
    public static Linear operator *(int factor, Linear v) {
        return v*factor;
    }
    public int Dot(Linear other) {
        return Values.Keys.Union(other.Values.Keys).Select(k => this[k]*other[k]).SumWrap() + Offset * other.Offset;
    }
    public static implicit operator Linear(int v) {
        return new Linear(new Dictionary<string, int>(), v);
    }
    public override string ToString() {
        return string.Join(Environment.NewLine, Values.OrderBy(e => e.Key).Select(e => string.Format("{0}: {1}", e.Key, e.Value.Bin()))) + Environment.NewLine + "const: " + Offset.Bin();
    }
}
static class Hash4 {
    public static HashState Hash(IEnumerable<Int32> data) {
        var inv3 = MathEx.MultiplicativeInverseS32(3);
        var a = new Linear();
        var b = new Linear();
        var offsetA = new Linear("offsta", 1);
        var offsetB = new Linear("offstb", 1);
        var dataLin = data.Select((e, i) => new Linear("data " + i, 1)).ToArray();
        var v = new Linear("offsta", 0x74FA)
            + new Linear("offstb", 0x81BE) 
            + data.Select((e, i) => new Linear("data " + i, e)).Aggregate(new Linear(), (e1, e2) => e1 + e2);
        var iters = 0;
        unchecked {
            foreach (var e in dataLin) {
                for (var i = 0; i < 17; i++) {
                    a *= -6;
                    a += b;
                    a += offsetA;
                    a -= e;
                    var k = "r" + iters.ToString("00") + "." + i.ToString("00");
                    v += new Linear(k, b.Dot(v)%3);
                    b -= new Linear(k, 1);
                    b *= inv3;
                    b += a;
                    b += offsetB;
                    b -= e;
                }
                iters += 1;
            }
        }

        var va = a.Dot(v);
        var vb = b.Dot(v);
        return new HashState(va, vb);
    }
    public static string BreakIntoIntegerProblem(int resultA, int resultB, int assumedLength) {
        var inv3 = MathEx.MultiplicativeInverseS32(3);
        var a = new Linear();
        var b = new Linear();
        var offsetA = new Linear("offA", 1);
        var offsetB = new Linear("offB", 1);
        var ss = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var dataLin = Enumerable.Range(0, assumedLength).Select((e, i) => new Linear("val" + ss[i], 1)).ToArray();
        var iters = 0;
        var eqs = new List<string>();
        var vars = new List<string>();
        unchecked {
            foreach (var e in dataLin) {
                vars.Add("val{0}".Inter(ss[iters]));
                eqs.Add("+val{0} >= {1}".Inter(ss[iters], MainHash.MinDataValue));
                eqs.Add("+val{0} <= {1}".Inter(ss[iters], MainHash.MaxDataValue));
                foreach (var i in 17.Range()) {
                    a *= -6;
                    a += b;
                    a += offsetA;
                    a -= e;
                    var k = "rem" + ss[iters] + ss[i];
                    b -= new Linear(k, 1);

                    vars.Add(k);
                    vars.Add("{0}_range".Inter(k));
                    vars.Add("{0}_sign".Inter(k));
                    vars.Add("{0}_mul3".Inter(k));

                    eqs.Add("{0}_sign >= 0".Inter(k));
                    eqs.Add("{0}_sign <= 1".Inter(k));
                    eqs.Add("{0} <= 2".Inter(k));
                    eqs.Add("{0} >= -2".Inter(k));
                    eqs.Add("{0} +2 {0}_sign >= 0".Inter(k));
                    eqs.Add("{0} +2 {0}_sign <= 2".Inter(k));
                    eqs.Add("{0} +{1} {0}_range >= {2}".Inter(k, 1L << 32, int.MinValue));
                    eqs.Add("{0} +{1} {0}_range <= {2}".Inter(k, 1L << 32, int.MaxValue));
                    eqs.Add("{0} +{1} {0}_range +{2} {0}_sign >= 0".Inter(k, 1L << 32, 1L << 31));
                    eqs.Add("{0} +{1} {0}_range +{2} {0}_sign <= {2}".Inter(k, 1L << 32, 1L << 31));

                    eqs.Add("{1} +{2} {0}_range +3 {0}_mul3 = 0".Inter(k, b.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));

                    b *= inv3;
                    b += a;
                    b += offsetB;
                    b -= e;
                }
                iters += 1;
            }
        }

        eqs.Add("a = {0}".Inter(resultA));
        eqs.Add("b = {0}".Inter(resultB));
        eqs.Add("offA = {0}".Inter(0x74FA));
        eqs.Add("offB = {0}".Inter(0x81BE));

        eqs.Add("{0} +{1} Fa - a = 0".Inter(a.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));
        eqs.Add("{0} +{1} Fb - b = 0".Inter(b.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));
        vars.Add("Fa");
        vars.Add("Fb");
        vars.Add("offA");
        vars.Add("offB");
        vars.Add("a");
        vars.Add("b");

        var minimize = "min: ;{0}".Inter(Environment.NewLine);
        var subjectTo = eqs.Select(e => e + ";" + Environment.NewLine).Join("");
        var general = vars.Select(e => "int " + e + ";" + Environment.NewLine).Join("");

        return minimize + subjectTo + general;
    }
    public static string BreakIntoIntegerProblem2(int resultA, int resultB, int assumedLength) {
        var inv3 = MathEx.MultiplicativeInverseS32(3);

        var a = new Linear();
        var b = new Linear();
        var offsetA = new Linear("offA", 1);
        var offsetB = new Linear("offB", 1);
        var ss = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var dataLin = Enumerable.Range(0, assumedLength).Select((e, i) => new Linear("val" + ss[i], 1)).ToArray();
        var iters = 0;
        var eqs = new List<string>();
        var vars = new List<string>();
        unchecked {
            foreach (var e in dataLin) {
                vars.Add("val{0}".Inter(ss[iters]));
                eqs.Add("+val{0} >= {1}".Inter(ss[iters], MainHash.MinDataValue));
                eqs.Add("+val{0} <= {1}".Inter(ss[iters], MainHash.MaxDataValue));
                foreach (var i in 17.Range()) {
                    a *= -6;
                    a += b;
                    a += offsetA;
                    a -= e;
                    var k = "rem" + ss[iters] + ss[i];
                    b -= new Linear(k, 1);

                    vars.Add(k);
                    vars.Add("{0}_range".Inter(k));
                    vars.Add("{0}_sign".Inter(k));
                    vars.Add("{0}_mul3".Inter(k));

                    eqs.Add("{0}_sign >= 0".Inter(k));
                    eqs.Add("{0}_sign <= 1".Inter(k));
                    eqs.Add("{0} <= 2".Inter(k));
                    eqs.Add("{0} >= -2".Inter(k));
                    eqs.Add("{0} +2 {0}_sign >= 0".Inter(k));
                    eqs.Add("{0} +2 {0}_sign <= 2".Inter(k));
                    eqs.Add("{0} +{1} {0}_range >= {2}".Inter(k, 1L << 32, int.MinValue));
                    eqs.Add("{0} +{1} {0}_range <= {2}".Inter(k, 1L << 32, int.MaxValue));
                    eqs.Add("{0} +{1} {0}_range +{2} {0}_sign >= 0".Inter(k, 1L << 32, 1L << 31));
                    eqs.Add("{0} +{1} {0}_range +{2} {0}_sign <= {2}".Inter(k, 1L << 32, 1L << 31));

                    eqs.Add("{1} +{2} {0}_range +3 {0}_mul3 = 0".Inter(k, b.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));

                    b *= inv3;
                    b += a;
                    b += offsetB;
                    b -= e;
                }
                iters += 1;
            }
        }

        eqs.Add("a = {0}".Inter(resultA));
        eqs.Add("b = {0}".Inter(resultB));
        eqs.Add("offA = {0}".Inter(0x74FA));
        eqs.Add("offB = {0}".Inter(0x81BE));

        eqs.Add("{0} +{1} Fa - a = 0".Inter(a.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));
        eqs.Add("{0} +{1} Fb - b = 0".Inter(b.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));
        vars.Add("Fa");
        vars.Add("Fb");
        vars.Add("offA");
        vars.Add("offB");
        vars.Add("a");
        vars.Add("b");

        var minimize = "Minimize{0} obj:{0}".Inter(Environment.NewLine);
        var subjectTo = "Subject To" + Environment.NewLine + eqs.Select(e => " " + e + Environment.NewLine).Join("");
        var bounds = "Bounds" + Environment.NewLine + vars.Select(e => " " + e + " free" + Environment.NewLine).Join("");
        var general = "General" + Environment.NewLine + vars.Select(e => " " + e + Environment.NewLine).Join("");

        return minimize + subjectTo + bounds + general + "End";
    }
    private static IEnumerable<Fequation> GenerateConstraints(HashState result, int assumedLength, HashState start = default(HashState)) {
        var inv3 = MathEx.MultiplicativeInverseS32(3);
        var numRounds = 17;

        var a = (Finear)start.A;
        var b = (Finear)start.B;
        var eqs = new List<Fequation>();
        unchecked {
            foreach (var i in assumedLength.Range()) {
                var e = new Finear(1 + i, 1);
                foreach (var j in numRounds.Range()) {
                    a *= -6;
                    a += b;
                    a += 0x74FA;
                    a -= e;
                    var k = new Finear(1 + assumedLength + i * numRounds + j, 1);
                    eqs.Add(new Fequation(b, k, 3));
                    b -= k;

                    b *= inv3;
                    b += a;
                    b += 0x81BE;
                    b -= e;
                }
            }
        }

        eqs.Add(new Fequation(a, result.A));
        eqs.Add(new Fequation(b, result.B));
        return eqs;
    }
    public static int[] Break(HashState result, int assumedLength, HashState start = default(HashState)) {
        var inv3 = MathEx.MultiplicativeInverseS32(3);
        var numRounds = 17;
        var eqs = GenerateConstraints(result, assumedLength, start);

        var bitSplitEquations = eqs.SelectMany(e => e.BitSplit()).Where(e => !e.Right.Values.SequenceEqual(e.Left.Values)).ToImmutableList();
        var numDimensions = 1 + assumedLength + assumedLength*numRounds;
        var equationsToCheck = new Fequation[assumedLength,numRounds][];
        var remainingEquations = bitSplitEquations;
        var assignedDimensionIndexes = ImmutableHashSet.Create<int>().Add(0);
        foreach (var dataIndex in assumedLength.Range().Reverse()) {
            assignedDimensionIndexes = assignedDimensionIndexes.Add(1 + dataIndex);
            foreach (var round in numRounds.Range().Reverse()) {
                assignedDimensionIndexes = assignedDimensionIndexes.Add(1 + assumedLength + dataIndex*numRounds + round);
                var ready =
                    remainingEquations
                        .Where(e => numDimensions.Range().All(i => (e.Left[i] == 0 && e.Right[i] == 0) || assignedDimensionIndexes.Contains(i)))
                        .ToArray();
                remainingEquations = remainingEquations.RemoveRange(ready);
                equationsToCheck[dataIndex, round] = ready;
            }
        }

        var numExpandOutward = Math.Max(0, Math.Min(4, assumedLength - 1));
        var filter = HashStateBloomFilter.Gen(start, numExpandOutward, 0.001);

        long xx = 0;
        long nn = 0;
        var yyf = new long[assumedLength, numRounds];
        var yy = new long[assumedLength, numRounds];
        var matches = new Dictionary<HashState, int[]>();
        Action<int, HashState> advanceData = null;
        Action<int, int, int, HashState> advanceRounds = null;
        var assignments = new int[numDimensions];
        assignments[0] = 1;
        var af = new Finear(assignments, optimize: false);
        advanceRounds = (dataIndex, dataValue, round, h) => {
            if (round == -1) {
                advanceData(dataIndex - 1, h);
                return;
            }

            var eqc = equationsToCheck[dataIndex, round];
            xx -= 1;

            int[] prevBs;
            {
                var q = h.B - h.A - 0x81BE + dataValue;
                if (q < Int32.MinValue / 3 || q > Int32.MaxValue / 3) return;

                var n = q * 3;
                if (n == 0) {
                    prevBs = new[] { -2, -1, 0, +1, +2 };
                } else if (q == Int32.MaxValue / 3) {
                    prevBs = new[] { n, n + 1 };
                } else if (n > 0) {
                    prevBs = new[] { n, n + 1, n + 2 };
                } else {
                    prevBs = new[] { n - 2, n - 1, n };
                }
            }
            try {
                foreach (var prevB in prevBs) {
                    assignments[1 + assumedLength + dataIndex * numRounds + round] = prevB % 3;
                    xx += 1;
                    yyf[dataIndex, round] += 1;
                    if (eqc.Length > 0 && eqc.Any(eq => !eq.Satisfies(af))) {
                        yy[dataIndex, round] += 1;
                        continue;
                    }

                    xx -= 1;
                    int[] prevAs;
                    {
                        var ax = h.A - 0x74FA + dataValue - prevB;
                        if (ax % 2 != 0) continue;
                        ax /= 2;
                        ax *= -inv3;
                        prevAs = new[] { ax, ax ^ (1 << 31) };
                    }
                    foreach (var prevA in prevAs) {
                        xx += 1;
                        advanceRounds(dataIndex, dataValue, round - 1, new HashState(prevA, prevB));
                    }
                }
            } finally {
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
                advanceRounds(dataIndex, dataValue, numRounds-1, h);

                nn += 1;
                if (nn == 100000) {
                    nn = 0;
                    Console.WriteLine(string.Join(", ", assignments.Skip(1).Take(assumedLength).Skip(numExpandOutward).Reverse()));
                    Console.WriteLine(string.Join(
                        Environment.NewLine,
                        assumedLength.Range().Skip(numExpandOutward).Select(
                            d => string.Join(
                                ", ",
                                numRounds.Range().Select(
                                    r => yyf[d, r] == 0 ? "----" : (yy[d, r] / (double)yyf[d, r]).ToString("0.00"))))));
                }
            }
            assignments[1 + dataIndex] = 0;
        };

        advanceData(assumedLength - 1, result);

        if (matches.Count == 0) return null;
        var head = start.ExploreTraceVolatile(numExpandOutward).FirstOrDefault(e => matches.ContainsKey(e.Item1));
        var x = head == null ? null : head.Item2.Concat(matches[head.Item1].Skip(1 + numExpandOutward).Take(assumedLength - numExpandOutward)).ToArray();
        return x;
    }
}
class UnknownValue {
    public readonly string Key;
    public readonly List<int> PossibleValues;
    public UnknownValue(string key, params int[] possibleValues) {
        this.Key = key;
        this.PossibleValues = possibleValues.ToList();
    } 
}
class Equation {
    public readonly Linear Left;
    public readonly Linear Right;
    public int? Modulo;
    public Equation(Linear left, Linear right, int? mod = null) {
        this.Left = left;
        this.Right = right;
        this.Modulo = mod;
    }
    public override string ToString() {
        return Left.Values.Where(e => e.Value != 0).Select(e => "{1}*{0}".Inter(e.Key, e.Value.Bin())).Join(" + ") + " + " + Left.Offset.Bin() + (Modulo.HasValue ? " (mod {0})".Inter(Modulo.Value) : " (Int32")
            + " = " + Right.Values.Where(e => e.Value != 0).Select(e => "{1}*{0}".Inter(e.Key, e.Value.Bin())).Join(" + ") + " + " + Right.Offset.Bin();
    }
}
class Fequation {
    public readonly Finear Left;
    public readonly Finear Right;
    public int? Modulo;
    public Fequation(Finear left, Finear right, int? mod = null) {
        this.Left = left;
        this.Right = right;
        this.Modulo = mod;
    }
    public bool Satisfies(Finear assignedValues) {
        var leftVal = Left.Dot(assignedValues);
        var rightVal = Right.Dot(assignedValues);
        if (Modulo == 3) return leftVal % 3 == rightVal;
        if (Modulo.HasValue) return (leftVal & (Modulo.Value - 1)) == (rightVal & (Modulo.Value - 1));
        return leftVal == rightVal;
    }
    public override string ToString() {
        return Left + (Modulo.HasValue ? " (mod {0})".Inter(Modulo.Value) : " (Int32)") + " = " + Right;
    }
}
class HashStateBloomFilter {
    private readonly ulong[] _bits;
    private static HashState? _genState;
    private static Dictionary<int, HashStateBloomFilter> _lastGenned;
    public HashStateBloomFilter(int power, double pFalsePositive) {
        var n = (long)Math.Pow(MainHash.DataRange.Length, Math.Max(power, 2));
        var m = -(long)((n * Math.Log(pFalsePositive)) / (Math.Log(2) * Math.Log(2)));
        var k = m * Math.Log(2) / n;
        m = 1L << (int)Math.Ceiling(Math.Log(m, 2));
        m = Math.Min(m, 1L << 32);
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
        } else if (pow == 4) {
            x = Gen4(start, pFalsePositive);
        } else {
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
        foreach (var d1 in MainHash.DataRange) {
            var start1 = start.Advance(d1);
            foreach (var d2 in MainHash.DataRange) {
                var start2 = start1.Advance(d2);
                foreach (var d3 in MainHash.DataRange) {
                    var start3 = start2.Advance(d3);
                    foreach (var d4 in MainHash.DataRange) {
                        var start4 = start3.Advance(d4);
                        foreach (var d5 in MainHash.DataRange) {
                            x.Add(start4.Advance(d5));
                        }
                    }
                }
            }
        }
        return x;
    }
    public bool MayContain(HashState h) {
        var m = (_bits.Length << 6) - 1;
        var v1 = h.A & m;
        var v2 = h.B & m;
        var v3 = (v1 ^ v2) & m;
        var v4 = (v1 + v2) & m;
        var v5 = (v1 - v2) & m;
        var v6 = (h.A ^ (((h.B >> 16) & 0xFFFF) | (h.B << 16))) & m;
        return ((_bits[(uint)v1 >> 6] >> (v1 & 63)) & 1) != 0
            && ((_bits[(uint)v2 >> 6] >> (v2 & 63)) & 1) != 0
            && ((_bits[(uint)v3 >> 6] >> (v3 & 63)) & 1) != 0
            && ((_bits[(uint)v4 >> 6] >> (v4 & 63)) & 1) != 0
            && ((_bits[(uint)v5 >> 6] >> (v5 & 63)) & 1) != 0
            && ((_bits[(uint)v6 >> 6] >> (v6 & 63)) & 1) != 0;
    }
    public void Add(HashState h) {
        var m = (_bits.Length << 6) - 1;
        var v1 = h.A & m;
        var v2 = h.B & m;
        var v3 = (v1 ^ v2) & m;
        var v4 = (v1 + v2) & m;
        var v5 = (v1 - v2) & m;
        var v6 = (h.A ^ (((h.B >> 16) & 0xFFFF) | (h.B << 16))) & m;
        _bits[(uint)v1 >> 6] |= 1ul << (v1 & 63);
        _bits[(uint)v2 >> 6] |= 1ul << (v2 & 63);
        _bits[(uint)v3 >> 6] |= 1ul << (v3 & 63);
        _bits[(uint)v4 >> 6] |= 1ul << (v4 & 63);
        _bits[(uint)v5 >> 6] |= 1ul << (v5 & 63);
        _bits[(uint)v6 >> 6] |= 1ul << (v6 & 63);
    }
}
static class Util {
    public static IEnumerable<Fequation> BitSplit(this Fequation eq) {
        if (eq.Modulo.HasValue) {
            yield return eq;
            yield break;
        }
        foreach (var i in 32.Range()) {
            var leftE = new Finear(eq.Left.Values.Select(e => e.MaskBitsAbove(i)).ToArray());
            var rightE = new Finear(eq.Right.Values.Select(e => e.MaskBitsAbove(i)).ToArray());
            yield return new Fequation(leftE, rightE, i == 31 ? (int?)null : (2 << i));
        }
    }
    public static bool BitAt(this int i, int offset) {
        return ((i >> offset) & 1) != 0;
    }
    public static int MaskBitsAbove(this int i, int offset) {
        return i & ((1 << (offset + 1)) - 1);
    }
    public static int MaskBitsExcept(this int i, int offset) {
        return i & (1 << offset);
    }
    public static IEnumerable<UnknownValue> BitSplit(this UnknownValue v) {
        return 32.Range().Select(e => new UnknownValue(v.Key + "_" + e, 0, 1));
    }
    public static Linear BitSplitUpTo(this Linear v, int offset) {
        return new Linear(v.Values.SelectMany(e => (offset + 1).Range().Select(p => new KeyValuePair<string, int>(e.Key + "_" + p, e.Value.MaskBitsExcept(p)))), 0);
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