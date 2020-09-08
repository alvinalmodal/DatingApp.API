using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        public IAuthRepository _authRepo { get; }
        public IConfiguration _config { get; }
        public IMapper _mapper { get; }
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthRepository authRepo, IConfiguration config, IMapper mapper,
            UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
            _config = config;
            _authRepo = authRepo;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            // validate request
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _authRepo.UserExists(userForRegisterDto.Username))
            {
                return BadRequest("Username already exists.");
            }

            var userToCreate = _mapper.Map<User>(userForRegisterDto);

            var createdUser = await _authRepo.Register(userToCreate, userForRegisterDto.Password);

            return Ok(createdUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLogin)
        {

            var user = await _userManager.FindByNameAsync(userForLogin.Username);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLogin.Password, false);

            if (result.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(user);

                return Ok(new
                {
                    token = GenerateJwtToken(user),
                    user = appUser
                });
            }

            return BadRequest("Failed to login.");

        }

        private string GenerateJwtToken(User user)
        {
            var claims = new[]
                        {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}
