using System;
using System.Linq;
using System.Collections.Generic;

public struct HashState : IEquatable<HashState> {
    private readonly Int64 _state;

    public HashState(Int32 a, Int32 b) {
        unchecked {
            _state = ((Int64)a << 32) | (b & 0xFFFFFFFFL);
        }
    }
    public Int32 A { get { return (Int32)((_state >> 32) & 0xFFFFFFFFL); } }
    public Int32 B { get { return (Int32)(_state & 0xFFFFFFFFL); } }

    public bool Equals(HashState other) {
        return this._state == other._state;
    }
    public override bool Equals(Object obj) {
        return obj is HashState && this.Equals((HashState)obj);
    }
    public override int GetHashCode() {
        return this._state.GetHashCode();
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
        return from nb in MathEx.InvDivS32(b - a - 0x81BE + e, 3)
               from na in MathEx.InvMulS32(a - 0x74FA + e - nb, -6)
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
    public override string ToString() {
        return "(A: " + this.A + ", B: " + this.B + ")";
    }
}
