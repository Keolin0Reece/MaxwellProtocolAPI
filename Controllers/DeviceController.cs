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


    public class CommandRequest
    {
        public string Command { get; set; }
    }
}
