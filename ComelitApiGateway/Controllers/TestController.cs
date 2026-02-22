using Microsoft.AspNetCore.Mvc;

namespace ComelitApiGateway.Controllers
{
    [ApiController]
    [Route("Test")]
    public class TestController(IConfiguration config) : BaseController(config)
    {
        /// <summary>
        /// Returns the current server timezone and local DateTime
        /// </summary>
        /// <returns></returns>
        [HttpGet("timezone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetTimezone()
        {
            var timeZone = TimeZoneInfo.Local;
            return Ok(new
            {
                timezone = timeZone.Id,
                displayName = timeZone.DisplayName,
                utcOffset = timeZone.BaseUtcOffset.ToString(),
                now = DateTime.Now
            });
        }
    }
}
