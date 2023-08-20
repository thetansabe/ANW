using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Amnhac.Models
{
    public class SongLogs
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string SongId { get; set; }
        [BsonElement("ctg")]
        public string SongCategory { get; set; }
        public long ViewCount { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
