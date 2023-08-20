using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class ChatContent
    {
        public string Content { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
    }
}
