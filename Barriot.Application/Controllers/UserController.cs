using Microsoft.AspNetCore.Mvc;

namespace Barriot.Application.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            _logger.LogInformation("Received GET request with id {}", id);

            await Task.CompletedTask;
            
            return Ok(NoContent());
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync()
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            _logger.LogInformation("Received DELETE request with id {}", id);

            await Task.CompletedTask;

            return Ok(NoContent());
        }
    }
}
