using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HackathonWebApp.Models
{
    public class Team
    {
        [BsonId]
        public ObjectId Id {get; set;}

        [BsonId]
        [BsonElement("event_id")]
        public ObjectId EventId {get; set;}

        [Required]
        [BsonElement("name")]
        public string Name { get; set; }

        [Required]
        [BsonElement("team_members")]
        public Dictionary<Guid, ApplicationUser> TeamMembers { get; set; } = new Dictionary<Guid, ApplicationUser>();

        // Fields that would need aggregated from other collections
        [BsonIgnore]
        public Dictionary<Guid, EventApplication> TeamMemberApplications { get; set; } = new Dictionary<Guid, EventApplication>();
        [BsonIgnore]
        public Dictionary<string, ScoringSubmission> ScoringSubmissions {get; set; } = new Dictionary<string, ScoringSubmission>();

        // Calculated Results
        public Dictionary<string, int> CountScoresByQuestionId { get {

            // Collapse to just the questions
            var allQuestions = this.ScoringSubmissions.Values.Select(p=> p.Scores).SelectMany(x => x).ToList();
            
            // Count by question ID
            var counts = allQuestions.GroupBy(p=> p.Key).ToDictionary(p=> p.Key, p=> p.Count());

            return counts;
        }}
        public Dictionary<string, double> AvgScoresByQuestionId { get {
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

        // Calculated Properties
        public double HackathonExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.HackathonExperience);
        }}
        public double CodingExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.CodingExperience);
        }}
        public double CommunicationExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.CommunicationExperience);
        }}
        public double OrganizationExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.OrganizationExperience);
        }}
        public double DocumentationExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.DocumentationExperience);
        }}
        public double BusinessExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.BusinessExperience);
        }}
        public double CreativityExperience { get {
            return this.TeamMemberApplications.Values.Sum(ea => ea.CreativityExperience);
        }}

        public double AvgHackathonExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.HackathonExperience);
        }}
        public double AvgCodingExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.CodingExperience);
        }}
        public double AvgCommunicationExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.CommunicationExperience);
        }}
        public double AvgOrganizationExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.OrganizationExperience);
        }}
        public double AvgDocumentationExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.DocumentationExperience);
        }}
        public double AvgBusinessExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.BusinessExperience);
        }}
        public double AvgCreativityExperience { get {
            return this.TeamMemberApplications.Values.Average(ea => ea.CreativityExperience);
        }}
    }
}
