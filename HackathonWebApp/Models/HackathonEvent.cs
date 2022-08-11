using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
        public static Dictionary<string, Team> AssignTeams(List<EventApplication> eventApplications, int numTeams) {
            var assignedTeams = new Dictionary<string, Team>();


            // Example: Adding a team to the dictionary
            // The team is stored in the dictionary using the team ID, so it can be quickly retrieved in other operations.
            var myTeam = new Team() {Id = ObjectId.GenerateNewId() };
            assignedTeams.Add(myTeam.Id.ToString(), myTeam);

            // Example" Adding an EventApplication to a team.
            // The appliction is stored in the dictionary using their ID, so it can be quickly retrieved in other operations.
            var userEventApplication = eventApplications.First();
            //myTeam.EventApplications.Add(userEventApplication.UserId.ToString(), userEventApplication);


            // Write some optimization code here to move event applications on to various teams.


            return assignedTeams;
        }

    }
}
