using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Hash3 {
    public static HashState Hash(IEnumerable<Int32> data) {
        int inv3 = MathEx.MultiplicativeInverseS32(3).Value;
        unchecked {
            Int32 a = 0;
            Int32 b = 0;
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    a *= -6;
                    a += b;
                    a += 0x74FA;
                    a -= e;
                    b -= b % 3;
                    b *= inv3;
                    b += a;
                    b += 0x81BE;
                    b -= e;
                }
            }
            return new HashState(a, b);
        }
    }
}
