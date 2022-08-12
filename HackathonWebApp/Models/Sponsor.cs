﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class Sponsor
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("tier")]
        public string Tier { get; set; }

        [BsonElement("years_consecutive_support")]
        public int YearsConsecutiveSuppport { get; set; }
        
        [BsonElement("website_url")]
        public string WebsiteUrl { get; set; }
        
        [BsonElement("logo")]
        public string Logo { get; set; }
    }
}
