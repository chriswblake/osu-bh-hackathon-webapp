using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class ScoringRole
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
        [Required]
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("description")]
        public string Description { get; set; }

        [BsonElement("scoring_weight")]
        [Range(0.0, 1.0)]
        public double ScoringWeight { get; set; }

        [BsonElement("score_question_ids")]
        public List<string> ScoreQuestionsIds { get; set; }

        // Methods
        public override string ToString()
        {
            return $"{Name} ({ScoringWeight})";
        }
    }
}
