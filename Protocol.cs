using ConsoleApp1;
using ConsoleApp1.Models;
using System;
using System.Text;

namespace APIService
{
    /// <summary>
    /// Provides encoding and decoding methods for WMBUS communication protocol.
    /// </summary>
    public static class WMBUSProtocol
    {
        /// <summary>
        /// Encodes a message into WMBUS format.
        /// </summary>
        /// <param name="messageID">The message identifier.</param>
        /// <param name="data">The message to encode.</param>
        /// <returns>Byte array representing the WMBUS packet.</returns>
        public static byte[] EncodeWMBUSMessage(byte messageID, Message data)
        {
            byte[] payload = Encoding.ASCII.GetBytes(data.MessageData);
            byte checksum = CalculateChecksum(messageID, payload);

            // Construct WMBUS packet: [Message ID] [Message Length] [Message Type ID] [Payload] [Checksum] [End Marker]
            byte[] wmbusPacket = new byte[3 + payload.Length + 2];
            wmbusPacket[0] = messageID;                   // Message ID
            wmbusPacket[1] = (byte)payload.Length;        // Message Length
            wmbusPacket[2] = (byte)data.TypeofData.MessageTypeID; // Message Type ID
            Array.Copy(payload, 0, wmbusPacket, 3, payload.Length); // Payload
            wmbusPacket[wmbusPacket.Length - 2] = checksum;         // Checksum
            wmbusPacket[wmbusPacket.Length - 1] = (byte)'\n';       // End marker

            return wmbusPacket;
        }

        /// <summary>
        /// Decodes a WMBUS message from raw bytes.
        /// </summary>
        /// <param name="buffer">The received byte buffer.</param>
        /// <param name="length">The length of valid data in the buffer.</param>
        /// <returns>A <see cref="ProtocolMessage"/> object if decoding is successful.</returns>
        /// <exception cref="ArgumentException">Thrown if the buffer length is invalid.</exception>
        public static ProtocolMessage DecodeWMBUSMessage(byte[] buffer, int length)
        {
            if (buffer == null || length < 5)
            {
                throw new ArgumentException("Invalid buffer length for WMBUS decoding.");
            }

            byte messageID = buffer[0];
            byte messageLength = buffer[1];
            byte messageTypeID = buffer[2];

            if (messageLength + 3 > length - 2) // Ensuring data length matches expected size
            {
                throw new ArgumentException("Message length mismatch.");
            }

            byte[] data = new byte[messageLength];
            Array.Copy(buffer, 3, data, 0, messageLength);

            byte checksum = buffer[length - 2];
            byte endMarker = buffer[length - 1];

            if (endMarker != (byte)'\n')
            {
                throw new ArgumentException("Invalid message format: Missing end marker.");
            }

            bool isValidChecksum = checksum == CalculateChecksum(messageID, data);

            return new ProtocolMessage
            {
                MessageID = messageID,
                MessageLength = messageLength,
                MessageTypeID = messageTypeID,
                Data = data,
                Checksum = checksum,
                IsValidChecksum = isValidChecksum
            };
        }

        /// <summary>
        /// Calculates a checksum for a WMBUS message.
        /// </summary>
        /// <param name="messageID">The message identifier.</param>
        /// <param name="data">The message payload.</param>
        /// <returns>The calculated checksum byte.</returns>
        private static byte CalculateChecksum(byte messageID, byte[] data)
        {
            int checksum = messageID;
            foreach (byte b in data)
            {
                checksum += b; // Sum all bytes
            }
            return (byte)(checksum % 255);
        }
    }
}
