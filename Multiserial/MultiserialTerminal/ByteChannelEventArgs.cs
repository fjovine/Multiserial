//----------------------------------------------------------------------
// <copyright file="ByteChannelEventArgs.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------
namespace MultiserialTerminal
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Arguments of the ByteChannelEvent that carries information about the received byte.<para/>
    /// The information contained is
    /// <list type="">
    /// <item>The channel that originates the byte</item>
    /// <item>The arrived byge.</item>
    /// </list>
    /// </summary>
    public class ByteChannelEventArgs : EventArgs
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteChannelEventArgs" /> class.
        /// The bytes arrive in this format
        /// HI 1SSS SCCC
        /// LO 000C CCCC
        /// Where SSSS are the bits indicating the source channel
        /// and CCCCCCCC are the bits composing the byte.
        /// </summary>
        /// <param name="high">Most significant byte received</param>
        /// <param name="low">Least significant byte received</param>
        public ByteChannelEventArgs(byte high, byte low)
        {
            if ((high & 0x80) != 0x80)
            {
                throw new ArgumentException("The most significant byte must have the most significant bit set.");
            }

            this.FromChannel = (high & 0x78) >> 3;
            this.TheByte = (byte)(((high & 0x7) << 5) | (low & 0x1F));
        }

        /// <summary>
        /// Gets where the bytecomes from.
        /// </summary>
        public int FromChannel
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the arrived byte.
        /// </summary>
        public byte TheByte
        {
            get;
            private set;
        }
    }
}
