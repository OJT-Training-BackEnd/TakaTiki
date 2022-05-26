﻿using MongoDB.Driver;
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

        public ServiceResponses<List<User>> Create(User user)
        {
            //var ServiceResponses = new ServiceResponses<List<User>>();
            throw new NotImplementedException();

        }

        public ServiceResponses<List<User>> Delete(string id)
        {
            throw new NotImplementedException();
        }

        public ServiceResponses<List<User>> Get()
        {
            var serviceResponses = new ServiceResponses<List<User>>();
            var dbUser = _user.Find(s => true).ToList();
            serviceResponses.Data = dbUser;
            return serviceResponses;
        }

        public ServiceResponses<User> Get(string id)
        {
            throw new NotImplementedException();
        }

        public ServiceResponses<List<User>> Update(string id, User user)
        {
            throw new NotImplementedException();
        }
    }
}
