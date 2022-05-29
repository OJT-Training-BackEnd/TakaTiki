using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TikiFake.Models
{
    //[BsonIgnoreExtraElements]
    public class User
    {

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("role")]
        public List<Roles> Role { get; set; } = new List<Roles> { Roles.CUSTOMER };
        
        [BsonElement("active")]
        public bool Isactive { get; set; } = true;

    }
}
