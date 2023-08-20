using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string DisplayName { get; set; }
        public string FullName { get; set; }
        public string Idcard { get; set; }
        public DateTime DateOfBirth { get; set; } 
        public string Desc { get; set; }
        public string AvatarImg { get; set; } = "empty-user.png";
        public string BackgroundImg { get; set; } = "bg.jpg";
        public int MaxChannel { get; set; } =0; 
        public int Rank { get; set; } = 0;
        [JsonIgnore]
        public string Permission { get; set; }
        public List<string> SongFavorite { get; set; } = new List<string>();
        public List<string> VideoFavorite { get; set; } = new List<string>();
        [JsonIgnore]
        public List<UserLogs> Logs { get; set; } = new List<UserLogs>();
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        [JsonIgnore]
        public DateTime CreatedOn { get; set; } = DateTime.Now;

        [JsonIgnore]
        public List<UserHistory> History { get; set; } = new List<UserHistory>();
        [JsonIgnore]
        public Queue<UserChatLog> ChatLog { get; set; } = new Queue<UserChatLog>();

        /// <summary>
        /// Clear user's sentitive data before responsing.
        /// </summary>
        public void ClearSentitiveData()
        {
            this.Password = "";
            this.Logs = null;
            this.SongFavorite = null;
            this.VideoFavorite = null;
            this.DateOfBirth = DateTime.Now;
            this.Permission = "";
            this.Email = "";
            this.Phone = "";
        }
    }
}
