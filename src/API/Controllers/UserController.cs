using API.Attributes;
using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Interfaces;
using Application.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace balance_tracker_backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    [Produces("application/json")]
    [ProducesErrorResponseType(typeof(ApiResponse<string>))]
    public class UserController : ControllerBase
    {
        private readonly IUserMapper userMapper;
        private readonly IUserService userService;
        private readonly IPasswordService passwordService;
        private readonly IJwtService jwtService;

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
        public async Task<ActionResult<ApiResponse<string>>> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            passwordService.CheckPasswordComplexity(userRegisterDto.Password, userRegisterDto.Username);
            var user = userMapper.FromUserRegisterDtoToUser(userRegisterDto);
            await userService.RegisterAsync(user);
            return Created("", ApiResponse<string>.Success("User successfully registered", "success.user.register"));
        }

        [HttpPatch("verify-email")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TokensDto>>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var newTokens = await userService.VerifyEmailAsync(user, verifyEmailDto.EmailVerificationCode);
            return Ok(ApiResponse<TokensDto>.Success(
                new TokensDto(newTokens),
                "success.user.verifyEmail"
                ));
        }

        [HttpPost("verify-email/reset-code")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<string>>> ResetEmailVerificationCode()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            await userService.ResetEmailVerificationCodeAsync(user);
            return Ok(ApiResponse<string>.Success(
                "New email verification code generated and sent through mail",
                "success.user.resetEmailVerificationCode"
                ));
        }

        [HttpPost("authenticate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<TokensDto>>> Authenticate([FromBody] AuthenticateDto authenticateDto)
        {
            var tokens = await userService.AuthenticateAsync(authenticateDto.UsernameOrEmail, authenticateDto.Password);
            return Ok(ApiResponse<TokensDto>.Success(new TokensDto(tokens)));
        }

        [HttpGet("validate-token")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<string>> ValidateToken()
        {
            return Ok(ApiResponse<string>.Success("Access token valid"));
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TokensDto>>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            var newTokens = await userService.RefreshTokenAsync(refreshTokenDto.RefreshToken);
            return Ok(ApiResponse<TokensDto>.Success(new TokensDto(newTokens)));
        }

        [HttpDelete("revoke-tokens")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<ApiResponse<string>> RevokeTokens()
        {
            jwtService.RevokeTokens(HttpContext.Items[Constants.AUTHORIZED_USERNAME].ToString());
            return Ok(ApiResponse<string>.Success("Tokens revoked"));
        }

        [HttpPost("password/reset/request")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<string>>> ResetPasswordRequest([FromBody] ResetPasswordRequestDto resetPasswordRequestDto)
        {
            await userService.GenerateResetPasswordCodeAsync(resetPasswordRequestDto.UsernameOrEmail);
            return Ok(ApiResponse<string>.Success(
                "If such account exists, email with reset password code has been sent",
                "success.user.resetPasswordRequest"
                ));
        }

        [HttpPatch("password/reset")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            await userService.ResetPasswordAsync(resetPasswordDto.ResetPasswordCode, resetPasswordDto.NewPassword);
            return Ok(ApiResponse<string>.Success(
                "Password successfully changed",
                "success.user.resetPassword"
                ));
        }

        [HttpPatch("password/change")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            await userService.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            return Ok(ApiResponse<string>.Success(
                "Password successfully changed",
                "success.user.changePassword"
                ));
        }

        [HttpGet("data")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<UserDataDto>>> GetUserData()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var userDataDto = userMapper.FromUserToUserDataDto(user);
            return Ok(ApiResponse<UserDataDto>.Success(userDataDto));
        }

        [HttpPatch("data")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TokensDto>> ChangeUserData([FromBody] ChangeUserDataDto changeUserDataDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            var isEmailChanged = !user.Email.ToLower().Equals(changeUserDataDto.Email.ToLower());
            var newTokens = await userService.ChangeUserDataAsync(
                user,
                changeUserDataDto.Username,
                changeUserDataDto.Email,
                changeUserDataDto.FirstName,
                changeUserDataDto.LastName
                );
            return Ok(ApiResponse<TokensDto>.Success(
                new TokensDto(newTokens),
                isEmailChanged ? "success.user.data.emailChanged" : "success.user.data.emailNotChanged"
                ));
        }
    }
}
