using System;
using System.Collections.Generic;
using System.Linq;

static class Hash3 {
    public static HashState Hash(IEnumerable<Int32> data) {
        var i3 = MathEx.MultiplicativeInverseS32(3);
        var i3p17 = i3.Pow(17);
        var m6p17 = (-6).Pow(17);
        unchecked {
            var a = 0;
            var b = 0;
            var dn = 0;
            var ee = new List<Int32>();

            foreach (var e in data.Take(1)) {
                for (var i = 0; i < 17; i++) {
                    var c = (0x74FA - e) * MathEx.TrianglePowSum(-6, i3, i)
                            + (0x81BE - e) * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6));
                    a *= -6;
                    a += b + c;

                    b -= (b + c + i3.PowSum(i) * (0x81BE - e)) % 3;
                    
                    b *= i3;
                    b += a;
                }

                ee.Add(e);
                dn += 1;
            }

            foreach (var e in data.Skip(1).Take(1)) {
                for (var i = 0; i < 17; i++) {
                    var c = 733353434 * i3.Pow(i)
                              + 0x74FA * MathEx.TrianglePowSum(-6, i3, i)
                              + -e * MathEx.TrianglePowSum(-6, i3, i)
                              + -e * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6))
                              + ee.RevDot(m6p17.Powers()) * -1995367200 * MathEx.DiagonalPowSum(i3, -6, i)
                              + -879790284 * MathEx.DiagonalPowSum(i3, -6, i)
                              + 664196932 * MathEx.TrianglePowSum(m6p17, i3p17, Math.Max(1 - 1, 0)) * MathEx.DiagonalPowSum(i3, -6, i)
                              + ee.Single() * -21473370 * MathEx.DiagonalPowSum(i3, -6, i)
                              + i3p17.PowSum(1) * -1268346242 * i3.Powers().Take(i).RevDot(MathEx.DiagonalPowSums(i3, -6))
                              + ee.Single() * -i3.PowSum(17) * Enumerable.Range(1, i).Select(j => MathEx.DiagonalPowSum(i3, -6, j)).RevDot(i3.Powers())
                              + ee.Single() * 454799799 * i3.Pow(i)
                              + 0x81BE * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6));
                    a *= -6;
                    a += b + c;

                    b -= (b
                          + c
                          + i3.Pow(i) * -1268346242
                          + ee.RevDot(i3p17.Powers()) * -i3.PowSum(17) * i3.Pow(i)
                          + i3.PowSum(i) * 0x81BE
                          - i3.PowSum(i) * e) % 3;

                    b *= i3;
                    b += a;
                }

                ee.Add(e);
                dn += 1;
            }

            foreach (var e in data.Skip(2)) {
                for (var i = 0; i < 17; i++) {
                    var c = 733353434 * i3p17.PowSum(dn) * i3.Pow(i)
                              + 0x74FA * MathEx.TrianglePowSum(-6, i3, i)
                              + -e * MathEx.TrianglePowSum(-6, i3, i)
                              + -e * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6))
                              + MathEx.FactorTrianglePowerSum(ee.Take(dn - 1), i3p17, m6p17) * -951417952 * i3.Pow(i)
                              + ee.TakeLast(2).RevDot(m6p17.Powers()) * -1995367200 * MathEx.DiagonalPowSum(i3, -6, i)
                              + 910530428 * i3p17.Pow(dn - 2) * i3.Pow(i)
                              + -1755474052 * i3p17.PowSum(dn - 2) * i3.Pow(i)
                              + 710375220 * MathEx.DiagonalPowSum(i3, -6, i)
                              + Enumerable.Range(1, dn - 1).Select(j => MathEx.TrianglePowSum(m6p17, i3p17, j - 1)).RevDot(i3p17.Powers()) * 46210348 * i3.Pow(i)
                              + 664196932 * MathEx.TrianglePowSum(m6p17, i3p17, dn - 1) * MathEx.DiagonalPowSum(i3, -6, i)
                              + Enumerable.Range(0, dn).Select(j => ee.Take(j).RevDot(MathEx.DiagonalPowSums(m6p17, i3p17))).RevDot(i3p17.Powers()) * 243764226 * i3.Pow(i)
                              + ee.RevDot(MathEx.DiagonalPowSums(m6p17, i3p17)) * -21473370 * MathEx.DiagonalPowSum(i3, -6, i)
                              + i3.Pow(i) * -1859854618 * i3p17.Powers().Dot(Enumerable.Range(1, dn - 1))
                              + i3p17.PowSum(dn) * -1268346242 * i3.Powers().Take(i).RevDot(MathEx.DiagonalPowSums(i3, -6))
                              + -907067373 * Enumerable.Range(0, dn - 1).Select(j => ee[j] * i3p17.Pow(dn - j - 2) * (dn - 1 - j)).SumWrap() * i3.Pow(i)
                              + ee.RevDot(i3p17.Powers()) * -i3.PowSum(17) * Enumerable.Range(1, i).Select(j => MathEx.DiagonalPowSum(i3, -6, j)).RevDot(i3.Powers())
                              + ee.RevDot(i3p17.Powers()) * 454799799 * i3.Pow(i)
                              + 0x81BE * i3.Powers().Take(i - 1).RevDot(MathEx.TrianglePowSums(i3, -6));
                    a *= -6;
                    a += b + c;

                    b -= (b
                          + c
                          + i3p17.PowSum(dn) * i3.Pow(i) * -1268346242
                          + ee.RevDot(i3p17.Powers()) * -i3.PowSum(17) * i3.Pow(i)
                          + i3.PowSum(i) * 0x81BE
                          - i3.PowSum(i) * e) % 3;

                    b *= i3;
                    b += a;
                }

                ee.Add(e);
                dn += 1;
            }

            return new HashState(
                a
                + -1814922448 * ee.RevDot(m6p17.Powers())
                + (dn == 0 ? 0 : dn == 1 ? -2000851934 : 2029087778)
                + -1542355254 * MathEx.TrianglePowSum(m6p17, i3p17, Math.Max(dn - 1, 0))
                + 3578895 * ee.RevDot(MathEx.DiagonalPowSums(m6p17, i3p17)), 
                b
                + -534992808 * i3p17.PowSum(dn)
                + 583503382 * ee.RevDot(i3p17.Powers())
                + -1859854618 * i3p17.Powers().Dot(Enumerable.Range(1, Math.Max(dn - 1, 0)))
                + -907067373 * Enumerable.Range(0, Math.Max(dn - 1, 0)).Select(j => ee[j] * i3p17.Pow(dn - j - 2) * (dn - 1 - j)).SumWrap()
                + -951417952 * MathEx.FactorTrianglePowerSum(ee.Take(dn - 1), i3p17, m6p17)
                + (dn <= 1 ? 0 : 910530428 * i3p17.Pow(dn - 2) + -1755474052 * i3p17.PowSum(Math.Max(dn - 2, 0)))
                + Enumerable.Range(0, dn).Select(i => MathEx.TrianglePowSum(m6p17, i3p17, Math.Max(i - 1, 0))).RevDot(i3p17.Powers()) * 46210348
                + Enumerable.Range(0, dn).Select(i => ee.Take(i).RevDot(MathEx.DiagonalPowSums(m6p17, i3p17))).RevDot(i3p17.Powers()) * 243764226);
        }
    }
}
