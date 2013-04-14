using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using System.Linq;

namespace TestProject1 {
    [TestClass()]
    public class MathExTest {
        [TestMethod()]
        public void ExtendedGreatestCommonDivisorTest() {
            Assert.AreEqual(MathEx.ExtendedGreatestCommonDivisor(1, 0), new GCDExResult(1, 1, 0));
            Assert.AreEqual(MathEx.ExtendedGreatestCommonDivisor(0, 1), new GCDExResult(1, 0, 1));
            Assert.AreEqual(MathEx.ExtendedGreatestCommonDivisor(1, 1), new GCDExResult(1, 1, 0));
        }

        [TestMethod()]
        public void InvDivS32Test() {
            Assert.IsTrue(MathEx.InvDivS32(0, 1).SequenceEqual(new Int32[] { 0 }));
            Assert.IsTrue(MathEx.InvDivS32(0, 3).SequenceEqual(new Int32[] { -2, -1, 0, 1, 2 }));
            Assert.IsTrue(MathEx.InvDivS32(1, 1).SequenceEqual(new Int32[] { 1 }));
            Assert.IsTrue(MathEx.InvDivS32(1, 2).SequenceEqual(new Int32[] { 2, 3 }));
            Assert.IsTrue(MathEx.InvDivS32(5, 4).SequenceEqual(new Int32[] { 20, 21, 22, 23 }));
            Assert.IsTrue(MathEx.InvDivS32(-6, 5).SequenceEqual(new Int32[] { -30, -31, -32, -33, -34 }));
        }
    }
}
