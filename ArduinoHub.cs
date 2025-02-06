using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace APIService
{
    /// <summary>
    /// SignalR hub for real-time communication between the server and connected clients.
    /// </summary>
    public class ArduinoHub : Hub
    {
        /// <summary>
        /// Sends data to all connected clients.
        /// </summary>
        /// <param name="data">The data to be broadcast.</param>
        public async Task SendData(string data)
        {
            await Clients.All.SendAsync("ReceiveData", data);
        }
    }
}
