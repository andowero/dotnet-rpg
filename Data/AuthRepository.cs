using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace dotnet_rpg.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private static readonly byte[] _fakePasswordHash;

        static AuthRepository()
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                _fakePasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Prdy123"));
            }
        }

        public AuthRepository(DataContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<ServiceResponse<string>> LogIn(string username, string password)
        {
            var response = new ServiceResponse<string>();

            var user = await _context.Users.SingleOrDefaultAsync(user => user.Username.ToLower().Equals(username.ToLower()));

            if (user is null)
            {
                FakePasswordHashEqual();
            }
            else if (PasswordHashEqual(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Data = CreateToken(user);
                response.Success = true;
                return response;
            }

            response.Data = "";
            response.Success = false;
            response.Message = "Wrong username or password";
            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();
            if (await UserExists(user.Username))
            {
                response.Success = false;
                response.Message = $"User {user.Username} already exists!";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            response.Data = user.Id;
            return response;
        }

        public async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(user => user.Username.ToLower() == username.ToLower());
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool PasswordHashEqual(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                return passwordHash.SequenceEqual(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password)));
            }

        }
        private void FakePasswordHashEqual()
        {
            using(var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                _fakePasswordHash.SequenceEqual(hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Prdy123")));
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                //simplified - no need to specify "Claim" for new
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username)
            };

            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value;
            if (appSettingsToken is null)
            {
                throw new Exception(@"Specify ""Appsettings"":""Token"" value!");
            }
            SymmetricSecurityKey key = new(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));
            SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            JwtSecurityTokenHandler tokenHandler = new();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }

}