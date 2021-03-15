using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SingularityChat.WebAPI.Models;
using SingularityChat.WebAPI.Services;

namespace SingularityChat.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        UserManager<User> _userManager;
        SignInManager<User> _signinManager;
        JwtTokenService _jwtTokenService;
        ChatRoomServices _chatRoomServices;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, JwtTokenService jwtTokenService, ChatRoomServices chatRoomServices)
        {
            _userManager = userManager;
            _signinManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _chatRoomServices = chatRoomServices;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] LoginCredential loginCredential)
        {
            if(!string.IsNullOrEmpty(loginCredential.Username))
            {
                var existUser = await _userManager.FindByNameAsync(loginCredential.Username);
                if(existUser == null)
                {
                    User user = new User()
                    {
                        UserName = loginCredential.Username,
                    };

                    var createResult = await _userManager.CreateAsync(user, loginCredential.Password);
                    if (createResult.Succeeded)
                        return CreatedAtRoute("login", new { }, loginCredential);
                }
            }

            return BadRequest();
        }

        [HttpPost("login", Name = "login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginCredential loginCredential)
        {
            var signInResult = await _signinManager.PasswordSignInAsync(loginCredential.Username, loginCredential.Password, false, false);
            if(signInResult.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(loginCredential.Username);
                return Ok(new { access_token = _jwtTokenService.GenerateToken(user) });
            }

            return Forbid();
        }

        [HttpPost("ws")]
        public async Task<IActionResult> AuthenticateWS([FromQuery] string connection_id)
        {
            var id = User.Claims.FirstOrDefault(x => x.Type == "UserId")?.Value;
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                _chatRoomServices.AddConnection(connection_id, user);
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }
    } 
}