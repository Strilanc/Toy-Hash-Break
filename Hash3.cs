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
            Int32 b4 = 0;
            Int32 b7 = 0;
            Int32 b8 = 0;
            Int32 b9 = 0;
            Int32 b10 = 0;
            Int32 b11 = 0;
            Int32 b12 = 0;
            Int32 b13 = 0;
            Int32 dn = 0;
            var ee = new List<Int32>();
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    Int32 c = 643801250 * inv3.Pow(17).PowSum(dn) * inv3.Pow(i) 
                              + 0x74FA * MainHash.PowSumRevPowSum(-6, inv3, i)
                              + ee.AsEnumerable().Reverse().Dot(inv3.Pow(17).Powers().Take(dn + 1)) * -2053849445 * inv3.Pow(i)
                              + -e * MainHash.PowSumRevPowSum(inv3, -6, i)
                              + b9 * inv3.Pow(i) + ee.AsEnumerable().Reverse().Dot((-6).Pow(17).Powers()) * -1814922448 * -6 * MainHash.PowRevPowSum(inv3, -6, i)
                              + b10 * inv3.Pow(i) + (dn == 0 ? 0 : dn == 1 ? -879790284 : 710375220) * MainHash.PowRevPowSum(inv3, -6, i)
                              + b11 * inv3.Pow(i) + (-1542355254 * MainHash.PowSumRevPowSum((-6).Pow(17), inv3.Pow(17), dn - 1)) * -6 * MainHash.PowRevPowSum(inv3, -6, i)
                              + b12 * inv3.Pow(i) + ee.Select((ei, j) => ei * MainHash.PowRevPowSum((-6).Pow(17), inv3.Pow(17), dn - j)).SumWrap() * 267738705 * -inv3.PowSum(17) * -6 * MainHash.PowRevPowSum(inv3, -6, i);
                    a *= -6;
                    a += b + b7 + b8 + b13 + c + b4;

                    b -= (b + b7 + b8 + b13
                          + c 
                          + b4 
                          + inv3.Pow(17).PowSum(dn) * inv3.Pow(i) * -1268346242
                          + ee.AsEnumerable().Reverse().Dot(inv3.Pow(17).Powers().Take(dn + 1)) * -inv3.PowSum(17) * inv3.Pow(i) 
                          + inv3.PowSum(i) * 0x81BE 
                          - inv3.PowSum(i) * e) % 3;
                    b *= inv3;
                    b4 *= inv3;
                    b7 *= inv3;
                    b8 *= inv3;
                    b13 *= inv3;
                    b4 += 0x81BE * MainHash.PowSumRevPowSum(inv3, -6, i);
                    b += a;
                    b7 += inv3.Pow(17).PowSum(dn) * -1268346242 * MainHash.PowRevPowSum(inv3, -6, i + 1);
                    b8 += ee.AsEnumerable().Reverse().Dot(inv3.Pow(17).Powers().Take(dn + 1)) * -inv3.PowSum(17) * MainHash.PowRevPowSum(inv3, -6, i + 1);
                    b13 += -e * MainHash.PowSumRevPowSum(inv3, -6, i);
                }

                b9 = b9 * inv3.Pow(17) + ee.AsEnumerable().Reverse().Dot((-6).Pow(17).Powers()) * -1814922448 * -6 * MainHash.PowRevPowSum(inv3, -6, 17);
                b10 = dn == 0 ? 0 : 910530428 * inv3.Pow(17).Pow(dn - 1) + -1755474052 * inv3.Pow(17).PowSum(dn - 1);
                b11 = b11 * inv3.Pow(17) + -1542355254 * MainHash.PowSumRevPowSum((-6).Pow(17), inv3.Pow(17), dn - 1) * -1620747810;
                b12 = b12 * inv3.Pow(17) + ee.Select((ei, i) => ei * MainHash.PowRevPowSum((-6).Pow(17), inv3.Pow(17), dn - i)).SumWrap() * 267738705 * -inv3.PowSum(17) * -1620747810;

                ee.Add(e);

                dn += 1;
            }
            return new HashState(
                a
                + ee.AsEnumerable().Reverse().Dot((-6).Pow(17).Powers()) * -1814922448
                + (dn == 0 ? 0 : dn == 1 ? -2000851934 : 2029087778)
                + -1542355254 * MainHash.PowSumRevPowSum((-6).Pow(17), inv3.Pow(17), dn - 1)
                + ee.Select((ei, i) => ei * MainHash.PowRevPowSum((-6).Pow(17), inv3.Pow(17), dn - i)).SumWrap() * 267738705 * -inv3.PowSum(17), 
                b 
                + inv3.Pow(17).PowSum(dn) * -624544992 
                + b4
                + ee.AsEnumerable().Reverse().Dot(inv3.Pow(17).Powers().Take(dn + 1)) * -2053849445
                + ee.AsEnumerable().Reverse().Dot(inv3.Pow(17).Powers().Take(dn + 1)) * -inv3.PowSum(17)
                + b7 + b8 + b9 + b10 + b11 + b12 + b13);
        }
    }
}
