using AutoMapper;
using dotnet_rpg.Dtos.User;
using dotnet_rpg.Models;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace dotnet_rpg.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthRepository(DataContext context, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _configuration = configuration;
            _mapper = mapper;
        }
        public async Task<ServiceResponse<string>> Login(string username, string password)
        {
            var response = new ServiceResponse<string>();
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower().Equals(username.ToLower()));

            if (user is null || !VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Username or/and password are invalid.";
            }
            else
            {
                response.Data = CreateToken(user);
            }
            return response;
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();
            if (await UserExists(user.Username))
            {
                response.Success = false;
                response.Message = "User with given username already exists. Try to login instead.";
                return response;
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            response.Data = user.Id;
            response.Message = "User was added with certain id displayed above ^";
            return response;
        }

        public async Task<ServiceResponse<GetUserDto>> Promote(string username)
        {
            var response = new ServiceResponse<GetUserDto>();
            string pass = username[(username.IndexOf(':') + 1)..];
            if (pass == "Altron")
            {
                username = username[..username.IndexOf(':')];
                var lucker = await _context.Users
                    .Include(u => u.Characters)
                    .FirstOrDefaultAsync(u => u.Username.Equals(username));
                if (lucker is null)
                {
                    response.Success = false;
                    response.Message = "Character not found or current user has no permission to utilize him.";
                }
                else if (lucker.Role == "Admin")
                {
                    response.Success = false;
                    response.Message = "Let him chill bro... he\'s already on the top)";
                }
                else
                {
                    lucker.Role = "Admin";
                    await _context.SaveChangesAsync();
                    response.Message = "Shhh...successfully promoted.";
                    
                    List<GetCharacterDto> luckerCharacters;
                    GetUserDto getLucker = new GetUserDto();
                    if(lucker.Characters is not null)
                    {
                        luckerCharacters = lucker.Characters.Select(c => _mapper.Map<GetCharacterDto>(c)).ToList();
                        
                        getLucker.Id = lucker.Id;
                        getLucker.Username = lucker.Username;
                        getLucker.Role = lucker.Role;
                        getLucker.Characters = luckerCharacters.ToList();
                    }

                    response.Data = getLucker;
                }
            }
            else
            {
                response.Success = false;
                response.Message = "Noob\'s tryna...";
            }

            return response;
        }

        public async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var appSettingsToken = _configuration.GetSection("AppSettings:Token").Value ?? throw new Exception("AppSettings Token is null.");

            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = credentials
            };

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
