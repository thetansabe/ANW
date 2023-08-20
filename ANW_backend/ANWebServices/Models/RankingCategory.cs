using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Amnhac.Models
{
    public class RankingCategory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        [BsonElement("lst")]
        public HashSet<string> TypeList { get; set; } = new HashSet<string>();

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is RankingCategory)
                return Id == ((RankingCategory)obj).Id;
            return false;
        }
        
    }
}
