using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class Hash4Test {
    private static void AssertCanBreak(string prefix, string suffix) {
        var start = MainHash.Hash(MainHash.Encode(prefix));
        var end = MainHash.Hash(MainHash.Encode(prefix + suffix));
        var breakSuffix = MainHash.Decode(Hash4.Break(end, suffix.Length, cache:false, start: start));
        Assert.AreEqual(suffix, breakSuffix);        
    }
    [TestMethod]
    public void BreakTest1() {
        AssertCanBreak("fjdh", "glfi");
    }
    [TestMethod]
    public void BreakTest2() {
        AssertCanBreak("", "a");
    }
    [TestMethod]
    public void BreakTest3() {
        AssertCanBreak("", "");
    }
    [TestMethod]
    public void BreakTest4() {
        AssertCanBreak("a", "");
    }
    [TestMethod]
    public void BreakTest5() {
        AssertCanBreak("a", "bc");
    }
}
