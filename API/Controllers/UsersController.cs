using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var users = await _userRepository.GetMembersAsync(userParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            string username = User.GetUsername();
            AppUser user = await _userRepository.GetUserByUsernameAsync(username);

            // _mapper.Map(memberUpdateDto, user);

            user.Introduction = memberUpdateDto.Introduction;
            user.LookingFor = memberUpdateDto.LookingFor;
            user.Interests = memberUpdateDto.Interests;
            user.City = memberUpdateDto.City;
            user.Country = memberUpdateDto.Country;

            _userRepository.Update(user);

            if (await _userRepository.SaveAllAsync()) return NoContent(); // Ok("Profile updated successfully");

            return BadRequest("Failed to update user");

        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            string username = User.GetUsername();
            AppUser user = await _userRepository.GetUserByUsernameAsync(username);

            ImageUploadResult result = await _photoService.AddPhotoAsync(file);

            if (result.Error != null)
                return BadRequest(result.Error.Message);

            Photo photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };

            if (user.Photos.Count == 0)
                photo.IsMain = true;

            user.Photos.Add(photo);

            //TODO update table

            if (await _userRepository.SaveAllAsync())
            {
                // return _mapper.Map<PhotoDto>(photo);
                PhotoDto photoDtoResult = new PhotoDto 
                {
                    Url = photo.Url,
                    Id = photo.Id,
                    IsMain = photo.IsMain
                };

                return CreatedAtRoute("GetUser", new {username = username} ,photoDtoResult);
            }

            return BadRequest("Problem adding photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            string username = User.GetUsername();
            AppUser user = await _userRepository.GetUserByUsernameAsync(username);

            Photo photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo.IsMain)
                return BadRequest("This is already your main photo");

            Photo currentMainPhoto = user.Photos.FirstOrDefault(photo => photo.IsMain == true);

            // turn the already main photo to false
            if (currentMainPhoto != null)
                currentMainPhoto.IsMain = false;

            // turn the new photo to main
            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync()) 
                return NoContent();

            return BadRequest("Failed to set main photo");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            string username = User.GetUsername();
            AppUser user = await _userRepository.GetUserByUsernameAsync(username);

            Photo photo = user.Photos.FirstOrDefault(photo => photo.Id == photoId);

            if (photo == null)
                return NotFound();

            if (photo.IsMain)
                return BadRequest("You cannot delete your main photo");

            // if photo is stored in cloudinary too, remove it from there as well
            if (photo.PublicId != null)
            {
                DeletionResult result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null)
                    return BadRequest(result.Error.Message);
            }

            // remove it from database
            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync())
                return Ok();

            
            return BadRequest("Failed to delete the photo");

        }
    }
}