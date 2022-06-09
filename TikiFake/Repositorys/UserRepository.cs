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

        private readonly IMongoCollection<User> _user;
        private readonly IMongoCollection<RefreshToken> _refreshToken;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public UserRepository(IUserstoreDatabaseSettings settings,
                            IMapper mapper,
                            IConfiguration configuration)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _user = database.GetCollection<User>(settings.UsersCollectionName);
            _refreshToken = database.GetCollection<RefreshToken>(settings.RefreshTokensCollectionName);
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<ServiceResponses<List<User>>> Get()
        {
            var serviceResponses = new ServiceResponses<List<User>>();
            var dbUser = await _user.Find(s => true).ToListAsync();
            serviceResponses.Data = dbUser.ToList();
            return serviceResponses;
        }

        public async Task<ServiceResponses<User>> Get(string id)
        {
            var serviceResponses = new ServiceResponses<User>();
            var dbUser = await _user.Find(s => s.Id == id).FirstOrDefaultAsync();
            serviceResponses.Data = dbUser;

            return serviceResponses;
        }


        
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
            var token = CreateToken(user);
            if (user != null)
            {
                response.Success = true;
                response.Message = "Login successed";
                response.Data = token;
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
        private TokenModel CreateToken(User user)
        {

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role[0].ToString()),
                
                
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = System.DateTime.Now.AddSeconds(20),
                SigningCredentials = creds
            };

            // JWT security token handler.

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor); 

            var accessToken = tokenHandler.WriteToken(token);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                RTokenId = Guid.NewGuid(),
                JwtId = token.Id,
                UserId = user.Id,
                Token = refreshToken,
                isUsed = false,
                isRevoked = false,
                IssuedAt = DateTime.UtcNow,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10)
            };
            _refreshToken.InsertOne(refreshTokenEntity);
            return new TokenModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private string GenerateRefreshToken()
        {
            var random = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        public async Task<ServiceResponses<string>> RenewToken(TokenModel model)
        {
            var response = new ServiceResponses<string>();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value));
            var tokenValidateParam = new TokenValidationParameters
            {
                //ký vào token 
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                //tự cấp token 
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                ValidateLifetime = false //ko kiem tra token het han
            };
            try
            {
                //check 1 : AccessToken Valid format
                var tokenInVerification = tokenHandler.ValidateToken(model.AccessToken, tokenValidateParam, out var validatedToken);
                //check 2: check alg
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
                    if (!result)
                    {
                        response.Success = false;
                        response.Message = "Invalid token";
                        return response;
                    }
                }
                //check 3: check accessToken expire?
                var utcExpireDate = long.Parse(tokenInVerification.Claims.FirstOrDefault(
                    x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expireDate = ConvertUnixTimeToDateTime(utcExpireDate);
                if (expireDate > DateTime.UtcNow)
                {
                    response.Success = false;
                    response.Message = "Access token has not yet expired";
                    return response;
                }
                //check 4: check refreshtoken exist in DB
                var storedToken = _refreshToken.Find(x => x.Token == model.RefreshToken).FirstOrDefault();
                if (storedToken == null)
                {
                    response.Success = false;
                    response.Message = "Refresh token does not exist";
                    return response;
                }
                //check 5 : check refresh token is used/revoked?
                if (storedToken.isUsed)
                {
                    response.Success = false;
                    response.Message = "Refresh token has been used";
                    return response;
                }
                if (storedToken.isRevoked)
                {
                    response.Success = false;
                    response.Message = "Refresh token has been revoked";
                    return response;
                }
                //check 6: AccessToken id == JwtId in RefreshToke
                var jti = tokenInVerification.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
                if (storedToken.JwtId != jti)
                {
                    response.Success = false;
                    response.Message = "Token doesn't match";
                    return response;
                }
                //Update token is used
                storedToken.isRevoked = true;
                storedToken.isUsed = true;
                _refreshToken.ReplaceOne(n => n.Token == model.RefreshToken, storedToken);
                //create new token 
                var user = _user.Find(n => n.Id == storedToken.UserId).FirstOrDefault();
                var token = CreateToken(user);

                response.Success = true;
                response.Message = "Renew token success";
                response.Data = token;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = "Something went wrong";
                return response;
            }
        }

        private DateTime ConvertUnixTimeToDateTime(long utcExpireDate)
        {
            var dateTimeInterval = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTimeInterval.AddSeconds(utcExpireDate).ToUniversalTime();
            return dateTimeInterval;
        }
    }
}
