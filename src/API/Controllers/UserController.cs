using Application.Dtos;
using Application.Exceptions;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
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
        [ProducesErrorResponseType(typeof(ActionErrorDto))]
        public async Task<ActionResult<ActionInfoDto>> Register([FromBody] UserRegisterDto userRegisterDto)
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
                var errorDto = new ActionErrorDto(
                    StatusCodes.Status400BadRequest,
                    ex.Message,
                    ex.ErrorTranslationKey
                    );
                return BadRequest(errorDto);
            }
            return Created("", new ActionInfoDto(
                true,
                "User successfully registered"
                // TODO: Add translation key
                ));
        }
    }
}
