using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject1 {
    [TestClass]
    public class MathExTest {
        [TestMethod]
        public void ExtendedGreatestCommonDivisorTest() {
            Assert.AreEqual(MathEx.ExtendedGreatestCommonDivisor(1, 0), new GCDExResult(1, 1, 0));
            Assert.AreEqual(MathEx.ExtendedGreatestCommonDivisor(0, 1), new GCDExResult(1, 0, 1));
            Assert.AreEqual(MathEx.ExtendedGreatestCommonDivisor(1, 1), new GCDExResult(1, 1, 0));
        }

        [TestMethod]
        public void InvDivS32Test() {
            Assert.IsTrue(0.InvDivS32(1).SequenceEqual(new[] { 0 }));
            Assert.IsTrue(0.InvDivS32(3).SequenceEqual(new[] { -2, -1, 0, 1, 2 }));
            Assert.IsTrue(1.InvDivS32(1).SequenceEqual(new[] { 1 }));
            Assert.IsTrue(1.InvDivS32(2).SequenceEqual(new[] { 2, 3 }));
            Assert.IsTrue((int.MinValue / 3).InvDivS32(3).OrderBy(e => e).SequenceEqual(new[] { int.MinValue, int.MinValue + 1, int.MinValue + 2 }));
            Assert.IsTrue(5.InvDivS32(4).SequenceEqual(new[] { 20, 21, 22, 23 }));
            Assert.IsTrue((-6).InvDivS32(5).SequenceEqual(new[] { -30, -31, -32, -33, -34 }));
        }
    }
}
