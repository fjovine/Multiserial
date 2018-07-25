using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MultiserialIntegrationTest
{
    [TestClass]
    public class UnitTest1
    {
        public void TestCase(string[] stimulus, string[] expected)
        {
            MultiserialIntegrationTester tester = new MultiserialIntegrationTester(stimulus);
            List<string> got = tester.LaunchAndDecodeResult();
            Assert.AreEqual(got.Count, expected.Length);
            for (int i=0; i<expected.Length; i++)
            {
                Assert.AreEqual(expected[i], got[i]);
            }

        }

        [TestMethod]
        public void TestMethod1()
        {
            TestCase(
                new string[] {
                    "9600",
                    "0 0 \"01234\""
                },
                new string[]
                {
                   "0,30",
                   "0,31",
                   "0,32",
                   "0,33",
                   "0,34"
                }
            );
        }

        [TestMethod]
        public void TestMethod2()
        {
            TestCase(
                new string[] {
                    "9600",
                    "0 0 \"01234\"",
                    "0 1 \"01234\""
                },
                new string[]
                {
                   "0,30",
                   "1,30",
                   "0,31",
                   "1,31",
                   "0,32",
                   "1,32",
                   "0,33",
                   "1,33",
                   "0,34",
                   "1,34"
                }
            );
        }
    }
}
