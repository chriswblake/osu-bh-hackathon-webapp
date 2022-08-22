using System;
using Xunit;
using MongoDB.Bson;
using HackathonWebApp.Models;


namespace HackathonWebAppTests
{
    public class WeightedScoreTests
    {
        /// <summary>
        /// Description: Verifies that the calculated score is correct.
        /// </summary> 
        [Fact]
        public void Test_CalculatedScore()
        {
            // Define
            int unweightedScore = 3;
            ScoringRole role = new ScoringRole() { ScoringWeight = 0.6 };
            ScoreQuestion question = new ScoreQuestion() { PossiblePoints = 30 };
            
            // Process
            var w = new WeightedScore(unweightedScore, role, question);
            var calculateScore = w.CalculatedScore;

            // Assert
            Assert.Equal((3/5.0)*30*0.6, calculateScore);
        }
    }
}
