using Hook_Api.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Hook_Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class SumSubController : ControllerBase
    {
        private readonly ILogger<SumSubController> _logger;
        private readonly AppToken _appToken;
        public SumSubController(ILogger<SumSubController> logger, AppToken appToken)
        {
            _logger = logger;
            _appToken = appToken;
        }

        [HttpPost("createApplicant")]
        public async Task<ActionResult> CreateApplicant()
        {
            string externalUserId = $"USER_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
            string levelName = "basic-kyc-level";
            var result = await _appToken.CreateApplicant(externalUserId, levelName);
            return Ok(result);
        }

        [HttpGet("getApplicantStatus")]
        public async Task<ActionResult> GetApplicantStatus()
        {
            var result = await _appToken.GetApplicantStatus("6229cac9cbee0c000132ee96");
            return Ok(result);
        }

    }
}