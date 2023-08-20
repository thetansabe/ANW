using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class Album
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public string AvatarImg { get; set; }
        public string BackgroundImg { get; set; }
        [JsonIgnore]
        public HashSet<string> Collection { get; set; }
        [JsonIgnore]
        public HashSet<string> Artists { get; set; }
        public string CreatedBy { get; set; }
        public int Approved { get; set; }
        public long View { get; set; }
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string Tags { get; set; }
        public Boolean IsDeleted { get; set; }


        [BsonIgnore]
        public HashSet<Artist> ArtistList { get; set; } = new HashSet<Artist>();
        [BsonIgnore]
        public HashSet<Song> SongList { get; set; } = new HashSet<Song>();

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Album)
                return Id == ((Album)obj).Id;
            return false;
        }

        [BsonIgnoreIfNull]
        public double? TextMatchScore { get; set; }
    }
}
