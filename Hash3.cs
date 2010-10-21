using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Hash3 {
    public static HashState Hash(IEnumerable<Int32> data) {
        int inv3 = MathEx.MultiplicativeInverseS32(3).Value;
        unchecked {
            Int32 a1 = 0;
            Int32 a2 = 0;
            Int32 b1 = 0;
            Int32 b2 = 0;
            foreach (var e in data) {
                Int32 pa2 = a2;
                for (var i = 0; i < 17; i++) {
                    a1 *= -6;
                    a1 += b1 + b2;

                    a2 = pa2 * (-6).Pow(i) + (-6).PowSum(i) * (0x74FA - e);

                    b1 -= (b1 + b2) % 3;
                    b1 *= inv3;
                    b1 += a1 + a2;

                    b2 *= inv3;
                    b2 += 0x81BE;
                    b2 -= e;
                }
                a2 = pa2 * (-6).Pow(17) + (-6).PowSum(17) * (0x74FA - e);
            }
            return new HashState(a1 + a2, b1 + b2);
        }
    }
}
