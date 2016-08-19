using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace XModemProtocol {
    /// <summary>
    /// A class of extensions I use all throughout this library.
    /// </summary>
    internal static class PortExtensions {
        /// <summary>
        /// Writes the specified byte to the port.
        /// </summary>
        /// <param name="p">The port to use.</param>
        /// <param name="buffer">The byte to be written to the port.</param>
        public static void Write(this SerialPort p, byte buffer) {
            p.Write(new byte[] { buffer }, 0, 1);
        }

        /// <summary>
        /// Writes entire collectiion to the port.
        /// </summary>
        /// <param name="p">The port to use.</param>
        /// <param name="buffer">The byte array to be written to the port.</param>
        public static void Write(this SerialPort p, IEnumerable<byte> buffer) {
            byte[] bufferArray = buffer.ToArray();
            p.Write(bufferArray, 0, bufferArray.Length);
        }

        /// <summary>
        /// Flushes out of memory both the input buffer, and output buffer.
        /// </summary>
        /// <param name="p">The port to use.</param>
        public static void Flush(this SerialPort p) {
            p.DiscardInBuffer();
            p.DiscardOutBuffer();
        }

        /// <summary>
        /// Reads all available bytes at Port.
        /// </summary>
        /// <param name="p">The port to use.</param>
        /// <returns>A list containing bytes read.</returns>
        public static List<byte> ReadAllBytes(this SerialPort p) {
            int bytesToRead = p.BytesToRead;
            byte[] byteArray = new byte[bytesToRead];
            p.Read(byteArray, 0, bytesToRead);
            return byteArray.ToList();
        }

    }
}
