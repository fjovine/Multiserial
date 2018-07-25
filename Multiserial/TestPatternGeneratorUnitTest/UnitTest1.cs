//-----------------------------------------------------------------------
// <copyright file="UnitTest1.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
namespace MultiSerialTestPatternUnitTest
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using MultiSerialTestPattern;

    [TestClass]
    public class StimulusDescriptorUnitTest
    {
        [TestMethod]
        public void LoadFrom_WorksWell_WithCorectInput()
        {
            string[] testProgram =
            {
                "9600",
                "0 0 \"Test linea 0\"",
                "0 1 \"Test linea 1\"",
                "0 2 \"Test linea 2\"",
                "0 3 \"Test linea 3\"",
                "0 4 \"Test linea 4\"",
            };

            StimulusDescriptor.LoadFrom(testProgram);
            Assert.AreEqual(9600, StimulusDescriptor.Baud);
            int count = 0;
            StimulusDescriptor.Foreach(s =>
            {
                Assert.AreEqual(0, s.WhenMicrosecond);
                Assert.AreEqual(count, s.LineId);
                Assert.IsTrue(s.ToSend.StartsWith("Test linea"));
                count++;
            });
            Assert.AreEqual(count, 5);
        }

        [TestMethod]
        public void LoadFrom_EmitsException_WithWrongBaudSyntax()
        {
            string[] testProgram =
            {
                "blob"
            };

            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram), "The first significant line must contain the simulated baud rate");
        }

        [TestMethod]
        public void LoadFrom_EmitsException_WithWrongBaud()
        {
            string[] testProgram =
            {
                "  ;",
                "300"
            };

            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram), "This baud rate is not supported");
        }

        [TestMethod]
        public void LoadFrom_EmitsException_WithWrongMicrosecond()
        {
            string[] testProgram =
            {
                "9600",
                "sdfasd 1 \"Test linea 4\""
            };

            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram), "The first field must be an integer number representing a time in microseconds");
        }

        [TestMethod]
        public void LoadFrom_EmitsException_WithWrongLine()
        {
            string[] testProgram =
            {
                "9600",
                "0 -1 \"Test linea 4\""
            };

            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram), "The line must be an integer from 0 to 15");
        }

        [TestMethod]
        public void LoadFrom_EmitsException_WithWrongStringFormat()
        {
            string expectedErrorMessage = "The third field must be a string delimited by quotation marks";
            string[] testProgram1 = { "9600", "0 -1 \"Test linea 4" };
            string[] testProgram2 = { "9600", "0 -1 Test linea 4\"" };
            string[] testProgram3 = { "9600", "0 -1 Test linea 4" };

            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram1), expectedErrorMessage);
            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram2), expectedErrorMessage);
            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram3), expectedErrorMessage);
        }

        [TestMethod]
        public void LoadFrom_EmitsException_BecauseFollowingStringStartsBeforeTheEndOfThePrevousOne()
        {
            string[] testProgram =
            {
                "9600",
                "0 1 \"Test linea 1\"",
                "5 1 \"Test linea 2\""
            };

            Assert.ThrowsException<StimulusDescriptorException>(() => StimulusDescriptor.LoadFrom(testProgram), "The line must be an integer from 0 to 15");
        }
    }
}
