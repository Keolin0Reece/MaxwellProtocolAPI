using ConsoleApp1.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class Command
    {
        public enum CommandType{
            String,
            Int,
            Double,
            Bool
        }

        public void decodeData(Message message)
        {
            switch (message.TypeofData.MessageTypeID)
            {
                case 0: //String
                { 
                    
                }
                break;

                case 1: //Int
                {
                }
                break;
                case 2: //Double
                {
                }
                break;
                case 3: //Bool
                {
                }
                break;
            }
        }
    }
}
