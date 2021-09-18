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
        private readonly IPhotoService __photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService _photoService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            __photoService = _photoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers()
        {
            IEnumerable<MemberDto> users = await _userRepository.GetMembersAsync();

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

            ImageUploadResult result = await __photoService.AddPhotoAsync(file);

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

                return CreatedAtRoute("GetUser", new {username = username},photoDtoResult);
            }

            return BadRequest("Problem adding photo");
        }
    }
}