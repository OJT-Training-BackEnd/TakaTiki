using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TikiFake.DatabaseSettings;
using TikiFake.Models;

namespace TikiFake.Repositorys
{
    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _user;
        public UserRepository(IUserstoreDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _user = database.GetCollection<User>(settings.UsersCollectionName);
        }

        public async Task<ServiceResponses<List<User>>> Create(User user)
        {
            //var ServiceResponses = new ServiceResponses<List<User>>();
            throw new NotImplementedException();

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
            var dbUser = await _user.Find(s => s.Id == id).FirstOrDefaultAsync();
            if (dbUser != null)
            {
                _user.ReplaceOne(s => s.Id == id, user);
                serviceResponses.Message = "Update Thanh Cong";
                var dbUserFull = await _user.Find(s => true).ToListAsync();
                serviceResponses.Data = dbUserFull.ToList();

            }
            else
            {
                serviceResponses.Message = $"Can not find the id: {id}";
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
    }
}
