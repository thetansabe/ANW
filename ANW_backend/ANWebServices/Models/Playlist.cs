using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class Playlist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }
        public List<string> Collection { get; set; } = new List<string>();
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public long View { get; set; }
        public int Public { get; set; }
        [JsonIgnore]
        public string Tags { get; set; }


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Playlist)
                return Id == ((Playlist)obj).Id;
            return false;
        }


        [BsonIgnoreIfNull]
        public double? TextMatchScore { get; set; }
    }
}
