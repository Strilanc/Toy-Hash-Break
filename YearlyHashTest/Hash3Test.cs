using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class Hash3Test {
    [TestMethod]
    public void HashTest() {
        foreach (var test in new[] {"", "a", "<+test", "tes", "test", "abcdefghijklmnop"})
            Assert.AreEqual(Hash3.Hash(MainHash.Encode(test)), MainHash.Hash(MainHash.Encode(test)));
    }
}
