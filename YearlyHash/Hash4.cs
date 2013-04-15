using System;
using System.Collections.Generic;
using System.Linq;
using Strilanc.LinqToCollections;
using System.Collections.Immutable;

class Finear {
    public readonly int[] Values;
    public Finear(int[] values) {
        this.Values = values;
    }
    public Finear(int offset, int value) {
        this.Values = new int[offset + 1];
        this.Values[offset] = value;
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
                for (var i = 0; i < 2; i++) {
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

        var s1 = a.ToString();
        var s2 = b.ToString();
        var sv = v.ToString();
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
        //var r = new List<Equation>();
        //var u = new List<UnknownValue>();
        var eqs = new List<string>();
        var vars = new List<string>();
        unchecked {
            foreach (var e in dataLin) {
                vars.Add("val{0}".Inter(ss[iters]));
                eqs.Add("+val{0} >= 0".Inter(ss[iters], MainHash.CharSet.Length));
                eqs.Add("+val{0} < {1}".Inter(ss[iters], MainHash.CharSet.Length));
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
        //resultA = 0;
        //resultB = 0;
        //foreach (var i in 2.Range()) {
        //    resultA *= -6;
        //    resultA += resultB;
        //    resultA += 0x74FA;
        //    resultA -= 3;
        //    resultB /= 3;
        //    resultB += resultA;
        //    resultB += 0x81BE;
        //    resultB -= 3;
        //}

        var a = new Linear();
        var b = new Linear();
        var offsetA = new Linear("offA", 1);
        var offsetB = new Linear("offB", 1);
        var ss = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var dataLin = Enumerable.Range(0, assumedLength).Select((e, i) => new Linear("val" + ss[i], 1)).ToArray();
        var iters = 0;
        //var r = new List<Equation>();
        //var u = new List<UnknownValue>();
        var eqs = new List<string>();
        var vars = new List<string>();
        unchecked {
            foreach (var e in dataLin) {
                vars.Add("val{0}".Inter(ss[iters]));
                eqs.Add("+val{0} >= 0".Inter(ss[iters], MainHash.CharSet.Length));
                eqs.Add("+val{0} < {1}".Inter(ss[iters], MainHash.CharSet.Length));
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
    public static void Break(HashState result, int assumedLength) {
        var inv3 = MathEx.MultiplicativeInverseS32(3);

        var a = new Finear();
        var b = new Finear();
        var eqs = new List<Fequation>();
        unchecked {
            foreach (var i in assumedLength.Range()) {
                var e = new Finear(1 + i, 1);
                foreach (var j in 17.Range()) {
                    a *= -6;
                    a += b;
                    a += 0x74FA;
                    a -= e;
                    var k = new Finear(1 + assumedLength + i*17 + j, 1);
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

        var nk = (1 + assumedLength + assumedLength*17);
        var h = new[] { new { h = result, u = nk.Range().Select(e => (int?)null).ToImmutableList().SetItem(0, 1), eqs = eqs.ToImmutableList() } }.ToList();
        foreach (var i in assumedLength.Range().Reverse()) {
            var hn = h.ToList();
            hn.Clear();
            foreach (var e in MainHash.CharSet.Length.Range()) {
                var h3 = h.ToList();
                foreach (var j in 17.Range().Reverse()) {
                    var h2 = h3.ToList();
                    h2.Clear();
                    foreach (var x in h3) {
                        var Bs = MathEx.InvDivS32(x.h.B - x.h.A - 0x81BE + e, 3);
                        var k = 1 + assumedLength + i * 17 + j;

                        foreach (var pb in Bs) {
                            var As = MathEx.InvMulS32(x.h.A - 0x74FA + e - pb, -6);
                            var nu = x.u.SetItem(k, pb % 3).SetItem(1 + i, e);
                            foreach (var pa in As) {
                                var nh = new HashState(pa, pb);
                                var failed = false;
                                var ne = x.eqs;
                                foreach (var eq in ne.Where(eq => nk.Range().All(m => (eq.Left[m] == 0 && eq.Right[m] == 0) || nu[m].HasValue))) {
                                    ne = ne.Remove(eq);

                                    var curAssignedVals = new Finear(nu.Select(v => v ?? 0).ToArray());
                                    var left = eq.Left.Dot(curAssignedVals);
                                    if (eq.Modulo.HasValue) left %= eq.Modulo.Value;
                                    var right = eq.Right.Dot(curAssignedVals);
                                    if (left == right) continue;

                                    failed = true;
                                    break;
                                }
                                if (!failed) {
                                    h2.Add(new { h = nh, u = nu, eqs = ne });
                                }
                            }
                        }
                    }
                    h3 = h2;
                }
                hn.AddRange(h3);
            }
            h = hn;
        }

        var solution = h.FirstOrDefault(e => Equals(e.h, new HashState(0, 0)));
        if (solution != null) {
            var solutionR = solution.u.Skip(1).Take(assumedLength).Cast<int>().ToArray();

            return;
        }

        //u.Add(new UnknownValue("offA", 0x74FA));
        //u.Add(new UnknownValue("offB", 0x81BE));
        //u.Add(new UnknownValue("a", resultA));
        //u.Add(new UnknownValue("b", resultB));

        //// create equation system to solve
        //r.Add(new Equation(a - new Linear("a", resultA)));
        //r.Add(new Equation(b - new Linear("b", resultA)));

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
    public override string ToString() {
        return Left + (Modulo.HasValue ? " (mod {0})".Inter(Modulo.Value) : " (Int32)") + " = " + Right;
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