using ConsoleApp1.Models;
using ConsoleApp1;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace APIService
{
//---------------------------------------------------
// INITIALIZATIONS
//---------------------------------------------------
    // Interface for serial port communication services
    public interface ISerialPortService
    {
        void SendData(); // Method to send data to the serial port
    }

    // Main service for handling serial port communication and processing WMBUS protocol messages
    public class SerialPortService : ISerialPortService, IDisposable
    {
        private readonly SerialPort _serialPort;
        private readonly IHubContext<ArduinoHub> _hubContext;

        private const int MaxBufferSize = 1024; // Maximum buffer size to avoid memory issues
        private List<byte> dynamicBuffer; // Dynamically growing buffer to accumulate incoming data
        public MessageType[] messageTypes; // Array to store message types
        private readonly StringBuilder _buffer; // Buffer to accumulate data
        
        // Constructor to initialize the serial port and set up event listeners
        public SerialPortService(IHubContext<ArduinoHub> hubContext)
        {
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

            // Initialize the serial port with necessary configurations
            _serialPort = new SerialPort
            {
                PortName = "COM6", // Update with correct COM port
                BaudRate = 9600,
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.None,
                NewLine = "\n" // NewLine character for message termination
            };

            // Load message types from a JSON file
            messageTypes = MessageTypeLoader.LoadMessageTypesFromJson("TOMessage.json");

            // Subscribe to serial port data received event
            _serialPort.DataReceived += SerialPort_DataReceived;

            // Initialize dynamic buffer
            dynamicBuffer = new List<byte>();

            // Open the serial port
            _serialPort.Open();
        }

//---------------------------------------------------
// MAIN METHODS
//---------------------------------------------------

        // Method to send a message to the serial port
        public void SendData()
        {
            if (_serialPort.IsOpen)
            {
                // Example message data for encoding and sending
                Message message = new Message
                {
                    MessageData = "3", // Example data
                    TypeofData = messageTypes[0] // Example message type
                };

                // Encode message into WMBUS format
                byte[] encodedMessage = WMBUSProtocol.EncodeWMBUSMessage(0x01, message);

                // Write the encoded message to the serial port
                _serialPort.Write(encodedMessage, 0, encodedMessage.Length);
            }
            else
            {
                throw new InvalidOperationException("Serial port is not open.");
            }
        }

        // Event handler for receiving data from the serial port
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                byte[] tempBuffer = new byte[256];
                int bytesRead = _serialPort.Read(tempBuffer, 0, tempBuffer.Length);

                if (bytesRead > 0)
                {
                    // Add the received data to the dynamic buffer
                    dynamicBuffer.AddRange(tempBuffer.Take(bytesRead));

                    // Process any complete messages in the buffer
                    ProcessBuffer(dynamicBuffer);

                    // Ensure the buffer size remains within the limit
                    if (dynamicBuffer.Count > MaxBufferSize)
                    {
                        Console.WriteLine("Warning: Buffer exceeded maximum size. Clearing buffer to prevent memory issues.");
                        dynamicBuffer.Clear(); // Clear the buffer if it's too large
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading serial data: {ex.Message}");
            }
        }

        // Method to process the incoming buffer and decode complete messages
        private void ProcessBuffer(List<byte> buffer)
        {
            while (true)
            {
                // Look for the newline character as the message delimiter
                int newlineIndex = buffer.IndexOf((byte)'\n');
                if (newlineIndex == -1)
                {
                    break; // No complete message in the buffer yet
                }

                // Extract the message from the buffer
                byte[] messageBytes = buffer.GetRange(0, newlineIndex).ToArray();

                // Remove the processed message from the buffer
                buffer.RemoveRange(0, newlineIndex + 1);

                // Decode the WMBUS message
                ProtocolMessage message = WMBUSProtocol.DecodeWMBUSMessage(messageBytes, messageBytes.Length);

                if (message != null)
                {
                    // Calculate the voltage from the ADC value and send it to clients
                    float voltage = ByteArrayToInt(message.Data) * (5.0f / 1023);
                    Console.WriteLine(voltage.ToString("0.00"));
                    SendVoltageToClientsAsync(voltage);
                }
                else
                {
                    Console.WriteLine("Failed to decode WMBUS message.");
                }
            }
        }

        // Asynchronous method to send voltage data to SignalR clients
        private async void SendVoltageToClientsAsync(float voltage)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSignal", new { Signal = voltage });
                Console.WriteLine($"Voltage {voltage:0.00} sent to clients.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to SignalR clients: {ex.Message}");
            }
        }

//---------------------------------------------------
// HELPER FUNCTIONS
//---------------------------------------------------

        // Helper method to convert a byte array to an integer
        public static int ByteArrayToInt(byte[] byteArray)
        {
            int result = 0;
            foreach (byte b in byteArray)
            {
                result = (result << 8) | b;
            }
            return result;
        }

        // Dispose of the serial port when the service is no longer needed
        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close(); // Close the serial port
            }

            _serialPort.Dispose(); // Dispose of the serial port resource
        }
    }
}
