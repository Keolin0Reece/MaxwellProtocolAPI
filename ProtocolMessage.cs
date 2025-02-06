using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// Represents a decoded WMBUS protocol message.
    /// </summary>
    public class ProtocolMessage
    {
        public byte MessageID { get; set; }
        public int MessageLength { get; set; }
        public int MessageTypeID { get; set; }
        public byte[] Data { get; set; }
        public byte Checksum { get; set; }
        public bool IsValidChecksum { get; set; }

        /// <summary>
        /// Returns a string representation of the protocol message.
        /// </summary>
        public override string ToString()
        {
            return $"Message ID: {MessageID}\n" +
                   $"Message Length: {MessageLength}\n" +
                   $"MessageType ID: {MessageTypeID}\n" +
                   $"Data: {BitConverter.ToString(Data).Replace("-", " ")}\n" +
                   $"Checksum: 0x{Checksum:X2}\n" +
                   $"Is Checksum Valid: {IsValidChecksum}";
        }
    }
}
