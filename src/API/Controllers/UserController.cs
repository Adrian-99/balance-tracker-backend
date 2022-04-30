using API.Attributes;
using Application;
using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
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
                await userService.ValidateUserDetailsAsync(userRegisterDto.Username,
                                                           userRegisterDto.Email,
                                                           userRegisterDto.FirstName,
                                                           userRegisterDto.LastName);
                passwordService.CheckPasswordComplexity(userRegisterDto.Password, userRegisterDto.Username);
                var user = userMapper.FromUserRegisterDtoToUser(userRegisterDto);
                await userService.RegisterAsync(user);
            }
            catch (DataValidationException ex)
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.ErrorTranslationKey
                    ));
            }
            return Created("", new ActionResultDto(
                StatusCodes.Status201Created,
                "User successfully registered",
                "success.user.register"
                ));
        }

        [HttpPatch("email/verify")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            if (await userService.VerifyEmailAsync(user, verifyEmailDto.EmailVerificationCode))
            {
                return Ok(new ActionResultDto(
                    StatusCodes.Status200OK,
                    "Email verification successfull"
                    ));
            } 
            else
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
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
            var user = await userService.AuthenticateAsync(authenticateDto.UsernameOrEmail, authenticateDto.Password);

            if (user == null)
            {
                return Unauthorized(new ActionResultDto(
                    StatusCodes.Status401Unauthorized,
                    "Wrong username or password",
                    "error.user.authenticate.wrongCredentials"
                    ));
            }

            string accessToken, refreshToken;
            jwtService.GenerateTokens(user, out accessToken, out refreshToken);
            return Ok(new TokensDto(accessToken, refreshToken));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<TokensDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var username = jwtService.ValidateRefreshToken(refreshTokenDto.RefreshToken);

            if (username == null)
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    "Invalid refresh token"
                    ));
            }

            var user = await userService.GetUserByUsernameIgnoreCaseAsync(username);
            string accessToken, refreshToken;
            jwtService.GenerateTokens(user, out accessToken, out refreshToken);
            return Ok(new TokensDto(accessToken, refreshToken));
        }

        [HttpDelete("revoke-tokens")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public ActionResult<ActionResultDto> RevokeTokens()
        {
            jwtService.RevokeTokens(HttpContext.Items[Constants.AUTHORIZED_USERNAME].ToString());
            return Ok(new ActionResultDto(StatusCodes.Status200OK,
                "Tokens revoked"
                ));
        }

        [HttpPost("password/reset/request")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ActionResultDto>> ResetPasswordRequest([FromBody] ResetPasswordRequestDto resetPasswordRequestDto)
        {
            await userService.GenerateResetPasswordCodeAsync(resetPasswordRequestDto.UsernameOrEmail);
            return Ok(new ActionResultDto(StatusCodes.Status200OK,
                "If such account exists, email with reset password code has been sent",
                "success.user.resetPasswordRequest"
                ));
        }

        [HttpPatch("password/reset")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            var user = await userService.ValidateResetPasswordCodeAsync(resetPasswordDto.ResetPasswordCode);
            if (user == null)
            {
                return BadRequest(new ActionResultDto(StatusCodes.Status400BadRequest,
                    "Invalid reset password code",
                    "error.user.resetPassword.invalidCode"
                    ));
            }

            try
            {
                passwordService.CheckPasswordComplexity(resetPasswordDto.NewPassword, user);
            }
            catch (DataValidationException ex)
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.ErrorTranslationKey
                    ));
            }

            await userService.ChangePasswordAsync(user, resetPasswordDto.NewPassword);

            return Ok(new ActionResultDto(
                StatusCodes.Status200OK,
                "Password successfully changed",
                "success.user.resetPassword"
                ));
        }

        [HttpPatch("password/change")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);

            try
            {
                passwordService.CheckPasswordComplexity(changePasswordDto.NewPassword, user);
            }
            catch (DataValidationException ex)
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.ErrorTranslationKey
                    ));
            }

            await userService.ChangePasswordAsync(user, changePasswordDto.NewPassword);

            return Ok(new ActionResultDto(
                StatusCodes.Status200OK,
                "Password successfully changed",
                "success.user.changePassword"
                ));
        }

        [HttpGet("data")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<UserDataDto>> GetUserData()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var userDataDto = userMapper.FromUserToUserDataDto(user);
            return Ok(userDataDto);
        }

        [HttpPatch("data")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> ChangeUserData([FromBody] ChangeUserDataDto changeUserDataDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            try
            {
                await userService.ValidateUserDetailsAsync(changeUserDataDto.Username,
                                                           changeUserDataDto.Email,
                                                           changeUserDataDto.FirstName,
                                                           changeUserDataDto.LastName,
                                                           changeUserDataDto.Username.ToLower() != user.Username.ToLower(),
                                                           changeUserDataDto.Email.ToLower() != user.Email);
            }
            catch (DataValidationException ex)
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.ErrorTranslationKey
                    ));
            }

            var isEmailChanged = user.Email != changeUserDataDto.Email.ToLower();
            await userService.ChangeUserDataAsync(user, changeUserDataDto);

            return Ok(new ActionResultDto(
                StatusCodes.Status200OK,
                "User data updated successfully",
                isEmailChanged ? "success.user.data.emailChanged" : "success.user.data.emailNotChanged"
                ));
        }
    }
}
