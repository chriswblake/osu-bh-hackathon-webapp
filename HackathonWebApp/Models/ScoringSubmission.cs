using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class ScoringSubmission
    {
        [BsonId]
        public ObjectId Id {get; set;}

        [BsonElement("team_id")]
        public string TeamId { get; set; }
        
        [BsonElement("user_id")]
        public string UserId { get; set; }

        [BsonElement("scores")]
        public Dictionary<string, int> Scores = new Dictionary<string, int>();
    }
}