using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Secuirty.Dtos;
using Secuirty.Services;
using System.Threading.Tasks;

namespace Secuirty.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        public TestController(IEmailService emailService)
        {
            _emailService = emailService;

        }
        [HttpGet("GetData")]
        [Authorize]
        public IActionResult GetData()
        {
            return Ok(new { data = "Create User" });
        }
        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage(EmailModel model)
        {
            await _emailService.SendAsync(model);
            return Ok();

        }

    }
}
