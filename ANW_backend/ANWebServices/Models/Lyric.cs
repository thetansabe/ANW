using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Amnhac.Models
{
    public class Lyric
    {
        
        public string UserId { get; set; }
        [BsonIgnore]
        public string DisplayName { get; set; }
        public string Text { get; set; }
        public DateTime Time { get; set; } = DateTime.Now;
        public int Approved { get; set; }
        public int Rating { get; set; }
    }
}
