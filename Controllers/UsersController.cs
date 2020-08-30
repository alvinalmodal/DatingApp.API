using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Dtos;
using System.Collections.Generic;
using System;
using DatingApp.API.Helpers;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private IDatingRepository _datingRepo { get; }
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepo, IMapper mapper)
        {
            _mapper = mapper;
            _datingRepo = datingRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _datingRepo.GetUsers();
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _datingRepo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _datingRepo.GetUser(id);

            _mapper.Map(userForUpdateDto, userFromRepo);

            if (await _datingRepo.SaveAll())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} failed on save.");
        }

    }
}
