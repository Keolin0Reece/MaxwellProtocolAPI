using ConsoleApp1;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

public static class MessageTypeLoader
{
    public static MessageType[] LoadMessageTypesFromJson(string jsonFilePath)
    {
        try
        {
            // Read the JSON file content
            string jsonContent = File.ReadAllText(jsonFilePath);

            // Deserialize JSON into MessageTypeConfig
            MessageTypeConfig config = JsonConvert.DeserializeObject<MessageTypeConfig>(jsonContent);

            // Convert the list of MessageType objects to an array
            return config.MessageTypes.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading JSON file: {ex.Message}");
            return new MessageType[0];  // Return an empty array if there's an error
        }
    }
}
