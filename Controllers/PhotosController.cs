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
using Microsoft.Extensions.Options;
using datingApp.Api.Helpers;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/v1/users/{userId}/[controller]")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;
        public readonly IOptions<CloudinarySettings> _cloudinarySettings;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository repository, IMapper mapper,
        IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _cloudinarySettings = cloudinaryConfig;
            _datingRepo = repository;
            _mapper = mapper;

            Account account = new Account(
                _cloudinarySettings.Value.CloudName,
                _cloudinarySettings.Value.ApiKey,
                _cloudinarySettings.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(account);
        }

        [HttpGet("{id}", Name = nameof(GetPhoto))]
        public async Task<IActionResult> GetPhoto([FromRoute] int id, [FromRoute] int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var photoFromRepo = await _datingRepo.GetPhotoOfUser(id, userId);
            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);
            return Ok(photo);
        }

        [HttpPost()]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm] PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _datingRepo.GetUser(userId);

            var file = photoForCreationDto.File;

            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);

                }
            }

            photoForCreationDto.Url = uploadResult.Url.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;
            var photo = _mapper.Map<Photo>(photoForCreationDto);
            if (!userFromRepo.Photos.Any(u => u.IsMain))
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if (await _datingRepo.SaveAll())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                //return CreatedAtRoute(nameof(GetPhoto), new { id = photo.Id}, photoToReturn);
                return Ok(photoToReturn);
            }

            return BadRequest("Could not add the photo.");
        }
        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _datingRepo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photoFromRepo = await _datingRepo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("This is already the main photo.");
            }

            var currentMainPhoto = await _datingRepo.GetMainPhotoOfUser(userId);
            currentMainPhoto.IsMain = false;

            photoFromRepo.IsMain = true;

            if (await _datingRepo.SaveAll())
            {
                return NoContent();
            }

            return BadRequest("Could not set photo to main.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _datingRepo.GetUser(userId);

            if (!user.Photos.Any(p => p.Id == id))
            {
                return Unauthorized();
            }

            var photoFromRepo = await _datingRepo.GetPhoto(id);

            if (photoFromRepo.IsMain)
            {
                return BadRequest("You cannot delete your main photo");
            }

            if (photoFromRepo.PublicId != null)
            {
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if (result.Result == "ok")
                {
                    _datingRepo.Delete(photoFromRepo);
                }
            }

            if (photoFromRepo.PublicId == null)
            {
                _datingRepo.Delete(photoFromRepo);
            }


            if (await _datingRepo.SaveAll())
            {
                return Ok();
            }

            return BadRequest("Failed to delete the photo");
        }

    }
}
