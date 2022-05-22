﻿using API.Attributes;
using Application;
using Application.Dtos.Ingoing;
using Application.Dtos.Outgoing;
using Application.Exceptions;
using Application.Interfaces;
using Application.Settings;
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

        private readonly UserSettings userSettings;

        public UserController(IUserMapper userMapper,
            IUserService userService,
            IPasswordService passwordService,
            IJwtService jwtService,
            IConfiguration configuration)
        {
            this.userMapper = userMapper;
            this.userService = userService;
            this.passwordService = passwordService;
            this.jwtService = jwtService;

            userSettings = UserSettings.Get(configuration);
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

        [HttpPatch("verify-email")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<TokensDto>> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);

            if (user.IsEmailVerified)
            {
                return Conflict(new ActionResultDto(
                    StatusCodes.Status409Conflict,
                    "Email already verified"
                    ));
            }

            var updatedUser = await userService.VerifyEmailAsync(user, verifyEmailDto.EmailVerificationCode);

            if (updatedUser != null)
            {
                string newAccessToken, newRefreshToken;
                jwtService.GenerateTokens(updatedUser, out newAccessToken, out newRefreshToken);

                return Ok(new TokensDto(
                    newAccessToken,
                    newRefreshToken,
                    "success.user.verifyEmail"
                    ));
            } 
            else
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    "Invalid email verification code",
                    "error.user.verifyEmail.invalidCode"
                    ));
            }
        }

        [HttpPost("verify-email/reset-code")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public async Task<ActionResult<ActionResultDto>> ResetEmailVerificationCode()
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);

            if (user.IsEmailVerified)
            {
                return Conflict(new ActionResultDto(
                    StatusCodes.Status409Conflict,
                    "Email already verified"
                    ));
            }

            await userService.ResetEmailVerificationCodeAsync(user);

            return Ok(new ActionResultDto(
                StatusCodes.Status200OK,
                "New email verification code generated and sent through mail",
                "success.user.resetEmailVerificationCode"
                ));
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

        [HttpGet("validate-token")]
        [Authorize(false)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesErrorResponseType(typeof(ActionResultDto))]
        public ActionResult<ActionResultDto> ValidateToken()
        {
            return Ok(new ActionResultDto(
                StatusCodes.Status200OK,
                "Access token valid"
                ));
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

            if (!passwordService.VerifyPasswordHash(changePasswordDto.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    "Wrong current password",
                    "error.user.changePassword.wrongCurrentPassword"
                    ));
            }

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
        public async Task<ActionResult<TokensDto>> ChangeUserData([FromBody] ChangeUserDataDto changeUserDataDto)
        {
            var user = await userService.GetAuthorizedUserAsync(HttpContext);
            string? newAccessToken = null;
            string? newRefreshToken = null;

            var isUsernameChanged = !user.Username.Equals(changeUserDataDto.Username);
            var isEmailChanged = user.Email != changeUserDataDto.Email.ToLower();

            if (isUsernameChanged &&
                Utils.IsWithinTimeframe(user.LastUsernameChangeAt, userSettings.Username.AllowedChangeFrequencyDays, Utils.DateTimeUnit.DAYS))
            {
                return BadRequest(new ActionResultDto(
                    StatusCodes.Status400BadRequest,
                    $"Next allowed username change at {user.LastUsernameChangeAt.AddDays(userSettings.Username.AllowedChangeFrequencyDays)}"
                    ));
            }

            if (isUsernameChanged ||
                isEmailChanged ||
                changeUserDataDto.FirstName != user.FirstName ||
                changeUserDataDto.LastName != user.LastName)
            {
                var previousUsername = user.Username;
                try
                {
                    await userService.ValidateUserDetailsAsync(changeUserDataDto.Username,
                                                               changeUserDataDto.Email,
                                                               changeUserDataDto.FirstName,
                                                               changeUserDataDto.LastName,
                                                               user.Username.ToLower() != changeUserDataDto.Username.ToLower(),
                                                               isEmailChanged);
                }
                catch (DataValidationException ex)
                {
                    return BadRequest(new ActionResultDto(
                        StatusCodes.Status400BadRequest,
                        ex.Message,
                        ex.ErrorTranslationKey
                        ));
                }

                var updatedUser = await userService.ChangeUserDataAsync(user, changeUserDataDto);
                jwtService.RevokeTokens(previousUsername);
                jwtService.GenerateTokens(updatedUser, out newAccessToken, out newRefreshToken);
            }

            return Ok(new TokensDto(
                newAccessToken,
                newRefreshToken,
                isEmailChanged ? "success.user.data.emailChanged" : "success.user.data.emailNotChanged"
                ));
        }

        [HttpGet("settings")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<UserSettingsDto> GetUserSettings()
        {
            var userSettingsDto = userMapper.FromUserSettingsToUserSettingsDto(userSettings);
            return Ok(userSettingsDto);
        }
    }
}
