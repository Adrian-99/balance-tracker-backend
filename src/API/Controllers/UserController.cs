using Application.Dtos;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace balance_tracker_backend.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserMapper userMapper;
        private IUserService userService;

        public UserController(IUserMapper userMapper, IUserService userService)
        {
            this.userMapper = userMapper;
            this.userService = userService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ActionInfoDto>> Register([FromBody] UserRegisterDto userRegisterDto)
        {
            var user = userMapper.FromUserRegisterDtoToUser(userRegisterDto);
            await userService.Register(user);
            return Created("", new ActionInfoDto { Success = true });
        }
    }
}
