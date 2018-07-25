using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MultiSerialTestPattern;

namespace MultiSerialTestPatternUnitTest
{
    [TestClass]
    public class TestPatternGeneratorUnitTest
    {
        private void TestPatternChecker(string[] testProgram, string[] expectedResult)
        {
            int i = 0;
            StimulusDescriptor.LoadFrom(testProgram);
            TestPatternGenerator testPattern = new TestPatternGenerator();
            testPattern.ForeachState((w, s) =>
            {
                string ss = string.Format("{0:0000} {1:X8}", w, s);
                Console.WriteLine("ss {0}", ss);
                Assert.AreEqual(expectedResult[i++], ss);
            });
            Assert.AreEqual(expectedResult.Length, i);
        }

        [TestMethod]
        public void TestPatternGenerator_WorksWell_WithASingleCharacter()
        {
            TestPatternChecker(
                new string[] { "9600", "0 0 \"0\""},
                new string[] { "0000 FFFFFFFE", "0104 FFFFFFFE", "0208 FFFFFFFE", "0312 FFFFFFFF", "0416 FFFFFFFF", "0520 FFFFFFFE", "0624 FFFFFFFE", "0728 FFFFFFFE", "0832 FFFFFFFE", "0936 FFFFFFFF" });
        }

        [TestMethod]
        public void TestPatternGenerator_WorksWell_WithASingleCharacterOnTwoLines()
        {
            TestPatternChecker(
                new string[] { "9600", "0 0 \"0\"", "0 1 \"0\"" },
                new string[] { "0000 FFFFFFFC", "0104 FFFFFFFC", "0208 FFFFFFFC", "0312 FFFFFFFF", "0416 FFFFFFFF", "0520 FFFFFFFC", "0624 FFFFFFFC", "0728 FFFFFFFC", "0832 FFFFFFFC", "0936 FFFFFFFF" });
        }

        [TestMethod]
        public void TestPatternGenerator_WorksWell_WithASingleCharacterOnLinee0AndTwoOnLine1()
        {
            TestPatternChecker(
                new string[] { "9600", "0 0 \"0\"", "0 1 \"01\"" },
                new string[] {
                    "0000 FFFFFFFC", "0104 FFFFFFFC", "0208 FFFFFFFC", "0312 FFFFFFFF", "0416 FFFFFFFF", "0520 0000", "0624 0000", "0728 0000", "0832 0000",
                    "0936 0003", "1040 0000", "1144 0000", "1248 0000", "1352 0002", "1456 0002", "1560 0000", "1664 0000", "1768 0000", "1872 0002", "1976 0002" });
        }

    }
}
