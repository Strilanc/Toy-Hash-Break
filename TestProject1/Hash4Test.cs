using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class Hash4Test {
    [TestMethod]
    public void HashTest() {
        foreach (var test in new[] {"", "a", "<+test", "tes", "test", "abcdefghijklmnop"})
            Assert.AreEqual(Hash4.Hash(MainHash.Encode(test)), MainHash.Hash(MainHash.Encode(test)));
    }
    [TestMethod]
    public void BreakTest() {
        var r = MainHash.Hash(MainHash.Encode("fjdhglfi"));
        Assert.AreEqual("glfi", MainHash.Decode(Hash4.Break(r, 4, MainHash.Hash(MainHash.Encode("fjdh")))));
    }
    [TestMethod]
    public void Name2Test() {
        Assert.AreEqual(MainHash.Hash(MainHash.Encode("Procyon")), new HashState(0x605D4A4F, 0x7EDDB1E5));
    }
}
