using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Amnhac.Models
{
    public class SongRanked
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string SongId { get; set; }
        public int Priority { get; set; }
        [BsonElement("ctg")]
        public string Category { get; set; }
        public DateTime ValidFrom { get; set; } = DateTime.Now;
        public DateTime ValidTo { get; set; } = DateTime.Now.AddMonths(1);
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        [BsonIgnore]
        public Song Song { get; set; }
    }
}
