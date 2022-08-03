using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    public class ScoringRole
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
        [Required]
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("score_question_ids")]
        public List<string> ScoreQuestionsIds { get; set; }
    }
}
