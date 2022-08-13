using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    public class WeightedScore
    {
        // Fields
        public int UnweightedScore { get; set; }
        public ScoringRole Role { get; set; }
        public ScoreQuestion Question { get; set; }

        // Constructor
        public WeightedScore(int unweightedScore, ScoringRole role, ScoreQuestion question)
        {
            this.UnweightedScore = unweightedScore;
            this.Role = role;
            this.Question = question;
        }

        // Properties
        public double CalculatedScore { get {
            double questionPts = UnweightedScore/5.0 * Question.PossiblePoints;
            double pts = questionPts * Role.ScoringWeight;
            return pts;
        }}

        // Override
        public override string ToString() {
            return $"{CalculatedScore} = ({UnweightedScore}/5.0)*{Question.PossiblePoints}*{Role.ScoringWeight}";
        }


    }
}
