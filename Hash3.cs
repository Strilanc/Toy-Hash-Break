using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class Hash3 {
    public static HashState Hash(IEnumerable<Int32> data) {
        int i3 = MathEx.MultiplicativeInverseS32(3).Value;
        int i3p17 = i3.Pow(17);
        int m6p17 = (-6).Pow(17);
        unchecked {
            Int32 a = 0;
            Int32 b = 0;
            Int32 b7 = 0;
            Int32 b8 = 0;
            Int32 b9 = 0;
            Int32 b10 = 0;
            Int32 b11 = 0;
            Int32 b12 = 0;
            Int32 dn = 0;
            var ee = new List<Int32>();
            foreach (var e in data) {
                for (var i = 0; i < 17; i++) {
                    Int32 c = 733353434 * i3p17.PowSum(dn) * i3.Pow(i)
                              + 0x74FA * MathEx.TrianglePowSum(-6, i3, i)
                              + -e * MathEx.TrianglePowSum(-6, i3, i)
                              + b9 * i3.Pow(i) + ee.RevDot(m6p17.Powers()) * -1995367200 * MathEx.DiagonalPowSum(i3, -6, i)
                              + b10 * i3.Pow(i) + (dn == 0 ? 0 : dn == 1 ? -879790284 : 710375220) * MathEx.DiagonalPowSum(i3, -6, i)
                              + b11 * i3.Pow(i) + 664196932 * MathEx.TrianglePowSum(m6p17, i3p17, Math.Max(dn - 1, 0)) * MathEx.DiagonalPowSum(i3, -6, i)
                              + b12 * i3.Pow(i) + ee.RevDot(MathEx.DiagonalPowSums(m6p17, i3p17)) * -21473370 * MathEx.DiagonalPowSum(i3, -6, i)
                              + b7
                              + b8
                              + ee.RevDot(i3p17.Powers()) * 454799799 * i3.Pow(i)
                              + -e * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6))
                              + 0x81BE * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6));
                    a *= -6;
                    a += b + c;

                    b -= (b
                          + c 
                          + i3p17.PowSum(dn) * i3.Pow(i) * -1268346242
                          + ee.RevDot(i3p17.Powers()) * -i3.PowSum(17) * i3.Pow(i) 
                          + i3.PowSum(i) * 0x81BE 
                          - i3.PowSum(i) * e) % 3;
                    
                    b7 *= i3;
                    b7 += i3p17.PowSum(dn) * -1268346242 * MathEx.DiagonalPowSum(i3, -6, i + 1);
                    
                    b8 *= i3;
                    b8 += ee.RevDot(i3p17.Powers()) * -i3.PowSum(17) * MathEx.DiagonalPowSum(i3, -6, i + 1);
                    
                    b *= i3;
                    b += a;
                }

                b9 = b9 * i3p17 + ee.RevDot(m6p17.Powers()) * -951417952;
                b10 = dn == 0 ? 0 : 910530428 * i3p17.Pow(dn - 1) + -1755474052 * i3p17.PowSum(dn - 1);
                b11 = b11 * i3p17 + MathEx.TrianglePowSum(m6p17, i3p17, Math.Max(dn - 1, 0)) * 46210348;
                b12 = b12 * i3p17 + ee.RevDot(MathEx.DiagonalPowSums(m6p17, i3p17)) * 243764226;

                ee.Add(e);
                dn += 1;
            }
            return new HashState(
                a
                + ee.RevDot(m6p17.Powers()) * -1814922448
                + (dn == 0 ? 0 : dn == 1 ? -2000851934 : 2029087778)
                + -1542355254 * MathEx.TrianglePowSum(m6p17, i3p17, dn - 1)
                + ee.RevDot(MathEx.DiagonalPowSums(m6p17, i3p17)) * 267738705 * -i3.PowSum(17), 
                b
                + -534992808 * i3p17.PowSum(dn)
                + ee.RevDot(i3p17.Powers()) * 583503382
                + b7 + b8 + b9 + b10 + b11 + b12);
        }
    }
}
