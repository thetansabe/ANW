using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace Amnhac.Models
{
    public class Artist
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string AvatarImg { get; set; }
        public string BackgroundImg { get; set; }
        public string BelongTo { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Page { get; set; }
        public string Country { get; set; }
        [BsonIgnore]
        public string CountryName { get; set; }
        [JsonIgnore]
        public string Alphabet { get; set; }
        [JsonIgnore]
        public bool IsDeleted { get; set; } = false;
        public bool Deactivated { get; set; } = false;


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Artist)
                return Id == ((Artist)obj).Id;
            return false;
        }

        [BsonIgnoreIfNull]
        public double? TextMatchScore { get; set; }
    }
}
