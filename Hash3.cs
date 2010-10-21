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
            Int32 a4 = 0;
            Int32 a5 = 0;
            Int32 b = 0;
            Int32 b4 = 0;
            Int32 b5 = 0;
            Int32 b6 = 0;
            Int32 b7 = 0;
            Int32 b8 = 0;
            Int32 b9 = 0;
            Int32 b10 = 0;
            Int32 b11 = 0;
            Int32 b12 = 0;
            Int32 b13 = 0;
            Int32 dn = 0;
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    Int32 c = 643801250 * inv3.Pow(17).PowSum(dn) * inv3.Pow(i) 
                              + 0x74FA * MainHash.PowSumRevPowSum(-6, inv3, i)
                              + b5 * inv3.Pow(i)
                              + -e * MainHash.PowSumRevPowSum(inv3, -6, i)
                              + b9 * inv3.Pow(i) + a2 * -6 * MainHash.PowRevPowSum(inv3, -6, i)
                              + b10 * inv3.Pow(i) + (dn == 0 ? 0 : dn == 1 ? -879790284 : 710375220) * MainHash.PowRevPowSum(inv3, -6, i);
                    a *= -6;
                    a += b + b7 + b8 + b11 + b12 + b13 + c + b4;

                    b -= (b + b7 + b8 + b11 + b12 + b13
                          + c 
                          + b4 
                          + inv3.Pow(17).PowSum(dn) * inv3.Pow(i) * -1268346242 
                          + b6 * inv3.Pow(i) 
                          + inv3.PowSum(i) * 0x81BE 
                          - inv3.PowSum(i) * e) % 3;
                    b *= inv3;
                    b4 *= inv3;
                    b7 *= inv3;
                    b8 *= inv3;
                    b11 *= inv3;
                    b12 *= inv3;
                    b13 *= inv3;
                    b4 += 0x81BE * MainHash.PowSumRevPowSum(inv3, -6, i);
                    b += a;
                    b7 += inv3.Pow(17).PowSum(dn) * -1268346242 * MainHash.PowRevPowSum(inv3, -6, i + 1);
                    b8 += b6 * MainHash.PowRevPowSum(inv3, -6, i + 1);
                    b11 += a4 * (-6).Pow(i + 1);
                    b12 += a5 * (-6).Pow(i + 1);
                    b13 += -e * MainHash.PowSumRevPowSum(inv3, -6, i);
                }
                b10 = b10 * inv3.Pow(17) + (dn == 0 ? 0 : dn == 1 ? -879790284 : 710375220) * MainHash.PowRevPowSum(inv3, -6, 17);
                b9 = b9 * inv3.Pow(17) + a2 * -6 * MainHash.PowRevPowSum(inv3, -6, 17);
                a2 = a2 * (-6).Pow(17) - e * 1811343553;
                a4 = a4 * (-6).Pow(17) + inv3.Pow(17).PowSum(dn) * -1542355254;
                a5 = a5 * (-6).Pow(17) + b6 * 270124635;
                dn += 1;
                b5 = b5 * inv3.Pow(17) - e * 2053849445;
                b6 = b6 * inv3.Pow(17) - e * inv3.PowSum(17);
            }
            return new HashState(
                a 
                + a2
                + (dn == 0 ? 0 : dn == 1 ? -2000851934 : 2029087778)
                + a4 + a5, 
                b 
                + inv3.Pow(17).PowSum(dn) * -624544992 
                + b4 + b5 + b6 + b7 + b8 + b9 + b10 + b11 + b12 + b13);
        }
    }
}
