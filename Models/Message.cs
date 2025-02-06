using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1.Models
{
    // Represents a message containing data and its corresponding type.
    public class Message
    {
        public string MessageData { get; set; }
        public MessageType TypeofData { get; set; }

    }
}
