using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class Team
    {
        [BsonId]
        public ObjectId Id {get; set;}

        [Required]
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonIgnore]
        public HackathonEvent ReferenceEvent {get; set;}

        // Properties
        [BsonIgnore]
        public Dictionary<Guid, EventApplication> EventApplications {
            get {
                if (ReferenceEvent == null) 
                    return new Dictionary<Guid, EventApplication>();
                
                // Uses mapping of event applications to teams to build dictionary.
                var eventApplicationIds = this.ReferenceEvent.EventAppTeams.Where(kvp=> kvp.Value == this.Id.ToString()).Select(kvp => kvp.Key);
                var eventApplications = eventApplicationIds.Select(p => this.ReferenceEvent.EventApplications[p]).ToDictionary(p=> p.UserId, p=> p);
                return eventApplications;
            }
        }

        // Fields that would need aggregated from other data
        [BsonIgnore]
        public Dictionary<Guid, ApplicationUser> TeamMembers { get; set; } = new Dictionary<Guid, ApplicationUser>();

        
        #region Experience 
        [BsonIgnore]
        public double HackathonExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.HackathonExperience);
        }}
        [BsonIgnore]
        public double CodingExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.CodingExperience);
        }}
        [BsonIgnore]
        public double CommunicationExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.CommunicationExperience);
        }}
        [BsonIgnore]
        public double OrganizationExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.OrganizationExperience);
        }}
        [BsonIgnore]
        public double DocumentationExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.DocumentationExperience);
        }}
        [BsonIgnore]
        public double BusinessExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.BusinessExperience);
        }}
        [BsonIgnore]
        public double CreativityExperience { get {
            return this.EventApplications.Values.Sum(ea => ea.CreativityExperience);
        }}

        [BsonIgnore]
        public double AvgHackathonExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.HackathonExperience);
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgCodingExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.CodingExperience);    
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgCommunicationExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.CommunicationExperience); 
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgOrganizationExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.OrganizationExperience);  
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgDocumentationExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.DocumentationExperience); 
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgBusinessExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.BusinessExperience);  
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgCreativityExperience { get {
            if (this.EventApplications.Count() > 0)
                return this.EventApplications.Values.Average(ea => ea.CreativityExperience);    
            else
                return 0;
        }}
        #endregion
    
        #region Scoring
        [Required]
        [BsonElement("scoring_submissions")]
        public Dictionary<string, ScoringSubmission> ScoringSubmissions {get; set; } = new Dictionary<string, ScoringSubmission>();
        [BsonIgnore]
        public Dictionary<string, int> CountScoresByQuestionId { get {

            // Collapse to just the questions
            var allQuestions = this.ScoringSubmissions.Values.Select(p=> p.Scores).SelectMany(x => x).ToList();
            
            // Count by question ID
            var counts = allQuestions.GroupBy(p=> p.Key).ToDictionary(p=> p.Key, p=> p.Count());

            return counts;
        }}
        [BsonIgnore]
        public Dictionary<string, double> AvgUnweightedScoresByQuestionId { get {
            var counts = new Dictionary<string, int>();
            var sums = new Dictionary<string, int>();
            var avgs = new Dictionary<string, double>();

            foreach(var scoreSubmission in this.ScoringSubmissions.Values)
            {
                foreach(var kvp in scoreSubmission.Scores) {
                    string questionId = kvp.Key;
                    int score = kvp.Value;
                    if (!sums.Keys.Contains(questionId))
                    {
                        counts[questionId] = 0;
                        sums[questionId] = 0;
                    }
                    // Track count and sum
                    counts[questionId] += 1;
                    sums[questionId] += score;
                }
            }

            // Calculate average
            foreach (var kvp in sums)
            {
                avgs[kvp.Key] = sums[kvp.Key] / (double) counts [kvp.Key];
            }

            return avgs;
        }}
        [BsonIgnore]
        public Dictionary<string, double> AvgWeightedScoresByQuestionId { get {
            // Return unweighted scores if no reference event
            if (this.ReferenceEvent == null)
            {
                return this.AvgUnweightedScoresByQuestionId;
            }

            // Get unweighted averages
            var unweightedScores = this.AvgUnweightedScoresByQuestionId;

            // Multiply by weight of each question
            var questions =  this.ReferenceEvent.ScoringQuestions;
            Dictionary<string, double> weightedScores = unweightedScores.ToDictionary(kvp=> kvp.Key, kvp=> (kvp.Value/5.0)*questions[kvp.Key].PossiblePoints);

            return weightedScores;
        }}
        [BsonIgnore]
        public double CombinedScore {get {
            return this.AvgWeightedScoresByQuestionId.Values.Sum();
        }}


        [BsonIgnore]
        public List<WeightedScore> WeightedScores {
            get {
                List<WeightedScore> weightedScores = new List<WeightedScore>();

                // Collect all scores from each submission and convert
                foreach(var kvp in ScoringSubmissions)
                {
                    string userId = kvp.Key;
                    ScoringSubmission scoringSubmision = kvp.Value;

                    // Get user's role during scoring
                    string scoringRoleId = this.ReferenceEvent.UserScoringRoles[userId];
                    ScoringRole scoringRole = this.ReferenceEvent.ScoringRoles[scoringRoleId];

                    // Create weighted score objects
                    foreach(var kvp2 in scoringSubmision.Scores)
                    {
                        string questionId = kvp2.Key;
                        int score = kvp2.Value;

                        // Get question using question id
                        ScoreQuestion question = this.ReferenceEvent.ScoringQuestions[questionId];

                        var weightedScore = new WeightedScore(score, scoringRole, question);
                        weightedScores.Add(weightedScore);
                    }
                }

                return weightedScores;
            }
        }
        [BsonIgnore]
        public Dictionary<ScoringRole, Dictionary<ScoreQuestion, double>> AvgWeightedScoresByQuestionGroupedByRole { get {
            // Group the scores by role
            var groupedScoresByRole = this.WeightedScores.GroupBy(p=> p.Role);

            // Within each role, group by question, then find the average score for each question.
            var avgWeightedScoresByQuestionIdGroupedByRole = groupedScoresByRole.ToDictionary(
                roleGroup=> roleGroup.Key, // Store results using role as key
                roleGroup=> roleGroup // Provide all scores related to this role to next step
                    .GroupBy(kvQ => kvQ.Question).ToDictionary(
                        questionGroup => questionGroup.Key, // Store results using question as key
                        questionGroup => questionGroup.Average(s=> s.CalculatedScore) // Calculate average score for this question
                    )
            );
            return avgWeightedScoresByQuestionIdGroupedByRole;
        }}
        #endregion
    }
}
