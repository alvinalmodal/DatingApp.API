using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Dtos;
using System.Collections.Generic;
using System;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using System.Text.Json;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
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
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _datingRepo.GetUser(currentUserId);
            userParams.UserId = userFromRepo.Id;
            var users = await _datingRepo.GetUsers(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);
            Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
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

        [HttpPost("{id}/like/{recipientId}")]
        public async Task<IActionResult> LikeUser(int id, int recipientId)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _datingRepo.GetLike(id, recipientId);

            if (like != null)
            {
                return BadRequest("You already like this user");
            }

            if (await _datingRepo.GetUser(recipientId) == null)
            {
                return NotFound();
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recipientId
            };

            _datingRepo.Add<Like>(like);
            if (await _datingRepo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to like user.");
        }

    }
}
