using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class UserLogs
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Text { get; set; }
        public string Mentor { get; set; }
        [BsonElement("s_ref")]
        public List<string> SongMentioned { get; set; }
        [BsonElement("v_ref")]
        public List<string> VideoMentioned { get; set; }
        public int Unread { get; set; } = 0;
        public int IsReply { get; set; } = 0;
        public DateTime TargetTime { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
