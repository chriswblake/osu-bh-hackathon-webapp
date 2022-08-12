using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class ScoreQuestion {
        // Tracking Fields
        [BsonId]
        public ObjectId Id {get; set;}

        // Properties
        [Required]
        public string Title {get; set; }
        [Required]
        public string Description {get; set; }
        [Required]
        public int PossiblePoints {get; set; }
        [Required]
        public string Group {get; set; }
        [Required]
        public Dictionary<string, AnswerOption> AnswerOptions {get; set; }
    }

    [BsonIgnoreExtraElements]
    public class AnswerOption
    {
        public int Score { get; set; }
        public string Description { get; set; }
    }
}