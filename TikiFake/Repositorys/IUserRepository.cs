using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TikiFake.Dtos.User;
using TikiFake.Models;

namespace TikiFake.Repositorys
{
    public interface IUserRepository
    {
        Task<ServiceResponses<List<User>>> Get();
        Task<ServiceResponses<User>> Get(string id);
        Task<ServiceResponses<List<User>>> Update (string id, User user);
        Task<ServiceResponses<List<User>>> Delete (string id);
        Task<ServiceResponses<int>> Register(UserRegisterDto user, string password);
        Task<ServiceResponses<string>> login(string username, string password);
        Task<ServiceResponses<string>> RenewToken(TokenModel model); 
        bool UserExists(string username);

    }
}
