using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TikiFake.Dtos.User;
using TikiFake.Models;
using TikiFake.Repositorys;

namespace TikiFake.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Get all
        [HttpGet("GetAll")]
        public async Task<ActionResult<ServiceResponses<List<User>>>> Get()
        {
            string id = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
            return Ok(await _userRepository.Get(id));
        }

        // Get by Id
        [HttpGet("GetById")]
        public async Task<ActionResult<ServiceResponses<User>>> Get(string id)
        {
            return Ok(await _userRepository.Get(id));
        }

        // Update user status
        [HttpPut("UpdateUserStatus")]
        public async Task<ActionResult<ServiceResponses<List<User>>>> Delete(string id)
        {
            var user = _userRepository.Get(id);

            return Ok(await _userRepository.Delete(id));
        }

        // Register
        [AllowAnonymous] // All method is secured but this
        [HttpPost("Register")]
        public async Task<ActionResult<ServiceResponses<List<UserRegisterDto>>>> Register (UserRegisterDto user)

        {
            var response = await _userRepository.Register(user, user.Password);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        // Update user
        [HttpPut("{id:length(24)}")]
        public async Task<ActionResult<ServiceResponses<List<User>>>> UpdateUser(string id, User user)
        {
            var tempUser = _userRepository.Get(id);

            if (tempUser == null)
                return NotFound();
            return Ok(await _userRepository.Update(id, user));
        }

        // Login
        [AllowAnonymous] // All method is secured but this
        [HttpPost("Login")]
        public async Task<ActionResult<ServiceResponses<List<UserRegisterDto>>>> Login(string userName, string password)

        {
            var response = await _userRepository.login(userName, password);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
    }
}
