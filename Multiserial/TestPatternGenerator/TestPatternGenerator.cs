//-----------------------------------------------------------------------
// <copyright file="TestPatternGenerator.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
namespace MultiSerialTestPattern
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Geerates a test pattern out of a defined stimulus descriptor.
    /// </summary>
    public class TestPatternGenerator
    {
        /// <summary>
        /// Local copy of the baud rate.
        /// </summary>
        private int baud;

        /// <summary>
        /// Stores the states corresponding to the passed stimulus.
        /// </summary>
        private List<LineState> states = new List<LineState>();

        /// <summary>
        /// The duration of the bit according to the selected baud rate in microseconds.
        /// </summary>
        private double microsecondPerBit;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPatternGenerator"/> class.
        /// </summary>
        public TestPatternGenerator()
        {
            this.Baud = StimulusDescriptor.Baud;
            StimulusDescriptor.Foreach((s) =>
            {
                GenerateStates(s);
            });
        }

        /// <summary>
        /// Sets the baud rate of the generated pattern.
        /// </summary>
        private int Baud
        {
            set
            {
                this.baud = value;
                this.microsecondPerBit = 1E6 / this.baud;
            }
        }

        /// <summary>
        /// Iterator that applies the passed lambda to each state stored.
        /// </summary>
        /// <param name="visitor">Action lambda accepting the current time and the state of all the lines as an unsigned int.</param>
        public void ForeachState(Action<int, uint> visitor)
        {
            this.states.Sort(new LineState());
            uint state = ~0u;
            int currentTime;
            bool isEnd = false;

            for (int i = 0; i < this.states.Count && !isEnd; i++)
            {
                currentTime = this.states[i].When;
                for (int j = i; j < this.states.Count; j++)
                {
                    if (currentTime != this.states[j].When)
                    {
                        i = j - 1;
                        break;
                    }

                    isEnd = j == this.states.Count - 1;
                    uint theMask = 1u << this.states[j].Line;
                    if (this.states[j].State)
                    {
                        state |= theMask;
                    }
                    else
                    {
                        state &= ~theMask;
                    }
                }

                Console.WriteLine(">>>>>>{0} {1:X4} i {2}\n", currentTime, state, i);
                visitor(currentTime, state);
            }
        }

        /// <summary>
        /// Addds a state on the passed line at the passed time with the passed logic level.
        /// </summary>
        /// <param name="us">Time in microseconds the state refers to.</param>
        /// <param name="line">Serial line the state referes to.</param>
        /// <param name="level">Logic leve of the line at the defined time.</param>
        private void AddState(int us, int line, bool level)
        {
            this.states.Add(new LineState
            {
                When = us,
                Line = line,
                State = level
            });
        }

        /// <summary>
        /// Generates a sequence of states corresponding to the passed character transmitted on the passed serial line at the defined baud rate.
        /// </summary>
        /// <param name="currentTime">Time in microseconds from start when the transmission starts.</param>
        /// <param name="line">Serial line on which the character is transmitted.</param>
        /// <param name="c">Ascii code of the character being sent.</param>
        /// <returns>The time when the transmissione ends.</returns>
        private int GenerateStates(int currentTime, int line, byte c)
        {
            int finalTime = currentTime;
            //// Adds start and stop
            int stateSequence = (c << 1) | 0x1;
            for (int mask = 0x200; mask > 0; mask >>= 1)
            {
                this.AddState(finalTime, line, (stateSequence & mask) != 0);
                finalTime = (int)(finalTime + this.microsecondPerBit);
            }

            return finalTime;
        }

        /// <summary>
        /// Generates the states corresponding to the passed stimulus.
        /// </summary>
        /// <param name="descriptor">Descriptor of the stimulus to be generated.</param>
        private void GenerateStates(StimulusDescriptor descriptor)
        {
            int currentTimeMicroseconds = descriptor.WhenMicrosecond;
            foreach (byte c in Encoding.ASCII.GetBytes(descriptor.ToSend))
            {
                currentTimeMicroseconds = this.GenerateStates(currentTimeMicroseconds, descriptor.LineId, c);
            }
        }

        /// <summary>
        /// Describes the state of a single line at the passed time.
        /// </summary>
        private class LineState : IComparer<LineState>
        {
            /// <summary>
            /// Gets or sets the line number the state references to.
            /// </summary>
            public int Line
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the referenced line is high or low.
            /// </summary>
            public bool State
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the time when the state references to.
            /// </summary>
            public int When
            {
                get;
                set;
            }

            /// <summary>
            /// Compares two <see cref="LineState"/>
            /// </summary>
            /// <param name="x">First <see cref="LineState"/> to compare.</param>
            /// <param name="y">Second <see cref="LineState"/> to compare.</param>
            /// <returns>-1 0 or +1 if x&lt;=&gt; y</returns>
            public int Compare(LineState x, LineState y)
            {
                return x.When - y.When;
            }
        }
    }
}