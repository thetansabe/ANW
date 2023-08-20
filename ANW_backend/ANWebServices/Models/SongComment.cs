using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class SongComment
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string SongId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string UserId { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        [BsonIgnore]
        public string DisplayName { get; set; }
        public string Comment { get; set; }
        public List<SongComment> Reply { get; set; }
    }
}
