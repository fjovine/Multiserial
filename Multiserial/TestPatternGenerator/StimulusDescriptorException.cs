//-----------------------------------------------------------------------
// <copyright file="StimulusDescriptorException.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
namespace MultiSerialTestPattern
{
    using System;

    /// <summary>
    /// Descriptor of a problem decoding the stimulus source file.
    /// </summary>
    public class StimulusDescriptorException : Exception
    {
        /// <summary>
        /// Local copy of the source code number that generates the exception.
        /// </summary>
        private int numLine;

        /// <summary>
        /// Initializes a new instance of the <see cref="StimulusDescriptorException"/> class.
        /// </summary>
        /// <param name="line">Line number of the strimulus source file where that generates the exception.</param>
        /// <param name="message">Error message describing the proble.</param>
        public StimulusDescriptorException(int line, string message) : base(message)
        {
            this.numLine = line;
        }
    }
}
