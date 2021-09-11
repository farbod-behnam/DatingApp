using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public UserRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        //TODO: edit to remove auto mapper dependency
        public async Task<MemberDto> GetMemberAsync(string username)
        {
            // return await _context.Users
            //     .Where(row => row.UserName == username)
            //     .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            //     .SingleOrDefaultAsync();

            ICollection<PhotoDto> photoDtos = await GetMemberPhotosAsync(username);
            string photoUrl = await GetMemberMainPhotoUrl(username);

            return await _context.Users
                .Where(row => row.UserName == username)
                .Select(user => new MemberDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    PhotoUrl = photoUrl,
                    Age = user.DateOfBirth.CalculateAge(),
                    KnownAs = user.KnownAs,
                    Created = user.Created,
                    LastActive = user.LastActive,
                    Gender = user.Gender,
                    Introduction = user.Introduction,
                    LookingFor = user.LookingFor,
                    Interests = user.Interests,
                    City = user.City,
                    Country = user.Country,
                    Photos = photoDtos
                }).SingleOrDefaultAsync();
        }

        public async Task<string> GetMemberMainPhotoUrl(string username)
        {
            ICollection<PhotoDto> photoDtos = await GetMemberPhotosAsync(username);

            foreach (var photo in photoDtos)
            {
                if (photo.IsMain) 
                    return photo.Url;
            }

            return null;
        }

        public async Task<ICollection<PhotoDto>> GetMemberPhotosAsync(string username)
        {
            // List<Photo> memberPhotos = await _context.Users
            //     .Where(row => row.UserName == username)
            //     .SelectMany(row => row.Photos)
            //     .ToListAsync();

            // List<PhotoDto> photoDtos = new List<PhotoDto>();

            // foreach (var photo in memberPhotos)
            // {
            //    photoDtos.Add(new PhotoDto
            //    {
            //        Id = photo.Id,
            //        Url = photo.Url,
            //        IsMain = photo.IsMain
            //    });
            // }

            // return photoDtos;

            var photoDtosNew = await _context.Users
                .Where(user => user.UserName == username)
                .SelectMany(user => user.Photos, (user, photo) => new PhotoDto 
                {
                    Id = photo.Id,
                    Url = photo.Url,
                    IsMain = photo.IsMain
                }).ToListAsync();

            return photoDtosNew;
          
        }

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
            return await _context.Users
                .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<AppUser> GetUserByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(p => p.Photos)
                .SingleOrDefaultAsync(row => row.UserName == username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
                .Include(p => p.Photos)
                .ToListAsync();
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Update(AppUser user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}