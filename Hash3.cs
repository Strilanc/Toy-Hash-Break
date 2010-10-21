using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Hash3 {
    public static HashState Hash(IEnumerable<Int32> data) {
        int inv3 = MathEx.MultiplicativeInverseS32(3).Value;
        unchecked {
            Int32 a = 0;
            Int32 a2 = 0;
            Int32 b = 0;
            Int32 b2 = 0;
            foreach (var e in data) {
                Int32 a3 = 0;
                for (var i = 0; i < 17; i++) {
                    a3 *= -6;
                    a3 += b2 * (inv3).Pow(i) + (inv3).PowSum(i) * (0x81BE - e);

                    a *= -6;
                    a += b;

                    b -= (b + b2 * (inv3).Pow(i) + (inv3).PowSum(i) * (0x81BE - e)) % 3;
                    b *= inv3;
                    b += a + a3 + a2 * (-6).Pow(i + 1) + (-6).PowSum(i + 1) * (0x74FA - e);
                }
                b2 = b2 * (inv3).Pow(17) + (inv3).PowSum(17) * (0x81BE - e);
                a2 = a2 * (-6).Pow(17) + (-6).PowSum(17) * (0x74FA - e) + a3;
            }
            return new HashState(a + a2, b + b2);
        }
    }
}
