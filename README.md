# Software Protocol Implementation Project

## Overview

This project is a backend service designed for handling serial communication using the WMBUS protocol. It leverages SignalR to send real-time data to connected frontend clients. The service listens for incoming messages from a device (e.g., an Arduino), decodes them, and transmits relevant data, such as voltage readings, to the clients.

## Project Structure

### APIService

- **Purpose**: Contains the core service logic for serial communication and SignalR.
  
  - **SerialPortService.cs**: Handles serial port communication, message encoding/decoding, and sending data to the frontend via SignalR.
  - **Protocol.cs**: Contains methods to encode and decode WMBUS messages.
  - **Worker.cs**: Runs as a background service, initializing the serial communication and processing incoming data.
  - **ArduinoHub.cs**: Defines the SignalR hub for frontend-client communication.
  - **Models**: Defines the `MessageType`, `MessageTypeConfig`, and `ProtocolMessage` classes for message management.
  - **MessageTypeLoader.cs**: Responsible for loading message types from a JSON file (`TOMessage.json`).

### Configuration Files

- **TOMessage.json**: This JSON file contains the configuration for different message types used in the protocol. It is loaded by the `MessageTypeLoader` class and utilized by the `SerialPortService` to process incoming data.

## Components

### Serial Communication

The project establishes a connection with a device through a serial port (`COM6` by default). Data is read in chunks and processed using the WMBUS protocol. The service decodes the incoming data and processes it accordingly. If the data is valid, it is then sent to the frontend through SignalR.

### SignalR Integration

SignalR is used to facilitate real-time communication between the backend service and the frontend clients. When the service decodes data (e.g., voltage readings from an ADC), it is transmitted to the clients using the `ReceiveSignal` event. 

### Protocol

The service encodes and decodes data using a protocol similar to the WMBUS protocol. This involves:
- **Encoding**: Converts messages into a byte array that follows the WMBUS message format.
- **Decoding**: Processes incoming byte arrays, extracts the data, and validates the checksum to ensure message integrity.

### Buffer Management

Incoming data is buffered before it is processed. The service dynamically handles this buffer, ensuring that the buffer doesn't grow too large by clearing it if it exceeds a set size (`MaxBufferSize`). This prevents memory issues from unbounded growth of the buffer.

## Data Flow

1. **Data Reception**: The service listens to the serial port and receives incoming data.
2. **Message Decoding**: The received byte array is decoded using the WMBUS protocol. If the checksum is valid, the message is processed.
3. **Voltage Calculation**: If the message contains a voltage reading, the service converts the byte data to a voltage value.
4. **SignalR Broadcasting**: The voltage data is sent to all connected frontend clients in real-time using SignalR.

## Key Classes

### SerialPortService

This class is the core of the service, responsible for:

- Managing the serial port connection.
- Reading incoming data from the port.
- Decoding and processing the data.
- Sending the processed data to SignalR clients.

### WMBUSProtocol

A static class responsible for:

- **Encoding** messages into the WMBUS format (with a message ID, length, payload, and checksum).
- **Decoding** WMBUS messages by extracting the message ID, length, type, data, and checksum.
- **Calculating** checksums to ensure data integrity.

### ArduinoHub

Defines a SignalR hub for communication between the backend and frontend. It has a method `SendData` to broadcast messages to all connected clients.

### ProtocolMessage

A class representing a decoded WMBUS message, containing:

- `MessageID`: The ID of the message.
- `MessageLength`: The length of the message.
- `MessageTypeID`: The type ID of the message.
- `Data`: The raw data of the message.
- `Checksum`: The checksum of the message.
- `IsValidChecksum`: A boolean indicating if the checksum is valid.

## Error Handling

The project includes robust error handling for:

- **Serial Port Issues**: If the serial port is unavailable or cannot be opened, an error is logged.
- **Message Decoding**: If there is an issue decoding the incoming data (e.g., an invalid checksum), the system logs the error and continues processing subsequent data.
