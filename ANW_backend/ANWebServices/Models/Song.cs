using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class Song
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public List<SongPath> Paths { get; set; }
        public List<Lyric> Lyrics { get; set; }
        public List<string> Artists { get; set; }
        public string SubType { get; set; }
        public string UploadedBy { get; set; }
        public string RefVideo { get; set; }
        public string Tags { get; set; } = "";
        public int Approved { get; set; }
        public int SelfPerformance { get; set; } = 0;
        public long View { get; set; }
        public long HitDownload { get; set; } = 0;
        public long HitLove { get; set; } = 0;
        public int Price { get; set; } = 0;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string Timer { get; set; } = "";
        public string Uri { get; set; } = "";
        public Boolean IsDeleted { get; set; } = false;

        [BsonIgnore]
        public HashSet<Artist> ArtistList { get; set; } = new HashSet<Artist>();
        [BsonIgnore]
        public int Duration;
        [BsonIgnore]
        public User UploadedUser;


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Song)
                return Id == ((Song)obj).Id;
            return false;
        }

        [BsonIgnoreIfNull]
        public double? TextMatchScore { get; set; }
    }
}
