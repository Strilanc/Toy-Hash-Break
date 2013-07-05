using System;
using System.Linq;
using System.Collections.Generic;
using Strilanc.LinqToCollections;

public struct HashState {
    public readonly Int32 A;
    public readonly Int32 B;

    public HashState(Int32 a, Int32 b) {
        this.A = a;
        this.B = b;
    }

    public override int GetHashCode() {
        return A ^ B;
    }

    public HashState Advance(IEnumerable<Int32> e) {
        return e.Aggregate(this, (h, x) => h.Advance(x));
    }
    public HashState AdvanceRound(Int32 e) {
        unchecked {
            var a = this.A;
            var b = this.B;
            a = a * -6 + b + 0x74FA - e;
            b = b / 3 + a + 0x81BE - e;
            return new HashState(a, b);
        }
    }
    public HashState Advance(Int32 e) {
        unchecked {
            var a = this.A;
            var b = this.B;
            for (var i = 0; i < 17; i++) {
                a = a * -6 + b + 0x74FA - e;
                b = b / 3 + a + 0x81BE - e;
            }
            return new HashState(a, b);
        }
    }
    private static IEnumerable<int[]> ExploreDataVolatile(int levels, int n) {
        if (levels <= 0) {
            yield return new int[n];
            yield break;
        }
        foreach (var e in ExploreDataVolatile(levels - 1, n)) {
            foreach (var d in MainHash.DataRange) {
                e[levels - 1] = d;
                yield return e;
            }
        }
    }
    public IEnumerable<HashState> Explore(int levels) {
        if (levels <= 0) {
            yield return this;
            yield break;
        }
        foreach (var e in Explore(levels - 1)) {
            foreach (var d in MainHash.DataRange) {
                yield return e.Advance(d);
            }
        }
    }
    public IEnumerable<HashState> ExploreReverse(int levels) {
        if (levels <= 0) return ReadOnlyList.Singleton(this);
        return from r in ExploreReverse(levels - 1)
               from v in MainHash.DataRange
               from e in r.InverseAdvance(v)
               select e;
    }
    public IEnumerable<Tuple<HashState, int[]>> ExploreReverseTraceVolatile(int levels) {
        foreach (var e in ExploreDataVolatile(levels, levels)) {
            foreach (var p in InverseAdvance(e)) {
                yield return Tuple.Create(p, e);
            }
        }
    }
    public IEnumerable<Tuple<HashState, int[]>> ExploreTraceVolatile(int levels) {
        foreach (var e in ExploreDataVolatile(levels, levels)) {
            yield return Tuple.Create(Advance(e), e);
        }
    }

    private IEnumerable<HashState> InverseRoundFast(Int32 e) {
        unchecked {
            var q = this.B - this.A - 0x81BE + e;
            const Int32 d = 3;

            if (q < Int32.MinValue / d || q > Int32.MaxValue / d)
                yield break;

            Int32[] divs;
            var n = q * d;
            if (n == 0)
                divs = new[] { -2, -1, 0, 1, 2 };
            else if (q == Int32.MaxValue / d)
                divs = new[] { Int32.MaxValue - 1, Int32.MaxValue };
            else if (n > 0)
                divs = new[] { n, n + 1, n + 2 };
            else
                divs = new[] { n, n - 1, n - 2 };

            foreach (var nb in divs) {
                var product = this.A - 0x74FA + e - nb;
                if (product % 2 != 0) continue;

                var item = (product / 2) * 1431655765;
                yield return new HashState(item & ~(1 << 31), nb);
                yield return new HashState(item | (1 << 31), nb);
            }
        }
    }
    public IEnumerable<HashState> InverseRound(Int32 e) {
        var a = this.A;
        var b = this.B;
        return from nb in (b - a - 0x81BE + e).InvDivS32(3)
               from na in (a - 0x74FA + e - nb).InvMulS32(-6)
               select new HashState(na, nb);
    }

    public IEnumerable<HashState> InverseAdvance(Int32 e) {
        IEnumerable<HashState> result = new[] { this };
        for (var i = 0; i < 17; i++)
            result = (from h in result
                      from g in h.InverseRoundFast(e)
                      select g
                      ).Distinct().ToArray();
        return result;
    }
    public IEnumerable<HashState> InverseAdvance(IEnumerable<Int32> e) {
        return e.Aggregate(new[] {this}, (h, x) => h.SelectMany(v => v.InverseAdvance(x)).ToArray());
    }
    public override string ToString() {
        return "(A: " + this.A + ", B: " + this.B + ")";
    }
}
