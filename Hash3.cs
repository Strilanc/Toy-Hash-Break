﻿using System;
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
                              + 0x74FA * MathEx.TrianglePowSum(-6, inv3, i)
                              + ee.RevDot(inv3.Pow(17).Powers()) * -2053849445 * inv3.Pow(i)
                              + -e * MathEx.TrianglePowSum(inv3, -6, i)
                              + b9 * inv3.Pow(i) + ee.RevDot((-6).Pow(17).Powers()) * -1995367200 * MathEx.DiagonalPowSum(inv3, -6, i)
                              + b10 * inv3.Pow(i) + (dn == 0 ? 0 : dn == 1 ? -879790284 : 710375220) * MathEx.DiagonalPowSum(inv3, -6, i)
                              + b11 * inv3.Pow(i) + 664196932 * MathEx.TrianglePowSum((-6).Pow(17), inv3.Pow(17), Math.Max(dn - 1, 0)) * MathEx.DiagonalPowSum(inv3, -6, i)
                              + b12 * inv3.Pow(i) + ee.RevDot(MathEx.DiagonalPowSums((-6).Pow(17), inv3.Pow(17))) * -21473370 * MathEx.DiagonalPowSum(inv3, -6, i)
                              + b7
                              + b8
                              + b13;
                    a *= -6;
                    a += b + c + b4;

                    b -= (b
                          + c 
                          + b4 
                          + inv3.Pow(17).PowSum(dn) * inv3.Pow(i) * -1268346242
                          + ee.RevDot(inv3.Pow(17).Powers()) * -inv3.PowSum(17) * inv3.Pow(i) 
                          + inv3.PowSum(i) * 0x81BE 
                          - inv3.PowSum(i) * e) % 3;
                    
                    b4 *= inv3;
                    b4 += 0x81BE * MathEx.TrianglePowSum(inv3, -6, i);
                    
                    b7 *= inv3;
                    b7 += inv3.Pow(17).PowSum(dn) * -1268346242 * MathEx.DiagonalPowSum(inv3, -6, i + 1);
                    
                    b8 *= inv3;
                    b8 += ee.RevDot(inv3.Pow(17).Powers()) * -inv3.PowSum(17) * MathEx.DiagonalPowSum(inv3, -6, i + 1);
                    
                    b13 *= inv3;
                    b13 += -e * MathEx.TrianglePowSum(inv3, -6, i);

                    b *= inv3;
                    b += a;
                }

                b9 = b9 * inv3.Pow(17) + ee.RevDot((-6).Pow(17).Powers()) * -951417952;
                b10 = dn == 0 ? 0 : 910530428 * inv3.Pow(17).Pow(dn - 1) + -1755474052 * inv3.Pow(17).PowSum(dn - 1);
                b11 = b11 * inv3.Pow(17) + MathEx.TrianglePowSum((-6).Pow(17), inv3.Pow(17), Math.Max(dn - 1, 0)) * 46210348;
                b12 = b12 * inv3.Pow(17) + ee.RevDot(MathEx.DiagonalPowSums((-6).Pow(17), inv3.Pow(17))) * 243764226;

                ee.Add(e);
                dn += 1;
            }
            return new HashState(
                a
                + ee.RevDot((-6).Pow(17).Powers()) * -1814922448
                + (dn == 0 ? 0 : dn == 1 ? -2000851934 : 2029087778)
                + -1542355254 * MathEx.TrianglePowSum((-6).Pow(17), inv3.Pow(17), dn - 1)
                + ee.RevDot(MathEx.DiagonalPowSums((-6).Pow(17), inv3.Pow(17))) * 267738705 * -inv3.PowSum(17), 
                b 
                + inv3.Pow(17).PowSum(dn) * -624544992 
                + b4
                + ee.RevDot(inv3.Pow(17).Powers()) * -2053849445
                + ee.RevDot(inv3.Pow(17).Powers()) * -inv3.PowSum(17)
                + b7 + b8 + b9 + b10 + b11 + b12 + b13);
        }
    }
}
