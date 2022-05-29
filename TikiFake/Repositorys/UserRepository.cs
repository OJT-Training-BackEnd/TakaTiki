using AutoMapper;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TikiFake.DatabaseSettings;
using TikiFake.Dtos.User;
using TikiFake.Models;

namespace TikiFake.Repositorys
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _user;
        private readonly IMapper _mapper;
        public UserRepository(IUserstoreDatabaseSettings settings, IMapper mapper)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _user = database.GetCollection<User>(settings.UsersCollectionName);
            _mapper = mapper;
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

        public async Task<ServiceResponses<int>> Register(UserRegisterDto newUser)
        {
            var response = new ServiceResponses<int>();
            if (UserExists(newUser.Username))
            {
                response.Success = false;
                response.Message = "User already exists.";
                return response;
            }
            User user = _mapper.Map<User>(newUser);
            _user.InsertOne(user);
            response.Message = "Register sucessed";
            return response; 
        }

        public async Task<ServiceResponses<string>> login(string username, string password)
        {
            var response = new ServiceResponses<string>();

            var user = _user.Find(s => s.Username.Equals(username)).FirstOrDefault();
            var pass = _user.Find(x => x.Password.Equals(password)).FirstOrDefault();

            if (string.IsNullOrEmpty(username))
            {
                response.Success = false;
                response.Message = "Please enter your account.";
                return response;
            }

            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
                return response;
            }

            if (pass == null)
            {
                response.Success = false;
                response.Message = "Please enter your password.";
                return response;
            }
            response.Success = true;
            response.Message = "Login successed";
            response.Data = user.Id;
            return response;
        }

        public bool UserExists(string username)
        {
            var user = _user.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
                return false;
            return true;    
        }
    }
}
