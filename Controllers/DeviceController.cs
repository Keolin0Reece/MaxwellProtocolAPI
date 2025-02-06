using Microsoft.AspNetCore.Mvc;

namespace APIService
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly ISerialPortService _serialPortService;

        public DeviceController(ISerialPortService serialPortService)
        {
            _serialPortService = serialPortService;
        }

        /// <summary>
        /// Sends a command to the connected device via serial communication.
        /// </summary>
        /// <param name="command">The command to be sent.</param>
        /// <returns>A response indicating success or failure.</returns>
        [HttpPost]
        public IActionResult SendCommand([FromBody] CommandRequest command)
        {
            if (string.IsNullOrWhiteSpace(command.Command))
            {
                return BadRequest(new { message = "Command cannot be null or empty." });
            }

            try
            {
                _serialPortService.SendData();
                return Ok(new { message = $"Command '{command.Command}' sent successfully.", success = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send command.", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Represents a command request sent to the API.
    /// </summary>
    public class CommandRequest
    {
        public string Command { get; set; }
    }
}
