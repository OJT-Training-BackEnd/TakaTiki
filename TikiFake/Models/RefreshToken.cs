using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
namespace TikiFake.Models
{
    public class RefreshToken
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("RTokenId")]
        public Guid RTokenId { get; set; }
        [BsonElement("UserId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        [BsonElement("Token")]
        public string Token { get; set; }
        [BsonElement("JwtId")]
        public string JwtId { get; set; }
        [BsonElement("isUsed")]
        public bool isUsed { get; set; }
        [BsonElement("isRevoked")]
        public bool isRevoked { get; set; }
        [BsonElement("IssuedAt")]
        public DateTime IssuedAt { get; set; }
        [BsonElement("ExpiredAt")]
        public DateTime ExpiredAt { get; set; }
    }
}
