using System;
using System.Collections.Generic;
using System.Linq;
using Strilanc.LinqToCollections;

class Linear {
    public readonly IReadOnlyDictionary<string, int> Values;
    public Linear(IEnumerable<KeyValuePair<string, int>> values) {
        if (values == null) throw new ArgumentNullException("values");
        this.Values = values.Where(e => e.Value != 0).ToDictionary(e => e.Key, e => e.Value);
    }
    public Linear(IReadOnlyDictionary<string, int> values) {
        if (values == null) throw new ArgumentNullException("values");
        this.Values = values;
    }
    public Linear() {
        this.Values = new Dictionary<string, int>();
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
        return new Linear(v1.Values.Keys.Union(v2.Values.Keys).ToDictionary(k => k, k => v1[k] + v2[k]));
    }
    public static Linear operator -(Linear v1, Linear v2) {
        return new Linear(v1.Values.Keys.Union(v2.Values.Keys).ToDictionary(k => k, k => v1[k] - v2[k]));
    }
    public static Linear operator *(Linear v, int factor) {
        return new Linear(v.Values.Keys.ToDictionary(k => k, k => v[k] * factor));
    }
    public static Linear operator *(int factor, Linear v) {
        return v*factor;
    }
    public int Dot(Linear other) {
        return Values.Keys.Union(other.Values.Keys).Select(k => this[k]*other[k]).SumWrap();
    }
    private static string bin(int v) {
        unchecked {
            var u = (uint)v;
            var s = Enumerable.Range(0, 32).Select(e => ""+((u >> e) & 1));
            var n = s.TakeWhile(e => e == "0").Take(31).Count();
            s = Enumerable.Repeat(".", n).Concat(s.Skip(n));
            s = s.Reverse();
            n = s.TakeWhile(e => e == "0").Take(31).Count();
            s = Enumerable.Repeat(".", n).Concat(s.Skip(n));
            return string.Join("", s);
        }
    }
    public override string ToString() {
        return string.Join(Environment.NewLine, Values.OrderBy(e => e.Key).Select(e => string.Format("{0}: {1}", e.Key, bin(e.Value))));
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

        var s1 = a.ToString();
        var s2 = b.ToString();
        var sv = v.ToString();
        var va = a.Dot(v);
        var vb = b.Dot(v);
        return new HashState(va, vb);
    }
    public static void Break(int resultA, int resultB, int assumedLength) {
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
                foreach (var i in 2.Range()) {
                    a *= -6;
                    a += b;
                    a += offsetA;
                    a -= e;
                    var k = "rem" + ss[iters] + ss[i];
                    b -= new Linear(k, 1);
                    //u.Add(new UnknownValue(k, -2, -1, 0, +1, +2));
                    //r.Add(new Equation(b, 3));
                    vars.Add("F{0}".Inter(k));
                    vars.Add(k);
                    eqs.Add("{1} +5 F{0} = 0".Inter(k, b.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" ")));
                    eqs.Add("+{0} <= 2".Inter(k));
                    eqs.Add("+{0} >= -2".Inter(k));

                    b *= inv3;
                    b += a;
                    b += offsetB;
                    b -= e;
                }
                iters += 1;
            }
        }

        eqs.Add("+a = {0}".Inter(resultA));
        eqs.Add("+b = {0}".Inter(resultB));
        eqs.Add("+offA = {0}".Inter(0x74FA));
        eqs.Add("+offB = {0}".Inter(0x81BE));

        eqs.Add("{0} +{1} Fa - a = 0".Inter(a.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));
        eqs.Add("{0} +{1} Fb - b = 0".Inter(b.Values.Select(p => "{0:+#;-#;0} {1}".Inter(p.Value, p.Key)).Join(" "), 1L << 32));
        vars.Add("Fa");
        vars.Add("Fb");
        vars.Add("offA");
        vars.Add("offB");
        vars.Add("a");
        vars.Add("b");

        //u.Add(new UnknownValue("offA", 0x74FA));
        //u.Add(new UnknownValue("offB", 0x81BE));
        //u.Add(new UnknownValue("a", resultA));
        //u.Add(new UnknownValue("b", resultB));

        //// create equation system to solve
        //r.Add(new Equation(a - new Linear("a", resultA)));
        //r.Add(new Equation(b - new Linear("b", resultA)));

        //var n = 0;
        //foreach (var eq in r) {
        //    r.Add(n+"");
        //}
        var min = new[] {"min: "};// + mins.Select(e => "+" + e).Join(" ")};

        var minimize = "Minimize{0}\tobj:{0}".Inter(Environment.NewLine);
        var subjectTo = "Subject To{0}{1}{0}".Inter(Environment.NewLine, eqs.Select(e => "\t" + e).Join(Environment.NewLine));
        var general = "General{0}{1}{0}".Inter(Environment.NewLine, vars.Select(e => "\t" + e).Join(Environment.NewLine));

        var r = minimize + subjectTo + general + "End";
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
    public readonly Linear Zero;
    public int? Modulo;
    public Equation(Linear zero, int? mod = null) {
        this.Zero = zero;
        this.Modulo = mod;
    }
}
public static class Util {
    public static bool BitAt(this int i, int offset) {
        return ((i >> offset) & 1) != 0;
    }
    public static int MaskBitsAbove(this int i, int offset) {
        return i & ((1 << (offset + 1)) - 1);
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
}