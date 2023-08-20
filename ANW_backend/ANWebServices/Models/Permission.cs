using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.ComponentModel.DataAnnotations;

namespace Amnhac.Models
{
    public class Permission
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        [StringLength(20)]
        public string Color { get; set; }
        public long Price { get; set; }
        public int Category { get; set; }
    }
}
