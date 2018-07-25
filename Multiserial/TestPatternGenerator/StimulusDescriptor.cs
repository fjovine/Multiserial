//-----------------------------------------------------------------------
// <copyright file="StimulusDescriptor.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
namespace MultiSerialTestPattern
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Describes a stimulus, i.e. a serial string emitted on a specified line.
    /// </summary>
    public class StimulusDescriptor
    {
        /// <summary>
        /// Serial lines simulated: from 0 to MAXLINES.
        /// </summary>
        private const int MAXLINES = 15;

        /// <summary>
        /// List of built stimuli.
        /// </summary>
        private static List<StimulusDescriptor> stimuli = new List<StimulusDescriptor>();

        /// <summary>
        /// Contains all the supported baud rates acceptable.
        /// </summary>
        private static int[] supportedBauds = { 9600 };

        /// <summary>
        /// Gets the baud rate of the serial line.
        /// </summary>
        public static int Baud
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the identifier id where to send the <see cref="ToSend"/> message.
        /// </summary>
        public int LineId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the string to emit.
        /// </summary>
        public string ToSend
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets when the stimulus happens in microseconds.
        /// </summary>
        public int WhenMicrosecond
        {
            get;
            private set;
        }

        /// <summary>
        /// Iterates the passed action to each stimulus.
        /// </summary>
        /// <param name="visitor">Action accepting each stimulus in the list.</param>
        public static void Foreach(Action<StimulusDescriptor> visitor)
        {
            foreach (var stimulus in stimuli)
            {
                visitor(stimulus);
            }
        }

        /// <summary>
        /// Loads the stimulus from a line array.
        /// </summary>
        /// <param name="lines">Array of llines containing the stimulus file.</param>
        public static void LoadFrom(string[] lines)
        {
            bool isBaudDefined = false;
            stimuli = new List<StimulusDescriptor>();

            for (int numLine = 0; numLine < lines.Length; numLine++)
            {
                string trimmedLine = lines[numLine].Trim();
                if (trimmedLine[0] == ';')
                {
                    // comment
                    continue;
                }

                if (!isBaudDefined)
                {
                    int baud;
                    if (!int.TryParse(trimmedLine, out baud))
                    {
                        throw new StimulusDescriptorException(numLine, "The first significant line must contain the simulated baud rate");
                    }

                    Baud = baud;
                    if (!supportedBauds.Contains<int>(baud))
                    {
                        throw new StimulusDescriptorException(numLine, "This baud rate is not supported");
                    }

                    isBaudDefined = true;
                }
                else
                {
                    int microseconds;
                    int theLine;
                    string send;
                    int firstBlank = trimmedLine.IndexOf(' ');
                    string firstField = trimmedLine.Substring(0, firstBlank);

                    if (!int.TryParse(firstField, out microseconds))
                    {
                        throw new StimulusDescriptorException(numLine, "The first field must be an integer number representing a time in microseconds");
                    }

                    string restingLine = trimmedLine.Substring(firstBlank).Trim();
                    int secondBlank = restingLine.IndexOf(' ');
                    string secondField = restingLine.Substring(0, secondBlank);

                    if (!int.TryParse(secondField, out theLine))
                    {
                        throw new StimulusDescriptorException(numLine, "The second field must be an integer number representing a line the string is sent to");
                    }

                    if (theLine < 0 || theLine > 15)
                    {
                        throw new StimulusDescriptorException(numLine, "The line must be an integer from 0 to 15");
                    }

                    restingLine = restingLine.Substring(secondBlank).Trim();
                    if (!restingLine.StartsWith("\"") || !restingLine.EndsWith("\""))
                    {
                        throw new StimulusDescriptorException(numLine, "The third field must be a string delimited by quotation marks");
                    }
                    else
                    {
                        send = restingLine.Substring(1, restingLine.Length - 2);
                    }

                    stimuli.Add(new StimulusDescriptor
                    {
                        WhenMicrosecond = microseconds,
                        LineId = theLine,
                        ToSend = send
                    });
                }
            }

            PerformCheck();
        }

        /// <summary>
        /// Loads the stimulus from a file.
        /// </summary>
        /// <param name="fileName">File name whence the stimuli are loaded.</param>
        public static void LoadFrom(string fileName)
        {
            string[] lines = System.IO.File.ReadAllLines(fileName);
            LoadFrom(lines);
        }

        /// <summary>
        /// Generates the states and checks if a string is emitted before the former on the same line has been totally sent.
        /// </summary>
        private static void PerformCheck()
        {
            for (int line = 0; line <= MAXLINES; line++)
            {
                Dictionary<int, int> initial2Index = new Dictionary<int, int>();
                for (int i = 0; i < stimuli.Count; i++)
                {
                    StimulusDescriptor stimulus = stimuli[i];
                    if (stimulus.LineId != line)
                    {
                        continue;
                    }

                    initial2Index.Add(stimulus.WhenMicrosecond, i);
                }

                if (initial2Index.Count <= 1)
                {
                    continue;
                }

                int[] times = initial2Index.Keys.ToArray<int>();
                Array.Sort<int>(times);
                for (int i = 0; i < times.Length - 1; i++)
                {
                    // We have 8 bit data + 1 bit start 1bit stop = 10;
                    double microsecondsPerChar = 1E6 / StimulusDescriptor.Baud * 10.0;
                    int startTime = times[i];
                    StimulusDescriptor stimulus = stimuli[initial2Index[startTime]];
                    int endCurrentString = (int)(startTime + (microsecondsPerChar * stimulus.ToSend.Length));
                    if (endCurrentString > times[i + 1])
                    {
                        throw new StimulusDescriptorException(i, "The following string starts before the former one has ended");
                    }
                }
            }
        }
    }
}
