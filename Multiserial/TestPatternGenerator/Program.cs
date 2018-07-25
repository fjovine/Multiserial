//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
namespace MultiSerialTestPattern
{
    using System;
    using System.IO;

    /// <summary>
    /// This C# program generates test patterns to test the Multiserial application
    /// Its aim is to generate text files that are submitted to the test suite of the multiserial C software having the following format
    /// {time} {state}
    /// {time1} {state 1}
    /// where the time is the time in microseconds of a transition and state is a word of 16 bits representing the new state
    /// This file accepts an text file that has the following format
    /// {time} {line} {string}
    /// Where {time} is the time in microseconds when the first character of the corresponding {string} is emitted on the line {line}
    /// For instance 
    /// 123 1 "This is a string"
    /// will generate a sequence of continuous ascii This C# program generates test patterns to test the Multiserial application starting at microsecond 123
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Entry point of the test pattern generator.
        /// </summary>
        /// <param name="args">Command line arguments. If one argument is present, then the stimulus is loaded from the file named by this arguments, else the stimulus is read from stdin.</param>
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Syntax TestPatternGenerator <stimulusFile>");
                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("The required {0} file does not exist", args[0]);
            }

            StimulusDescriptor.LoadFrom(args[0]);
            TestPatternGenerator testPattern = new TestPatternGenerator();
            string hexOutput = Path.ChangeExtension(args[0], "hex");
            using (TextWriter tw = File.CreateText(hexOutput))
            {
                testPattern.ForeachState((w, s) =>
                {
                    tw.WriteLine("{0} {1:X4}", w, s);
                });
            }
        }
    }
}
