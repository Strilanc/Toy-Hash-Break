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
            Int32 a3 = 0;
            Int32 b = 0;
            Int32 b4 = 0;
            Int32 b5 = 0;
            Int32 b6 = 0;
            Int32 dn = 0;
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    Int32 c = inv3.Pow(17).PowSum(dn) * 643801250 * inv3.Pow(i) 
                              + 0x74FA * MainHash.PowSumRevPowSum(-6, inv3, i)
                              + b5 * inv3.Pow(i)
                              + -e * MainHash.PowSumRevPowSum(inv3, -6, i);
                    a *= -6;
                    a += b + c + b4;

                    b -= (b + c + b4 + (inv3.Pow(17).PowSum(dn) * -1268346242 + b6) * inv3.Pow(i) + inv3.PowSum(i) * (0x81BE - e)) % 3;
                    b *= inv3;
                    b4 *= inv3;
                    b4 += 0x81BE * MainHash.PowSumRevPowSum(inv3, -6, i);
                    b += a
                         + (inv3.Pow(17).PowSum(dn) * -1268346242 + b6) * MainHash.PowRevPowSum(inv3, -6, i + 1) 
                         + a2 * (-6).Pow(i + 1) 
                         + a3 * (-6).Pow(i + 1) 
                         - e * MainHash.PowSumRevPowSum(inv3, -6, i);
                }
                b5 = b5 * inv3.Pow(17) - e * 2053849445;
                a2 = a2 * (-6).Pow(17) - e * 1811343553 + (inv3.Pow(17).PowSum(dn) * -1268346242 + b6) * 270124635;
                dn += 1;
                a3 = dn == 1 ? -2000851934 : 2029087778;
                b6 = b6 * inv3.Pow(17) - inv3.PowSum(17) * e;
            }
            return new HashState(a + a2 + a3, b + inv3.Pow(17).PowSum(dn) * -1268346242 + b6 + inv3.Pow(17).PowSum(dn) * 643801250 + b4 + b5);
        }
    }
}
