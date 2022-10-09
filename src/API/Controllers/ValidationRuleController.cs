using API.Attributes;
using Application.Dtos.Outgoing;
using Application.Mappers;
using Application.Settings;
using Application.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/validation-rule")]
    [ApiController]
    [Produces("application/json")]
    [ProducesErrorResponseType(typeof(ApiResponse<string>))]
    public class ValidationRuleController : ControllerBase
    {
        private readonly ValidationRulesSettings validationRulesSettings;

        public ValidationRuleController(IConfiguration configuration)
        {
            validationRulesSettings = ValidationRulesSettings.Get(configuration);
        }

        [HttpGet("all")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<ValidationRulesDto>> GetAll()
        {
            var validationRulesDto = ValidationRuleMapper.FromValidationRulesSettingsToValidationRulesDto(validationRulesSettings);
            return Ok(ApiResponse<ValidationRulesDto>.Success(validationRulesDto));
        }
    }
}
