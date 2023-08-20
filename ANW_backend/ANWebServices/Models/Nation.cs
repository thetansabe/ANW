using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Amnhac.Models
{
    public class Nation
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Name { get; set; }


        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Nation)
                return Id == ((Nation)obj).Id;
            return false;
        }
    }
}
