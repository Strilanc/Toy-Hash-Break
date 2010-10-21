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
                for (var i = 0; i < 17; i++) {
                    a *= -6;
                    a += b;

                    b -= (b + b2 * (inv3).Pow(i) + (inv3).PowSum(i) * (0x81BE - e)) % 3;
                    b *= inv3;
                    b += a + (0x81BE - e) * MainHash.PowSumRevPowSum(inv3, -6, i) + b2 * MainHash.PowRevPowSum(inv3, -6, i + 1) + a2 * (-6).Pow(i + 1) + (-6).PowSum(i + 1) * (0x74FA - e);
                }
                a2 = a2 * (-6).Pow(17) + (-6).PowSum(17) * (0x74FA - e) + (0x81BE - e) * MainHash.PowSumRevPowSum(inv3, -6, 16) + b2 * MainHash.PowRevPowSum(inv3, -6, 17);
                b2 = b2 * (inv3).Pow(17) + (inv3).PowSum(17) * (0x81BE - e);
            }
            return new HashState(a + a2, b + b2);
        }
    }
}
