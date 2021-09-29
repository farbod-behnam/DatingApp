using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
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


            // 
            // ICollection<PhotoDto> photoDtos = await GetMemberPhotosAsync(username);
            // string photoUrl = await GetMemberMainPhotoUrl(username);

            return await _context.Users
                .Where(row => row.UserName == username)
                .Select(user => new MemberDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    PhotoUrl = user.Photos.Where(photo => photo.IsMain == true).Select(photo => photo.Url).SingleOrDefault(),
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
                    Photos = user.Photos.Select(photo => new PhotoDto
                    {
                        Id = photo.Id,
                        Url = photo.Url,
                        IsMain = photo.IsMain
                    }).ToList()
                }).SingleOrDefaultAsync();
        }




        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
            // return await _context.Users
            //     .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            //     .ToListAsync();


            IQueryable<AppUser> query = _context.Users.AsQueryable();

            // filter out current user from member list
            query = query.Where(user => user.UserName != userParams.CurrentUsername);
            // filter opposite gender from member list
            query = query.Where(user => user.Gender == userParams.Gender);


            IQueryable<MemberDto> result = query
                .Select(user => new MemberDto
                {
                    Id = user.Id,
                    Username = user.UserName,
                    PhotoUrl = user.Photos.Where(photo => photo.IsMain == true).Select(photo => photo.Url).SingleOrDefault(),
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
                    Photos = user.Photos.Select(photo => new PhotoDto
                    {
                        Id = photo.Id,
                        Url = photo.Url,
                        IsMain = photo.IsMain
                    }).ToList()
                }).AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(result, userParams.PageNumber, userParams.PageSize);
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