using API.Attributes;
using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Cors;
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
        private IJwtService jwtService;

        public UserController(IUserMapper userMapper,
            IUserService userService,
            IPasswordService passwordService,
            IJwtService jwtService)
        {
            this.userMapper = userMapper;
            this.userService = userService;
            this.passwordService = passwordService;
            this.jwtService = jwtService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
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
                "User successfully registered",
                "success.user.register"
                ));
        }

        [HttpGet("email/verify/{emailVerificationCode}")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        [HttpPost("authenticate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<TokensDto>> Authenticate([FromBody] AuthenticateDto authenticateDto)
        {
            try
            {
                var user = await userService.Authenticate(authenticateDto.Username, authenticateDto.Password);
                string accessToken, refreshToken;
                jwtService.GenerateTokens(user, out accessToken, out refreshToken);
                return Ok(new TokensDto(accessToken, refreshToken));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ActionResultDto(
                    StatusCodes.Status401Unauthorized,
                    "Wrong username or password"
                    ));
            }
        }
    }
}
