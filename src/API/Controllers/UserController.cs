using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace balance_tracker_backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private IUserMapper userMapper;
        private IUserService userService;
        private IPasswordService passwordService;

        public UserController(IUserMapper userMapper, IUserService userService, IPasswordService passwordService)
        {
            this.userMapper = userMapper;
            this.userService = userService;
            this.passwordService = passwordService;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            try
            {
                await userService.ValidateUsernameAndEmail(userRegisterDto.Username, userRegisterDto.Email);
                passwordService.CheckPasswordComplexity(userRegisterDto.Password, userRegisterDto.Username);
                var user = userMapper.FromUserRegisterDtoToUser(userRegisterDto);
                await userService.Register(user);
            }
            catch (DataValidationException ex)
            {
                var errorDto = new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.ErrorTranslationKey
                    );
                return BadRequest(errorDto);
            }
            return Created("", new ActionResultDto(
                StatusCodes.Status201Created,
                "User successfully registered"
                ));
        }

        [HttpGet("email/verify/{emailVerificationCode}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> VerifyEmail([FromRoute] string emailVerificationCode)
        {
            if (await userService.VerifyEmail(emailVerificationCode))
            {
                return Ok(new ActionResultDto(
                    StatusCodes.Status200OK,
                    "Email verification successfull"
                    ));
            } 
            else
            {
                return NotFound(new ActionResultDto(
                    StatusCodes.Status404NotFound,
                    "Invalid email verification code"
                    // TODO: Add translation key
                    ));
            }
        }
    }
}
