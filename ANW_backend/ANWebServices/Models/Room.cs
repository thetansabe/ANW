using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public int Capacity { get; set; }
        public string CreatedBy { get; set; }
        public int AllowChat { get; set; }
        public int AllowVoice { get; set; }
        public int QueueMode { get; set; }
        public int Publicity { get; set; }
        public HashSet<string> JoinedUsers { get; set; } = new HashSet<string>();
        [JsonIgnore]
        public Queue<ChatLog> ChatLog { get; set; } = new Queue<ChatLog>();
    }
}
