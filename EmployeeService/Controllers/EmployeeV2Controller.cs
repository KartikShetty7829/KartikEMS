using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EmployeeServiceDepartmentService.Controllers
{
    /// <summary>
    /// V2 Employee Controller. 
    /// Add your v2-specific endpoints here.
    /// Protected with ASP.NET Core native rate limiting.
    /// </summary>
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/Employee")]
    [ApiController]
    [EnableRateLimiting("EmployeeGeneralPolicy")]
    public class EmployeeV2Controller : ControllerBase
    {
        public EmployeeV2Controller()
        {
        }

        // Example V2 endpoint - you can replace or remove this
        [HttpGet("hello")]
        public IActionResult GetHelloV2()
        {
            return Ok(new { Message = "Hello from Employee API Version 2.0!" });
        }
    }
}
