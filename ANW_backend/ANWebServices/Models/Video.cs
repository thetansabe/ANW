using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class Video
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        [BsonElement("ext")]
        public string Extension { get; set; }
        [BsonElement("by")]
        public string UploadBy { get; set; }
        [BsonElement("thumb")]
        public string Thumbnail { get; set; }
        [BsonElement("th_ext")]
        public string ThumbnailExtesion { get; set; }
        [BsonElement("cron")]
        public DateTime CreatedOn { get; set; }
        [BsonElement("upon")]
        public DateTime UpdatedOn { get; set; }
        public string Type { get; set; }
        [BsonElement("apr")]
        public int Approve { get; set; }
        public long View { get; set; }
        public string Tags { get; set; }
        [BsonElement("arts")]
        public HashSet<string> Artists { get; set; }

        [BsonIgnore]
        public List<Artist> ArtistList { get; set; } = new List<Artist>();
        [BsonIgnore]
        public User Uploader { get; set; }
    }
}
