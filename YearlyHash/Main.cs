using System;
using System.Collections.Generic;
using System.Linq;

public static class MainHash {
    public const string CharSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_+-=|[];',.{}:<>? ";
    public const char CharNotInSet = '`';
    static readonly IEnumerable<Int32> DataRange = Encode(CharSet + CharNotInSet);

    public static void Main() {
        unchecked {
            //var yy = new HashState((int)0xDF8BEDAAu, (int)0xB5A86DDEu);
            //var zz = (from c in CharSet.Length.Range()
            //          from p in yy.InverseAdvance(c)
            //          select p).Distinct().ToArray();
            //var zz2 = (from c in CharSet.Length.Range()
            //           from z in zz
            //           from p in z.InverseAdvance(c)
            //           select p).Distinct().ToArray();
            //var zz3 = (from c in CharSet.Length.Range()
            //           from z in zz2
            //           from p in z.InverseAdvance(c)
            //           select p).Distinct().ToArray();
            var k = "cdf";
            var x = Hash(Encode(k));
            Hash4.Break(x, k.Length);
            Console.WriteLine(FindIntermediate(Hash(Encode("<+")), new HashState((int)0xDF8BEDAAu, (int)0xB5A86DDEu)).ToString());
            Console.ReadLine();
        }
    }

    static Int32 Encode(char c) {
        return CharSet.Contains(c) ? CharSet.IndexOf(c) : CharSet.Length + 1;
    }
    public static IEnumerable<Int32> Encode(string text) {
        return text.Select(Encode);
    }
    static char Decode(Int32 value) {
        return value == CharSet.Length + 1 ? CharNotInSet : CharSet[value];
    }

    static Int64 PackTuple(Tuple<Int32, Int32> v) {
        unchecked {
            return (Int64)(((UInt64)(UInt32)v.Item1) << 32 | (UInt32)v.Item2);
        }
    }
    static Tuple<Int32, Int32> UnpackTuple(Int64 v) {
        unchecked {
            var v1 = (Int32)((v >> 32) & 0xFFFFFFFFL);
            var v2 = (Int32)(v & 0xFFFFFFFFL);
            return Tuple.Create(v1, v2);
        }
    }
    public static HashState Hash(IEnumerable<Int32> data) {
        unchecked {
            return data.Aggregate(new HashState(), (acc,e) => acc.Advance(e));
        }
    }

    static IEnumerable<Tuple<HashState, Char>> FindPres(HashState ab, HashSet<HashState> s) {
        var q = new Queue<HashState>();
        q.Enqueue(ab);
        s.Add(ab);

        while (q.Count > 0) {
            var v = q.Dequeue();
            foreach (var e in DataRange) {
                foreach (var h in v.InverseAdvance(e).Where(s.Add)) {
                    q.Enqueue(h);
                    yield return Tuple.Create(h, Decode(e));
                }
            }
        }
    }
    static IEnumerable<Tuple<HashState, Char>> FindPosts(HashState ab, HashSet<HashState> s) {
        var q = new Queue<HashState>();
        q.Enqueue(ab);
        s.Add(ab);

        while (q.Count > 0) {
            var v = q.Dequeue();
            foreach (var e in DataRange) {
                var h = v.Advance(e);
                if (!s.Add(h)) continue;
                q.Enqueue(h);
                yield return Tuple.Create(h, Decode(e));
            }
        }
    }
    static Tuple<HashState, Char> FindIntermediate(HashState start, HashState end) {
        var hs = new HashSet<HashState>();
        var he = new HashSet<HashState>();
        var i1 = FindPosts(start, hs).GetEnumerator();
        var i2 = FindPres(end, he).GetEnumerator();

        const int ForwardSpeed = 10;
        const int BackwardSpeed = 1;

        while (true) {
            for (var i = 0; i < ForwardSpeed; i++) {
                if (!i1.MoveNext()) 
                    return null;
                if (he.Contains(i1.Current.Item1))
                    return i1.Current;
            }
            for (var i = 0; i < BackwardSpeed; i++) {
                if (!i2.MoveNext()) 
                    return null;
                if (hs.Contains(i2.Current.Item1))
                    return i2.Current;
            }
        }
    }

    static Int32 TimesPlusRepeat(this Int32 seed, Int32 factor, Int32 offset, int iter) {
        var result = seed;
        for (var i = 0; i < iter; i++) {
            unchecked {
                result = offset + factor * result;
            }
        }
        return result;
    }
    static Int32 PowerSum(this IEnumerable<Int32> ns, int factor) {
        return ns.Aggregate(0, (current, e) => current*factor + e);
    }
    public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> items, int maxCount) {
        var queue = new Queue<T>(maxCount + 1);
        foreach (var item in items) {
            queue.Enqueue(item);
            if (queue.Count > maxCount) queue.Dequeue();
        }
        foreach (var item in queue)
            yield return item;
    }

    
    ///<summary>Returns a triple (g, x, y), such that a*x + b*y = g = gcd(a,b)</summary>
    static GCDExResult ExtendedGreatestCommonDivisor(Int32 a, Int32 b) {
        return MathEx.ExtendedGreatestCommonDivisor(a, b);
    }

    static HashState Hash2(IEnumerable<Int32> data) {
        if (data.Count() < 3) return Hash(data);

        var s17 = (-6).PowSum(17);
        var p17 = (-6).Pow(17);
        var inv3 = MathEx.MultiplicativeInverseS32(3);

        var a = 0;
        var b = 0;

        //constants don't merge until third character (due to iterative *6 pushing bits away after 32nd round)
        //luckily all the target hashes are known to start with <+
        if (true) {
            var e1 = data.ElementAt(0);
            var e2 = data.ElementAt(1);
            for (var i = 1; i <= 17; i++) {
                a *= -6;
                a += b;
                b /= 3;
                b += 0x81BE - e1;
                b += a;
                b += (0x74FA - e1) * (-6).PowSum(i);
            }
            for (var i = 1; i <= 17; i++) {
                a *= -6;
                a += b;
                b /= 3;
                b += 0x81BE - e2;
                b += a;
                b += (0x74FA - e2) * (-6).PowSum(i);
                b += (0x74FA - e1) * (-6).Pow(i) * s17;
            }
        }

        var dn = 0;
        foreach (var es in data.Roll(3)) {
            var e = es[2];
            var p1 = es[1];
            var p2 = es[0];
            var f = p1 + p2 * (-6).Pow(17);

            for (var i = 0; i < 17; i++) {
                var d = 0x9274 * inv3.PowSum(i) //step-dependent
                        + inv3.Pow(17).PowSum(dn) * -2106460428 * inv3.Pow(i) //step and round-count dependent
                        - e * (MathEx.TrianglePowSum(-6, inv3, i) + inv3.PowSum(i)) //step and char dependent
                        + f * 6 * s17 * MathEx.DiagonalPowSum(-6, inv3, i); //step and prev 2 chars dependent
                a *= -6;
                a += b + d;
                b -= (b + d) % 3;
                b *= inv3;
                b += a;
            }
            dn += 1;
            b += f * 489897038 - e * 1925145862;
        }

        var x = data.TakeLast(2).ToArray();
        var t = (x[1] + x[0] * p17) * s17;

        return new HashState(a - t + 4278, b + inv3.Pow(17).PowSum(dn) * -2106460428);
    }

    static IEnumerable<T[]> Roll<T>(this IEnumerable<T> sequence, int size) {
        var q = new Queue<T>();
        foreach (var item in sequence) {
            q.Enqueue(item);
            if (q.Count >= size) {
                yield return q.ToArray();
                q.Dequeue();
            }
        }
    }

    static bool HashMatches(string text, params HashState[] validHashes) {
        return validHashes.Contains(Hash(Encode(text)));
    }
    static bool Verify(string chat, string playerName) {
        unchecked {
            return chat.StartsWith("<+")
                && HashMatches(chat, new HashState((int)0xDF8BEDAAu, (int)0xB5A86DDEu))
                && HashMatches(playerName, new HashState((int)0xAD414D7Du, (int)0x8CC36A67u),
                                           new HashState(0x605D4A4F, 0x7EDDB1E5),
                                           new HashState(0x3D10F092, 0x60084719));
        }
    }
}
