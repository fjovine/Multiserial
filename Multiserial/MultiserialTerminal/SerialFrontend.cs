//--------------------------------------------------------------------Serial---
// <copyright file="SerialFrontend.cs" company="fjm">
//     Copyright (c) Francesco Iovine.
// </copyright>
// <author>Francesco Iovine iovinemeccanica@gmail.com</author>
//-----------------------------------------------------------------------

namespace MultiserialTerminal
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Ports;
    using System.Text;

    /// <summary>
    /// Reads the multiplexed serial lines attached to a higher speed serial line and demultiplexes generating specific events whenever a new character
    /// arrives from the selected port.
    /// </summary>
    public class SerialFrontend
    {
        /// <summary>
        /// Port connected to the sniffer device.
        /// </summary>
        private readonly SerialPort multiplexedPort;

        /// <summary>
        /// Object used to synchronize the receiving process of packets. When data is being processed, this object
        /// is locked forcing incoming requests to wait for the previous packet processing to be finished.
        /// </summary>
        private readonly object sync = new object();

        /// <summary>
        /// Repository where lines are stored.
        /// </summary>
        private Dictionary<byte, StringBuilder> lineBuffer = new Dictionary<byte, StringBuilder>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Sniffer" /> class.
        /// </summary>
        /// <param name="snifferPort">Filename of the simulated port</param>
        public SerialFrontend(string snifferPort)
        {
            this.multiplexedPort = new SerialPort(snifferPort);
            this.multiplexedPort.BaudRate = 115200;
            this.multiplexedPort.Parity = Parity.None;
            this.multiplexedPort.StopBits = StopBits.One;
            this.multiplexedPort.DataBits = 8;
        }

        /// <summary>
        /// Delegate used by the SniffedRowAvailable event.
        /// </summary>
        /// <param name="sender">Sender object of the event.</param>
        /// <param name="e">Arguments of the event.</param>
        public delegate void ChannelByteAvailable(object sender, ByteChannelEventArgs e);

        /// <summary>
        /// Event triggered whenever a new row is available, coming from any ports.
        /// </summary>
        public event ChannelByteAvailable ByteAvailable;

        /// <summary>
        /// Delivers the SniffedRowAvailable event.
        /// </summary>
        /// <param name="e">Class containing information about the received row.</param>
        public void OnByteAvailable(ByteChannelEventArgs e)
        {
            this.ByteAvailable?.Invoke(this, e);
        }

        /// <summary>
        /// Starts the process of receiving and installs a data received event handler
        /// </summary>
        /// So a byte with the MSB set signals the firs byte to be decoded.
        /// <returns>True if port could be opened.</returns>
        public bool OpenAndDemultipex()
        {
            bool result = true;

            try
            {
                bool inSync = false;
                byte high = 0;

                this.multiplexedPort.DataReceived += (s, e) =>
                {
                    lock (sync)
                    {
                        byte[] receiveBuffer = new byte[this.multiplexedPort.ReadBufferSize];
                        int bytesRead = this.multiplexedPort.Read(receiveBuffer, 0, receiveBuffer.Length);

                        for (int i = 0; i < bytesRead; i++)
                        {
                            byte currentByte = receiveBuffer[i];
                            if ((currentByte & 0x80) != 0)
                            {
                                high = currentByte;
                                inSync = true;
                            }
                            else
                            {
                                if (inSync)
                                {
                                    this.OnByteAvailable(new ByteChannelEventArgs(high, currentByte));
                                }
                            }
                        }
                    }
                };

                this.multiplexedPort.Open();
            }
            catch (IOException)
            {
                result = false;
            }

            return result;
        }

        public void Close()
        {
            this.multiplexedPort.Close();
        }
    }
}
