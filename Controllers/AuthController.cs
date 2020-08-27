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

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        public IAuthRepository _authRepo { get; }
        public IConfiguration _config { get; }
        public IMapper _mapper { get; }

        public AuthController(IAuthRepository authRepo, IConfiguration config, IMapper mapper)
        {
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

            var userToCreate = new User
            {
                Username = userForRegisterDto.Username

            };

            var createdUser = await _authRepo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForRegisterDto userForRegisterDto)
        {

            var userFromRepo = await _authRepo.Login(userForRegisterDto.Username.ToLower(), userForRegisterDto.Password);

            if (userFromRepo == null)
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
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

            var user = _mapper.Map<UserForListDto>(userFromRepo);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token),
                user
            });
        }

    }
}
