using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Controllers;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _service;
        public AccountController(DataContext context, ITokenService service)
        {
            _service = service;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> RegisterUser(RegisterDto registerDto)
        {
            if (await IsUserExits(registerDto.username)) return BadRequest("Username is taken");
            // HMACSHA512(); is provides hashing algorith 
            using var hmac = new HMACSHA512();

            var user = new AppUser()
            {
                UserName = registerDto.username,
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PasswordSalt = hmac.Key
            };

            _context.Add(user);
            await _context.SaveChangesAsync();
            return new UserDto
            {
                Username = user.UserName,
                Token = _service.CreateToken(user)
            };
        }

        private async Task<bool> IsUserExits(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                      .SingleOrDefaultAsync(x => x.UserName == loginDto.username);

            if (user == null) return Unauthorized("Invalid User");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var ComputeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.password));

            for (int i = 0; i < ComputeHash.Length; i++)
            {
                if (user.PasswordHash[i] != ComputeHash[i]) return Unauthorized("Invalid Password");
            }

             return new UserDto
            {
                Username = user.UserName,
                Token = _service.CreateToken(user)
            };
        }
    }
}