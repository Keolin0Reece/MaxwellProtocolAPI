using ConsoleApp1;
using ConsoleApp1.Models;
using System;
using System.Text;

namespace APIService
{
    public static class WMBUSProtocol
    {
        /// <summary>
        /// Encodes a message into WMBUS format.
        /// </summary>
        public static byte[] EncodeWMBUSMessage(byte messageID, Message data)
        {
            byte[] payload = Encoding.ASCII.GetBytes(data.MessageData);
            byte checksum = CalculateChecksum(messageID, payload);

            // Construct WMBUS packet: [Message ID] [Message Length] [Payload] [Checksum]
            byte[] wmbusPacket = new byte[3 + payload.Length + 2];
            wmbusPacket[0] = messageID;               // Message ID
            wmbusPacket[1] = (byte)payload.Length;    // Message Length
            wmbusPacket[2] = (byte)data.TypeofData.MessageTypeID;
            Array.Copy(payload, 0, wmbusPacket, 3, payload.Length); // Payload
            wmbusPacket[wmbusPacket.Length - 2] = checksum;         // Checksum
            wmbusPacket[wmbusPacket.Length - 1] = (byte) '\n';

            return wmbusPacket;
        }

        /// <summary>
        /// Decodes a WMBUS message from raw bytes.
        /// </summary>

        public static ProtocolMessage DecodeWMBUSMessage(byte[] buffer, int length)
        {
            try
            {
                byte messageID = buffer[0];
                byte messageLength = buffer[1];
                byte messageTypeID = buffer[2];

                byte[] data = new byte[messageLength];
                Array.Copy(buffer, 3, data, 0, messageLength);

                byte checksum = buffer[length - 1];
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calculates a checksum for a WMBUS message.
        /// </summary>
        private static byte CalculateChecksum(byte messageID, byte[] data)
        {
            int checksum = messageID;
            foreach (byte b in data)
            {
                checksum += b; // Add all bytes
            }
            checksum = checksum % 255;
            return (byte) checksum;
        }
    }
}
