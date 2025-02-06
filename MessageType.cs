using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// Represents the type of a message, including its metadata.
    /// </summary>
    public class MessageType
    {
        public string Name { get; set; }
        public int MessageTypeID { get; set; }
        public string DataType { get; set; }
        public string Description { get; set; }
    }
}
