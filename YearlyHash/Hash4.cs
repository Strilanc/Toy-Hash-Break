﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Strilanc.LinqToCollections;
using System.Collections.Immutable;

class Finear {
    public readonly int[] Values;
    public Finear(int[] values) {
        this.Values = values;
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

        var a = (Finear)start.A;
        var b = (Finear)start.B;
        var eqs = ImmutableList.Create<Fequation>();
        unchecked {
            foreach (var i in assumedLength.Range()) {
                var e = new Finear(1 + i, 1);
                foreach (var j in numRounds.Range()) {
                    a *= -6;
                    a += b;
                    a += 0x74FA;
                    a -= e;
                    var k = new Finear(1 + assumedLength + i * numRounds + j, 1);
                    eqs = eqs.Add(new Fequation(b, k, 3));
                    b -= k;

                    b *= inv3;
                    b += a;
                    b += 0x81BE;
                    b -= e;
                }
            }
        }

        eqs = eqs.Add(new Fequation(a, result.A));
        eqs = eqs.Add(new Fequation(b, result.B));

        var numDimensions = 1 + assumedLength + assumedLength * numRounds;
        var equationsToCheck = new Fequation[assumedLength, numRounds][];
        var remainingEquations = eqs.SelectMany(e => e.BitSplit()).Where(e => !e.Right.Values.SequenceEqual(e.Left.Values)).ToImmutableList();
        var assignedDimensionIndexes = ImmutableHashSet.Create<int>().Add(0);
        foreach (var dataIndex in assumedLength.Range().Reverse()) {
            assignedDimensionIndexes = assignedDimensionIndexes.Add(1 + dataIndex);
            foreach (var round in numRounds.Range().Reverse()) {
                assignedDimensionIndexes = assignedDimensionIndexes.Add(1 + assumedLength + dataIndex * numRounds + round);
                var ready = 
                    remainingEquations
                    .Where(e => numDimensions.Range().All(i => (e.Left[i] == 0 && e.Right[i] == 0) || assignedDimensionIndexes.Contains(i)))
                    .ToArray();
                remainingEquations = remainingEquations.RemoveRange(ready);
                equationsToCheck[dataIndex, round] = ready;
            }
        }

        var numExpandOutward = 0;// Math.Max(0, Math.Min(4, assumedLength - 1));
        var filter = HashStateBloomFilter.Gen(start, numExpandOutward, 0.001);

        //var sss = "iampied";
        //var aaa = start.A;
        //var bbb = start.B;
        //var nnn = new List<HashState> { start };
        //foreach (var e in MainHash.Encode(sss)) {
        //    foreach (var _ in numRounds.Range()) {
        //        aaa *= -6;
        //        aaa += bbb;
        //        aaa += 0x74FA;
        //        aaa -= e;
        //        bbb /= 3;
        //        bbb += aaa;
        //        bbb += 0x81BE;
        //        bbb -= e;
        //        nnn.Add(new HashState(aaa, bbb));
        //    }
        //}
        //nnn.Add(result);

        var states = new[] {
            new {
                h = result,
                assignments = Enumerable.Repeat(0, numDimensions).ToImmutableList().SetItem(0, 1)
            }
        }.ToList();
        foreach (var dataIndex in assumedLength.Range().Skip(numExpandOutward).Reverse()) {
            var previousDataStates = states.Take(0).ToList();
            
            foreach (var dataValue in MainHash.DataRange) {
                var roundStates = states.ToList();
                foreach (var round in numRounds.Range().Reverse()) {
                    var previousRoundStates = roundStates.Take(0).ToList();
                    var eqc = equationsToCheck[dataIndex, round];

                    foreach (var state in roundStates) {
                        var as2 = state.assignments.SetItem(1 + dataIndex, dataValue);

                        foreach (var prevB in (state.h.B - state.h.A - 0x81BE + dataValue).InvDivS32(3)) {
                            var as3 = as2.SetItem(1 + assumedLength + dataIndex * numRounds + round, prevB % 3);
                            if (eqc.Length > 0) {
                                var asf = new Finear(as3.ToArray());
                                if (eqc.Any(eq => !eq.Satisfies(asf))) {
                                    //foreach (var prevA in (state.h.A - 0x74FA + dataValue - prevB).InvMulS32(-6)) {
                                    //    if (nnn.Contains(new HashState(prevA, prevB))) {
                                    //        var rrrr = 0;
                                    //    }
                                    //}
                                    continue;
                                }
                            }
                            foreach (var prevA in (state.h.A - 0x74FA + dataValue - prevB).InvMulS32(-6)) {
                                //if (nnn.Contains(new HashState(prevA, prevB))) {
                                    previousRoundStates.Add(new {h = new HashState(prevA, prevB), assignments = as3});
                                //}
                            }
                        }
                    }
                    roundStates = previousRoundStates;
                }
                var rrr = (dataIndex == numExpandOutward
                              ? roundStates.Where(e => filter.MayContain(e.h))
                              : roundStates); //.ToArray();
                //if (roundStates.Any(previousDataStates.Contains) && !rrr.Any(previousDataStates.Contains)) {
                //    var xxx = 0;
                //}
                previousDataStates.AddRange(rrr);
            }
            states = previousDataStates;
        }

        var tails = states.ToDictionary(e => e.h, e => e.assignments);
        if (tails.Count == 0) return null;
        var head = start.ExploreTraceVolatile(numExpandOutward).FirstOrDefault(e => tails.ContainsKey(e.Item1));
        return head == null ? null : head.Item2.Concat(tails[head.Item1].Skip(1 + numExpandOutward).Take(assumedLength - numExpandOutward)).ToArray();
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
    private readonly Int32[] _bits;
    private static HashState? _genState;
    private static Dictionary<int, HashStateBloomFilter> _lastGenned;
    public HashStateBloomFilter(int power, double pFalsePositive) {
        var n = (long)Math.Pow(MainHash.DataRange.Length, Math.Max(power, 2));
        var m = -(int)((n*Math.Log(pFalsePositive))/(Math.Log(2)*Math.Log(2)));
        var k = m * Math.Log(2) / n;
        m = 1 << (int)Math.Ceiling(Math.Log(m, 2));
        if (m < 0) m = 1 << 30;
        if (m < 32) m = 32;
        this._bits = new int[m / 32];
    }
    public static HashStateBloomFilter Gen(HashState start, int pow, double pFalsePositive) {
        if (!Equals(_genState, start)) {
            _genState = start;
            _lastGenned = new Dictionary<int, HashStateBloomFilter>();
        }
        if (_lastGenned.ContainsKey(pow))
            return _lastGenned[pow];
        HashStateBloomFilter x;
        if (pow == 4) {
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
    public bool MayContain(HashState h) {
        var m = (_bits.Length << 5) - 1;
        var v1 = h.A & m;
        var v2 = h.B & m;
        var v3 = (v1 ^ v2) & m;
        var v4 = (v1 + v2) & m;
        var v5 = (v1 - v2) & m;
        var v6 = (h.A ^ (((h.B >> 16) & 0xFFFF) | (h.B << 16))) & m;
        return ((_bits[v1 >> 5] >> (v1 & 31)) & 1) != 0
            && ((_bits[v2 >> 5] >> (v2 & 31)) & 1) != 0
            && ((_bits[v3 >> 5] >> (v3 & 31)) & 1) != 0
            && ((_bits[v4 >> 5] >> (v4 & 31)) & 1) != 0
            && ((_bits[v5 >> 5] >> (v5 & 31)) & 1) != 0
            && ((_bits[v6 >> 5] >> (v6 & 31)) & 1) != 0;
    }
    public void Add(HashState h) {
        var m = (_bits.Length << 5) - 1;
        var v1 = h.A & m;
        var v2 = h.B & m;
        var v3 = (v1 ^ v2) & m;
        var v4 = (v1 + v2) & m;
        var v5 = (v1 - v2) & m;
        var v6 = (h.A ^ (((h.B >> 16) & 0xFFFF) | (h.B << 16))) & m;
        _bits[v1 >> 5] |= 1 << (v1 & 31);
        _bits[v2 >> 5] |= 1 << (v2 & 31);
        _bits[v3 >> 5] |= 1 << (v3 & 31);
        _bits[v4 >> 5] |= 1 << (v4 & 31);
        _bits[v5 >> 5] |= 1 << (v5 & 31);
        _bits[v6 >> 5] |= 1 << (v6 & 31);
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
}