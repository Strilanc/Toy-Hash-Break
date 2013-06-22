using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Strilanc.LinqToCollections;

public static class Hash4 {
    public static IEnumerable<T[]> PartitionVolatile<T>(this IEnumerable<T> sequence, int partitionSize) {
        var r = new T[partitionSize];
        var i = 0;
        foreach (var e in sequence) {
            r[i] = e;
            i += 1;
            if (i == partitionSize) {
                yield return r;
                i = 0;
            }
        }
        if (i > 0) yield return r.Take(i).ToArray();
    }
    public static int[] Break(HashState end, int assumedLength, bool cache, HashState start = default(HashState)) {
        return Break(end, assumedLength, new[] { start }, cache).Item2;
    }
    public static Tuple<HashState, int[]> Break(HashState end, int assumedLength, IEnumerable<HashState> starts, bool cache) {
        var numExpandBackward = Math.Max(0, Math.Min(Math.Min(4, assumedLength - 1), (assumedLength * 2) / 3));
        var filter = cache
            ? HashStateBloomFilter.GenReverseCache(end, numExpandBackward, 0.0001)
            : HashStateBloomFilter.GenReverse(end, numExpandBackward, 0.0001);

        var possiblePartialSolutions = from start in starts
                                       from e in start.ExploreTraceVolatile(assumedLength - numExpandBackward)
                                       where filter.MayContain(e.Item1)
                                       select new { start, data = e.Item2.ToArray(), end = e.Item1 };
        
        var partitions = possiblePartialSolutions.PartitionVolatile(10000);

        var solutions = from partition in partitions
                        let partialSolutionMap = partition.ToDictionary(e => e.end, e => e)
                        from midPoint in end.ExploreReverseTraceVolatile(numExpandBackward)
                        where partialSolutionMap.ContainsKey(midPoint.Item1)
                        let partialSolution = partialSolutionMap[midPoint.Item1]
                        let start = partialSolution.start
                        let data = partialSolution.data.Concat(midPoint.Item2.Reverse()).ToArray()
                        select Tuple.Create(start, data);

        return solutions.FirstOrDefault();
    }
}

internal struct Bits {
    private readonly ulong[] _bits;
    private readonly ulong capacity;
    public Bits(ulong capacity) {
        this.capacity = capacity;
        if ((capacity & 63) != 0) throw new ArgumentException();
        _bits = new ulong[capacity >> 6];
    }
    public Bits(ulong[] bits) {
        this._bits = bits;
        this.capacity = (ulong)bits.LongLength*64;
    }
    public bool this[ulong index] {
        get {
            index &= capacity - 1;
            ulong bit = 1ul << (int)(index & 63);
            return (_bits[index >> 6] & bit) != 0;
        }
        set {
            index &= capacity - 1;
            ulong bit = 1ul << (int)(index & 63);
            if (value) {
                _bits[index >> 6] |= bit;
            } else {
                _bits[index >> 6] &= ~bit;
            }
        }
    }
    public void Save(string path) {
        using (var f = File.OpenWrite(path)) {
            using (var b = new BinaryWriter(f)) {
                Save(b);
            }
        }
    }
    public void Save(BinaryWriter b) {
        b.Write((ulong)_bits.LongLength);
        foreach (var x in _bits) {
            b.Write(x);
        }
    }
    public static Bits Load(string path) {
        using (var f = File.OpenRead(path)) {
            using (var b = new BinaryReader(f)) {
                return Load(b);
            }
        }
    }
    public static Bits Load(BinaryReader b) {
        var n = b.ReadUInt64();
        var bits = new ulong[n];
        for (var i = 0ul; i < n; i++) {
            bits[i] = b.ReadUInt64();
        }
        return new Bits(bits);
    }
}

internal class HashStateBloomFilter {
    private Bits _bits;
    private int _added;
    private int _collisions;
    private static HashState? _genState;
    private static Dictionary<int, HashStateBloomFilter> _lastGenned;
    private HashStateBloomFilter(Bits bits) {
        this._bits = bits;
    }
    public HashStateBloomFilter(int power, double pFalsePositive) {
        var n = (long)Math.Pow(MainHash.DataRange.Length, power);
        var m = -(long)((n*Math.Log(pFalsePositive))/(Math.Log(2)*Math.Log(2)));
        var k = m * Math.Log(2) / n;
        m = 1L << (int)Math.Ceiling(Math.Log(m, 2));
        if (m < 128) m = 128;
        m = 1L << 33;
        this._bits = new Bits((ulong)m);
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
    public static HashStateBloomFilter GenReverseCache(HashState end, int pow, double pFalsePositive) {
        var path = "rev_" + pow + ",A_" + end.A + ",B_" + end.B + ",p_" + pFalsePositive;
        if (File.Exists(path)) {
            return new HashStateBloomFilter(Bits.Load(path));
        }
        var x = GenReverse(end, pow, pFalsePositive);
        x._bits.Save(path);
        return x;
    }
    public static HashStateBloomFilter GenReverse(HashState end, int pow, double pFalsePositive) {
        var x = new HashStateBloomFilter(pow, pFalsePositive);
        foreach (var e in end.ExploreReverse(pow)) {
            if (x.MayContain(e)) x._collisions += 1;
            x.Add(e);
        }
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
    private static ulong Comb(int v1, int v2) {
        return (uint)v1 | ((ulong)(uint)v2 << 32);
    }
    private static ulong Rot(ulong v, int r) {
        return (v << r) | (v >> r);
    }
    public bool MayContain(HashState h) {
        var v = Comb(h.A, h.B);
        var v2 = Comb(h.A ^ h.B, h.A + h.B);
        var v3 = Comb(h.A - h.B, (h.A * 0x5bd1e995) ^ h.B);
        var v4 = Comb(h.A ^ (h.B - h.A), (h.B * 0x5bd1e997) ^ h.A);
        return _bits[v]
               && _bits[Rot(v, 32)]
               && _bits[v2]
               && _bits[Rot(v2, 32)]
               && _bits[v3]
               && _bits[Rot(v3, 32)]
               && _bits[v4]
               && _bits[Rot(v4, 32)];
    }
    public void Add(HashState h) {
        _added += 1;
        var v = Comb(h.A, h.B);
        var v2 = Comb(h.A ^ h.B, h.A + h.B);
        var v3 = Comb(h.A - h.B, (h.A*0x5bd1e995) ^ h.B);
        var v4 = Comb(h.A ^ (h.B - h.A), (h.B * 0x5bd1e997) ^ h.A);
        _bits[v] = true;
        _bits[Rot(v, 32)] = true;
        _bits[v2] = true;
        _bits[Rot(v2, 32)] = true;
        _bits[v3] = true;
        _bits[Rot(v3, 32)] = true;
        _bits[v4] = true;
        _bits[Rot(v4, 32)] = true;
    }
}
