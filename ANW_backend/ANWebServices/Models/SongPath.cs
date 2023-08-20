using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Amnhac.Models
{
    public class SongPath
    {
        public int Prefix { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
}
