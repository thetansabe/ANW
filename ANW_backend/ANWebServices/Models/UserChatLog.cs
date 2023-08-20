using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class UserChatLog
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public Queue<ChatContent> Content { get; set; } = new Queue<ChatContent>();
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
    }
}
