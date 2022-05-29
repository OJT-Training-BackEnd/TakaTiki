using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TikiFake.Dtos.User
{
    public class UserRegisterDto
    {
        public string Username { get; set; } 
        public string Password { get; set; }
    }
}
