using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class UserHistory
    {
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public string Song { get; set; }
        public string Video { get; set; }
    }
}
