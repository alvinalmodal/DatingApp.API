using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Dtos;
using System;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [ApiController]
    [Route("api/v1/users/{userId}/[controller]")]
    public class MessagesController : ControllerBase
    {
        private IDatingRepository _datingRepo { get; }
        private readonly IMapper _mapper;

        public MessagesController(IDatingRepository datingRepo, IMapper mapper)
        {
            _mapper = mapper;
            _datingRepo = datingRepo;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepo.GetMessage(id);

            if (messageFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messageFromRepo);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }
            Console.WriteLine(messageForCreationDto);

            messageForCreationDto.SenderId = userId;

            var recipient = await _datingRepo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
            {
                return BadRequest("Could not find user.");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);

            _datingRepo.Add(message);

            var messageToReturn = _mapper.Map<MessageForCreationDto>(message);

            if (await _datingRepo.SaveAll())
            {
                return Ok(messageToReturn);
            }

            return BadRequest("Failed to create message.");
        }

    }
}
