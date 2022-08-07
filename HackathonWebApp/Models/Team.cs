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
    }
}
