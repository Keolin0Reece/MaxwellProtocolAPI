using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using ConsoleApp1.Models;
using ConsoleApp1;
using static System.Net.Mime.MediaTypeNames;

namespace APIService
{
    public interface ISerialPortService
    {
        void SendData();
    }

    public class SerialPortService : ISerialPortService, IDisposable
    {
        private readonly SerialPort _serialPort;
        public MessageType[] messageTypes;
        private readonly StringBuilder _buffer; // Buffer to accumulate data
        private const int MaxBufferSize = 1024; // Maximum allowed buffer size to prevent unbounded growth
        private List<byte> dynamicBuffer;
        private static float voltage = 0;

        public SerialPortService()
        {
            _serialPort = new SerialPort
            {
                PortName = "COM6", // Update to your COM port
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                NewLine = "\n" // Set the NewLine character explicitly
            };

            messageTypes = MessageTypeLoader.LoadMessageTypesFromJson("C:\\Users\\impul\\Documents\\MaxwellnSpark\\APIService\\TOMessage.json");

            _buffer = new StringBuilder();
;
            _serialPort.DataReceived += SerialPort_DataReceived; // Subscribe to the event

            dynamicBuffer = new List<byte>();

            _serialPort.Open();
        }

        public void SendData()
        {
            if (_serialPort.IsOpen)
            {
                // Encode and send a WMBUS message (example message)
                Message message1 = new Message
                {
                    MessageData = "3",
                    TypeofData = messageTypes[0]
                };

                byte[] encodedMessage = WMBUSProtocol.EncodeWMBUSMessage(0x01, message1);
                _serialPort.Write(encodedMessage, 0, encodedMessage.Length);
            }
            else
            {
                throw new InvalidOperationException("Serial port is not open.");
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] tempBuffer = new byte[256];
                int bytesRead = _serialPort.Read(tempBuffer, 0, tempBuffer.Length);

                if (bytesRead > 0)
                {
                    // Add only the valid portion of tempBuffer to dynamicBuffer
                    for (int i = 0; i < bytesRead; i++)
                    {
                        dynamicBuffer.Add(tempBuffer[i]);
                    }

                    // Process messages from the buffer
                    ProcessBuffer(dynamicBuffer);

                    // Ensure buffer does not grow uncontrollably
                    if (dynamicBuffer.Count > MaxBufferSize)
                    {
                        Console.WriteLine("Warning: Buffer exceeded maximum size. Clearing buffer to prevent memory issues.");
                        dynamicBuffer.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading serial data: {ex.Message}");
            }
        }
        private static void ProcessBuffer(List<byte> buffer)
        {
            while (true)
            {
                // Look for the newline character as the delimiter
                int newlineIndex = buffer.IndexOf((byte)'\n');
                if (newlineIndex == -1)
                {
                    // No complete message in the buffer yet
                    break;
                }

                // Extract the full message (up to the newline character)
                byte[] messageBytes = buffer.GetRange(0, newlineIndex).ToArray();

                // Remove the processed message (including the newline character) from the buffer
                buffer.RemoveRange(0, newlineIndex + 1);

                // Decode the WMBUS message
                ProtocolMessage message = WMBUSProtocol.DecodeWMBUSMessage(messageBytes, messageBytes.Length);

                if (message != null)
                {
                    //Console.WriteLine("Received WMBUS message:");
                    //Console.WriteLine(message.ToString());
                    voltage = ByteArrayToInt(message.Data) * (5.0f /1023);
                    Console.WriteLine(voltage.ToString("0.00"));
                }
                else
                {
                    Console.WriteLine("Failed to decode WMBUS message.");
                }
            }
        }
        public static int ByteArrayToInt(byte[] byteArray)
        {
            int result = 0;

            // Iterate through the array from left to right, multiplying by the correct power of 256
            for (int i = 0; i < byteArray.Length; i++)
            {
                result = (result << 8) | byteArray[i];
            }

            return result;
        }
    

        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }

            _serialPort.Dispose();
        }
    }
}
