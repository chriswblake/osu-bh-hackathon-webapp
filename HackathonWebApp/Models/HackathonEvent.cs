using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    public class HackathonEvent
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
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
        [BsonElement("registration_settings")]
        public RegistrationSettings RegistrationSettings { get; set; }

        // Fields that would need aggregated from other collections
        [BsonIgnore]
        public Dictionary<ObjectId, Team> Teams {get; set;} = new Dictionary<ObjectId, Team>();


        // Calculated Properties
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


        // Convert these from abstract to actual methods and write the unit tests.
        [BsonIgnore]
        public Dictionary<Guid, ApplicationUser> Participants { get { return null; }} // Retrieves all members on team into 1 dictionary.
        [BsonIgnore]
        public double AvgTeamHackathonExperience { get { return 0; }}
        [BsonIgnore]
        public double AvgTeamCodingExperience { get { return 0; }}
        [BsonIgnore]
        public double AvgTeamCommunicationExperience { get { return 0; }}
        [BsonIgnore]
        public double AvgTeamOrganizationExperience { get { return 0; }}
        [BsonIgnore]
        public double AvgTeamDocumentationExperience { get { return 0; }}
        [BsonIgnore]
        public double AvgTeamBusinessExperience { get { return 0; }}
        [BsonIgnore]
        public double AvgTeamCreativityExperience { get { return 0; }}

        [BsonIgnore]
        public double StdDevTeamHackathonExperience { get { return 0; }}
        [BsonIgnore]
        public double StdDevTeamCodingExperience { get { return 0; }}
        [BsonIgnore]
        public double StdDevTeamCommunicationExperience { get { return 0; }}
        [BsonIgnore]
        public double StdDevTeamOrganizationExperience { get { return 0; }}
        [BsonIgnore]
        public double StdDevTeamDocumentationExperience { get { return 0; }}
        [BsonIgnore]
        public double StdDevTeamBusinessExperience { get { return 0; }}
        [BsonIgnore]
        public double StdDevTeamCreativityExperience { get { return 0; }}

        // Methods
        /// <summary>
        /// Description: Given a list of event applications, it assigns the applications
        /// into teams such that the the teams are evenly balanced.
        /// <para>eventApplications: List<EventApplications>: A list of event applications, representing the experience of various application users.</para>
        /// <para>numTeams: Int: The number of teams to assign the applications to.</para>
        /// </summary>
        public static Dictionary<ObjectId, Team> AssignTeams(List<EventApplication> eventApplications, int numTeams) {
            var assignedTeams = new Dictionary<ObjectId, Team>();


            // Example: Adding a team to the dictionary
            // The team is stored in the dictionary using the team ID, so it can be quickly retrieved in other operations.
            var myTeam = new Team() {Id = ObjectId.GenerateNewId() };
            assignedTeams.Add(myTeam.Id, myTeam);

            // Example" Adding an EventApplication to a team.
            // The appliction is stored in the dictionary using their ID, so it can be quickly retrieved in other operations.
            var userEventApplication = eventApplications.First();
            myTeam.TeamMemberApplications.Add(userEventApplication.UserId, userEventApplication);


            // Write some optimization code here to move event applications on to various teams.


            return assignedTeams;
        }

    }
}
