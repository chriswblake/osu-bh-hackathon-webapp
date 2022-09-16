using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
    public class HackathonEvent
    {
        #region Event Settings
        [BsonId]
        public ObjectId Id {get; set;}

        [Required]
        [BsonElement("is_active")]
        public bool IsActive { get; set; }
        
        [Required]
        [BsonElement("name")]
        public string Name { get; set; }
        
        [Required]
        [BsonElement("time_zone_id")]
        public string TimeZoneId { get; set; } = "Central Standard Time"; // Default
        [BsonIgnore]
        public TimeZoneInfo TimeZoneInfo {get {
            try {
                return TimeZoneInfo.FindSystemTimeZoneById(this.TimeZoneId);
            } catch {
                // If the id stored in the DB fails (becaue a time zone id is changed), fail to UTC.
                return TimeZoneInfo.Utc;
            }
        }}
        public ReadOnlyCollection<TimeZoneInfo> TimeZoneInfoOptions { get {
            return  TimeZoneInfo.GetSystemTimeZones();
        }}

        [Required]
        [BsonElement("start_time")]
        public DateTime StartTime { get; set; }

        [Required]
        [BsonElement("end_time")]
        public DateTime EndTime { get; set; }

        [Required]
        [BsonElement("registration_opens_time")]
        public DateTime RegistrationOpensTime { get; set; }

        [Required]
        [BsonElement("early_registration_closes_time")]
        public DateTime EarlyRegistrationClosesTime { get; set; }

        [Required]
        [BsonElement("registration_closes_time")]
        public DateTime RegistrationClosesTime { get; set; }

        [Required]
        [BsonElement("sponsors")]
        public Dictionary<string,Sponsor> Sponsors { get; set; } = new Dictionary<string, Sponsor>();

        [Required]
        [BsonElement("organizers")]
        public Dictionary<string,Organizer> Organizers { get; set; } = new Dictionary<string, Organizer>();

        [Required]
        [BsonElement("registration_settings")]
        public RegistrationSettings RegistrationSettings { get; set; }

        [Required]
        [BsonElement("equipment")]
        public Dictionary<string,HackingEquipment> Equipment { get; set; } = new Dictionary<string, HackingEquipment>();
        #endregion

        #region Team Management
        [Required]
        [BsonElement("show_teams_time")]
        public DateTime ShowTeamsTime { get; set; }

        [Required]
        [BsonElement("event_applications")]
        public Dictionary<string,EventApplication> EventApplications { get; set; } = new Dictionary<string, EventApplication>();

        /// <summary>
        /// A mapping of EventApplication UserIds (key) to Team ObjectIds (value), but stored as strings. Each Event application may be assigned to only 1 team.
        /// </summary>
        [Required]
        [BsonElement("event_app_teams")]
        public Dictionary<string,string> EventAppTeams { get; set; } = new Dictionary<string, string>();

        [BsonElement("teams")]
        public Dictionary<string, Team> Teams {get; set;} = new Dictionary<string, Team>();


        [BsonIgnore]
        public double HackathonExperience { get {
            return this.Teams.Values.Sum(t => t.HackathonExperience);
        }}
        [BsonIgnore]
        public double CodingExperience { get {
            return this.Teams.Values.Sum(t => t.CodingExperience);
        }}
        [BsonIgnore]
        public double CommunicationExperience { get {
            return this.Teams.Values.Sum(t => t.CommunicationExperience);
        }}
        [BsonIgnore]
        public double OrganizationExperience { get {
            return this.Teams.Values.Sum(t => t.OrganizationExperience);
        }}
        [BsonIgnore]
        public double DocumentationExperience { get {
            return this.Teams.Values.Sum(t => t.DocumentationExperience);
        }}
        [BsonIgnore]
        public double BusinessExperience { get {
            return this.Teams.Values.Sum(t => t.BusinessExperience);
        }}
        [BsonIgnore]
        public double CreativityExperience { get {
            return this.Teams.Values.Sum(t => t.CreativityExperience);
        }}


        [BsonIgnore]
        public double AvgTeamHackathonExperience { get {
            if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.HackathonExperience);
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgTeamCodingExperience { get { 
            if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.CodingExperience);
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgTeamCommunicationExperience { get { 
            if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.CommunicationExperience);
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgTeamOrganizationExperience { get { 
            if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.OrganizationExperience);
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgTeamDocumentationExperience { get { 
            if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.DocumentationExperience);
            else
                return 0;
        }}
        [BsonIgnore]
        public double AvgTeamBusinessExperience { get {
             if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.BusinessExperience);
            else
                return 0;
            }}
        [BsonIgnore]
        public double AvgTeamCreativityExperience { get { 
            if (this.Teams.Count() > 0)
                return this.Teams.Values.Average(ea => ea.CreativityExperience);
            else
                return 0;
        }}
        
        [BsonIgnore]
        public double AvgOfStdDevAllExperience { get {
            if (this.Teams.Count() > 1)
                return new List<double> {
                    StdDevTeamHackathonExperience,
                    StdDevTeamCodingExperience,
                    StdDevTeamCommunicationExperience,
                    StdDevTeamOrganizationExperience,
                    StdDevTeamDocumentationExperience,
                    StdDevTeamBusinessExperience,
                    StdDevTeamCreativityExperience
                }.Average();
            else
                return double.PositiveInfinity;
        }}
        [BsonIgnore]
        public double StdDevOfStdDevAllExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation( new List<double> {
                    StdDevTeamHackathonExperience,
                    StdDevTeamCodingExperience,
                    StdDevTeamCommunicationExperience,
                    StdDevTeamOrganizationExperience,
                    StdDevTeamDocumentationExperience,
                    StdDevTeamBusinessExperience,
                    StdDevTeamCreativityExperience
                });
            else
                return double.PositiveInfinity;
        }}

        [BsonIgnore]
        public double StdDevTeamHackathonExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.HackathonExperience));
            else
                return 0;
        }}
        [BsonIgnore]
        public double StdDevTeamCodingExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.CodingExperience));
            else
                return 0;
        }}
        [BsonIgnore]
        public double StdDevTeamCommunicationExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.CommunicationExperience));
            else
                return 0;
        }}
        [BsonIgnore]
        public double StdDevTeamOrganizationExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.OrganizationExperience));
            else
                return 0;
        }}
        [BsonIgnore]
        public double StdDevTeamDocumentationExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.DocumentationExperience));
            else
                return 0;
        }}
        [BsonIgnore]
        public double StdDevTeamBusinessExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.BusinessExperience));
            else
                return 0;
        }}
        [BsonIgnore]
        public double StdDevTeamCreativityExperience { get {
            if (this.Teams.Count() > 1)
                return StandardDeviation(this.Teams.Values.Select(ea => ea.CreativityExperience));
            else
                return 0;
        }}
        public static double StandardDeviation(IEnumerable<double> values)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v=>Math.Pow(v-avg,2)));
        }
        #endregion

        #region Scoring Management
        [Required]
        [BsonElement("scoring_questions")]
        public Dictionary<string,ScoreQuestion> ScoringQuestions { get; set; } = new Dictionary<string, ScoreQuestion>();

        [Required]
        [BsonElement("scoring_roles")]
        public Dictionary<string,ScoringRole> ScoringRoles { get; set; } = new Dictionary<string, ScoringRole>();

        /// <summary>
        /// A mapping of ApplicationUser Guids (key) to ScoringRole ObjectIds (key), but stored as strings. Each user may have only 1 role.
        /// </summary>
        [Required]
        [BsonElement("user_scoring_roles")]
        public Dictionary<string,string> UserScoringRoles { get; set; } = new Dictionary<string, string>();
        #endregion

        
        // Methods
        /// <summary>
        /// Description: Given a list of event applications, it assigns the applications
        /// into teams such that the the teams are evenly balanced.
        /// <para>eventApplications: List<EventApplications>: A list of event applications, representing the experience of various application users.</para>
        /// <para>numTeams: Int: The number of teams to assign the applications to.</para>
        /// </summary>
        public void AssignTeams(int numTeams) {
            # region Get available applications
            // Get only unassigned applications
            var unassignedEventApplications = this.EventApplications.Values.Where(
                p=>    p.ConfirmationState == EventApplication.ConfirmationStateOption.unassigned
                    || p.ConfirmationState == EventApplication.ConfirmationStateOption.assigned
                    || p.ConfirmationState == EventApplication.ConfirmationStateOption.unconfirmed
            );

            // Count experience and max for normalization
            double hackathonExpTotal      = unassignedEventApplications.Sum(p=> p.HackathonExperience);
            double codingExpTotal         = unassignedEventApplications.Sum(p=> p.CodingExperience);
            double communicationExpTotal  = unassignedEventApplications.Sum(p=> p.CommunicationExperience);
            double organizationExpTotal   = unassignedEventApplications.Sum(p=> p.OrganizationExperience);
            double documentationExpTotal  = unassignedEventApplications.Sum(p=> p.DocumentationExperience);
            double businessExpTotal       = unassignedEventApplications.Sum(p=> p.BusinessExperience);
            double creativityExpTotal     = unassignedEventApplications.Sum(p=> p.CreativityExperience);
            double maxExperience = new List<double>() {
                hackathonExpTotal,
                codingExpTotal,
                communicationExpTotal,
                organizationExpTotal,
                documentationExpTotal,
                businessExpTotal,
                creativityExpTotal
            }.Max();

            // Calculate weights
            double hackathonWeight      = 1 - hackathonExpTotal / maxExperience;
            double codingWeight         = 1 - codingExpTotal / maxExperience;
            double communicationWeight  = 1 - communicationExpTotal / maxExperience;
            double organizationWeight   = 1 - organizationExpTotal / maxExperience;
            double documentationWeight  = 1 - documentationExpTotal / maxExperience;
            double businessWeight       = 1 - businessExpTotal / maxExperience;
            double creativityWeight     = 1 - creativityExpTotal / maxExperience;

            // Create sorted clone of applications
            List<EventApplication> sortedApplications = unassignedEventApplications.ToDictionary(
                p=> p,
                p=> (
                      p.HackathonExperience         * hackathonWeight
                    + p.CodingExperience            * codingWeight
                    + p.CommunicationExperience     * communicationWeight
                    + p.OrganizationExperience      * organizationWeight
                    + p.DocumentationExperience     * documentationWeight
                    + p.BusinessExperience          * businessWeight
                    + p.CreativityExperience        * creativityWeight
                )
            ).OrderBy(kvp => kvp.Value).Select(kvp=> kvp.Key).ToList();

            #endregion

            // Run the auto placement several times and pick the one with the lowest standard deviation
            List<HackathonEvent> teamPlacementAttempts = new List<HackathonEvent>();
            for (int attempts=0; attempts<1000; attempts++)
            {
                // Create temporary hackathon event with copy of the event applications
                HackathonEvent currHackathonEvent = new HackathonEvent();
                currHackathonEvent.EventApplications = this.EventApplications;
                var currSortedApplications = sortedApplications.ToList();

                // Create empty teams and associate to the temporary hackathon event
                for (int t=1; t<=numTeams; t++)
                {
                    Team team = new Team() {
                        Id = ObjectId.GenerateNewId(),
                        Name = "Team " + t.ToString(),
                        ReferenceEvent = currHackathonEvent
                    };
                    currHackathonEvent.Teams[team.Id.ToString()] = team;
                }

                // Assign applications to teams
                Random rand = new Random();
                while (currSortedApplications.Count() > 0)
                {   
                    // Pick selection method for this round
                    string selectMethod = new string[] {"start", "middle", "end"}[rand.Next(3)];
                    
                    // Add an application to each team
                    foreach (Team currTeam in currHackathonEvent.Teams.Values)
                    {
                        // Stop if no applications left
                        if (currSortedApplications.Count == 0)
                            break;

                        // Pick pop location
                        var popPos = 0;
                        switch (selectMethod)
                        {
                            case "start":
                                // Use the default: 0
                                break;
                            case "middle":
                                popPos = currSortedApplications.Count()/2;
                                break;
                            case "end":
                                popPos = currSortedApplications.Count()-1;
                                break;
                        }

                        // Get an event application
                        var eventApp = currSortedApplications[popPos];

                        // Assign user to a team, and remove from list
                        currHackathonEvent.EventAppTeams[eventApp.UserId.ToString()] = currTeam.Id.ToString();
                        currSortedApplications.RemoveAt(popPos);
                    }
                }
            
                // Store Team Placement attempt
                teamPlacementAttempts.Add(currHackathonEvent);
            }

            // Select attempt with best average and standard deviation
            var bestTeamPlacement = teamPlacementAttempts.OrderBy(p=> p.AvgOfStdDevAllExperience).First();

            // Save results to this hackathon event
            this.EventAppTeams = bestTeamPlacement.EventAppTeams;
            this.Teams = bestTeamPlacement.Teams;
        }

    }
}
