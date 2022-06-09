using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TikiFake.DatabaseSettings;
using TikiFake.Dtos.User;
using TikiFake.Models;

namespace TikiFake.Repositorys
{
    public class UserRepository : IUserRepository
    {
        

        
        public async Task<ServiceResponses<List<User>>> Update(string id, User user)
        {
            var serviceResponses = new ServiceResponses<List<User>>();

            if (id == null)
            {
                serviceResponses.Message = "Id cannot null";
                serviceResponses.Success = false;
                return serviceResponses;
            }
            var updateUser = await _user.Find(s => s.Id == id).FirstOrDefaultAsync();

            if (updateUser != null)
            {
                updateUser.Isactive = false;
                _user.ReplaceOne(s => s.Id == id, user);
                var dbUser = await _user.Find(s => true).ToListAsync();
                serviceResponses.Message = "Update Successed";
                serviceResponses.Success = true;
                serviceResponses.Data = dbUser.ToList();
            }
            else
            {
                serviceResponses.Message = $"Cannot find user with the id : {id}";
                serviceResponses.Success = false;
            }

            return serviceResponses;
        }

        public async Task<ServiceResponses<List<User>>> Delete(string id)
        {
            var serviceResponses = new ServiceResponses<List<User>>();

            if (id == null)
            {
                serviceResponses.Message = "Id cannot null";
                serviceResponses.Success = false;
                return serviceResponses;
            }

            var deleteUser = await _user.Find(s => s.Id == id).FirstOrDefaultAsync();

            if (deleteUser != null)
            {
                deleteUser.Isactive = false;
                _user.ReplaceOne(s => s.Id == id, deleteUser);
                var dbUser = await _user.Find(s => true).ToListAsync();
                serviceResponses.Message = "Delete Successed";
                serviceResponses.Data = dbUser.ToList();
            }
            else
            {
                serviceResponses.Message = $"Cannot find user with the id : {id}";
                serviceResponses.Success = false;
            }

            return serviceResponses;
        }

        public async Task<ServiceResponses<int>> Register(UserRegisterDto newUser, string password)
        {
            var response = new ServiceResponses<int>();
            if (UserExists(newUser.Username))
            {
                response.Success = false;
                response.Message = "User already exists.";
                return response;
            }

            var passwordHash = getHash(password);
            newUser.Password = passwordHash;

            User user = _mapper.Map<User>(newUser);
            _user.InsertOne(user);
            response.Message = "Register sucessed";
            return response; 
        }

        public async Task<ServiceResponses<string>> login(string username, string password)
        {
            var response = new ServiceResponses<string>();

            if (string.IsNullOrEmpty(username))
            {
                response.Success = false;
                response.Message = "Please enter your username.";
                return response;
            }
            if (string.IsNullOrEmpty(password))
            {
                response.Success = false;
                response.Message = "Please enter your password.";
                return response;
            }

            var user = _user.Find(s => s.Username.Equals(username)).FirstOrDefault();

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            var password_login = getHash(password);
            if (password_login != user.Password)
            {
                response.Message = "Wrong password";
                return response;
            }

            if (user != null)
            {
                response.Success = true;
                response.Message = "Login successed";
                response.Data = CreateToken(user);
                return response;
            }
            return response;

        }

        public bool UserExists(string username)
        {
            var user = _user.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
                return false;
            return true;    
        }       
        
        private string getHash(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                // Send a sample text to hash.  
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                // Get the hashed string.  
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        //JWT
        private string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            // JWT security token handler.

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor); 


            return tokenHandler.WriteToken(token);
        }
    }
}
