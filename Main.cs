using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Diagnostics.Contracts;

public static class MainHash {
    const string charSet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789`~!@#$%^&*()_+-=|[];',.{}:<>? ";
    const char charNotInSet = '`';
    static readonly IEnumerable<Int32> DataRange = Encode(charSet + charNotInSet);

    public static void Main() {
        unchecked {
            Console.WriteLine(findIntermediate(Hash(Encode("<+")), new HashState((int)0xDF8BEDAAu, (int)0xB5A86DDEu)).ToString());
            Console.ReadLine();
        }
    }

    static Int32 Encode(char c) {
        return charSet.Contains(c) ? charSet.IndexOf(c) : charSet.Length + 1;
    }
    public static IEnumerable<Int32> Encode(string text) {
        return text.Select(Encode);
    }
    static char Decode(Int32 value) {
        return value == charSet.Length + 1 ? charNotInSet : charSet[value];
    }

    static Int64 PackTuple(Tuple<Int32, Int32> v) {
        unchecked {
            return (Int64)(((UInt64)(UInt32)v.Item1) << 32 | (UInt64)(UInt32)v.Item2);
        }
    }
    static Tuple<Int32, Int32> UnpackTuple(Int64 v) {
        unchecked {
            int v1 = (Int32)((v >> 32) & 0xFFFFFFFFL);
            int v2 = (Int32)(v & 0xFFFFFFFFL);
            return Tuple.Create(v1, v2);
        }
    }
    public static HashState Hash(IEnumerable<Int32> data) {
        unchecked {
            return data.Aggregate(new HashState(), (acc,e) => acc.Advance(e));
        }
    }

    static IEnumerable<Tuple<HashState, Char>> findPres(HashState ab, HashSet<HashState> s) {
        var q = new Queue<HashState>();
        q.Enqueue(ab);
        s.Add(ab);

        while (q.Count > 0) {
            var v = q.Dequeue();
            foreach (int e in DataRange) {
                foreach (var h in v.InverseAdvance(e)) {
                    if (!s.Add(h)) continue;
                    q.Enqueue(h);
                    yield return Tuple.Create(h, Decode(e));
                }
            }
        }
    }
    static IEnumerable<Tuple<HashState, Char>> findPosts(HashState ab, HashSet<HashState> s) {
        var q = new Queue<HashState>();
        q.Enqueue(ab);
        s.Add(ab);

        while (q.Count > 0) {
            var v = q.Dequeue();
            foreach (int e in DataRange) {
                var h = v.Advance(e);
                if (!s.Add(h)) continue;
                q.Enqueue(h);
                yield return Tuple.Create(h, Decode(e));
            }
        }
    }
    static Tuple<HashState, Char> findIntermediate(HashState start, HashState end) {
        var hs = new HashSet<HashState>();
        var he = new HashSet<HashState>();
        var i1 = findPosts(start, hs).GetEnumerator();
        var i2 = findPres(end, he).GetEnumerator();

        const int ForwardSpeed = 10;
        const int BackwardSpeed = 1;

        while (true) {
            for (int i = 0; i < ForwardSpeed; i++) {
                if (!i1.MoveNext()) 
                    return null;
                if (he.Contains(i1.Current.Item1))
                    return i1.Current;
            }
            for (int i = 0; i < BackwardSpeed; i++) {
                if (!i2.MoveNext()) 
                    return null;
                if (hs.Contains(i2.Current.Item1))
                    return i2.Current;
            }
        }
    }

    static Int32 TimesPlusRepeat(this Int32 seed, Int32 factor, Int32 offset, int iter) {
        Int32 result = seed;
        for (int i = 0; i < iter; i++) {
            unchecked {
                result = offset + factor * result;
            }
        }
        return result;
    }
    static Int32 PowerSum(this IEnumerable<Int32> ns, int factor) {
        Int32 result = 0;
        foreach (var e in ns)
            result = result * factor + e;
        return result;

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

        int s17 = (-6).PowSum(17);
        int p17 = (-6).Pow(17);
        int inv3 = MathEx.MultiplicativeInverseS32(3).Value;

        Int32 a = 0;
        Int32 b = 0;

        //constants don't merge until third character (due to iterative *6 pushing bits away after 32nd round)
        //luckily all the target hashes are known to start with <+
        if (true) {
            Int32 e1 = data.ElementAt(0);
            Int32 e2 = data.ElementAt(1);
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

        int dn = 0;
        foreach (var es in data.Roll(3)) {
            Int32 e = es[2];
            Int32 p1 = es[1];
            Int32 p2 = es[0];
            Int32 f = p1 + p2 * (-6).Pow(17);

            for (int i = 0; i < 17; i++) {
                var d = 0x9274 * inv3.PowSum(i) //step-dependent
                        + inv3.Pow(17).PowSum(dn) * -2106460428 * inv3.Pow(i) //step and round-count dependent
                        - e * (PowSumRevPowSum(-6, inv3, i) + inv3.PowSum(i)) //step and char dependent
                        + f * 6 * s17 * PowRevPowSum(-6, inv3, i); //step and prev 2 chars dependent
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
        Int32 t = (x[1] + x[0] * p17) * s17;

        return new HashState(a - t + 4278, b + inv3.Pow(17).PowSum(dn) * -2106460428);
    }

    public static Int32 PowRevPowSum(Int32 v1, Int32 v2, Int32 n) {
        Int32 result = 0;
        for (int i = 0; i < n; i++)
            result += v1.Pow(i) * v2.Pow(n - i - 1);
        return result;
    }
    public static Int32 PowSumRevPowSum(Int32 s1, Int32 v2, Int32 n) {
        Int32 result = 0;
        for (int i = 0; i < n; i++)
            result += s1.PowSum(i + 1) * v2.Pow(n - i - 1);
        return result;
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
