using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using MongoDB.Bson;
using HackathonWebApp.Models;


namespace HackathonWebAppTests
{
    public class HackathonEventTests
    {
        private static class SampleData
        {
            /// <summary>
            /// Description: This set of event applications is meant to represent applications to attend a hackathon event. It is used for testing the team balancing algorithm.
            /// </summary> 
            public static List<EventApplication> SampleEventApplications_Distributed { get {
                var eventApplications = new List<EventApplication>();
                string csvFileLocation = Path.Combine(Directory.GetCurrentDirectory(), "Models", "SampleData", "EventApplications-Distributed.csv");
                using(var reader = new StreamReader(csvFileLocation))
                {
                    var header = reader.ReadLine();
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var values = line.Split(',');
                        eventApplications.Add( new EventApplication(){
                            Id = ObjectId.GenerateNewId(),
                            EventId = ObjectId.Parse(values[0]),
                            UserId = Guid.Parse(values[1]),
                            HackathonExperience = Convert.ToInt32(values[2]),
                            CodingExperience = Convert.ToInt32(values[3]),
                            CommunicationExperience = Convert.ToInt32(values[4]),
                            OrganizationExperience = Convert.ToInt32(values[5]),
                            DocumentationExperience = Convert.ToInt32(values[6]),
                            BusinessExperience = Convert.ToInt32(values[7]),
                            CreativityExperience = Convert.ToInt32(values[8])
                        });
                    }
                }                
                return eventApplications;
            }}
        }

        /// <summary>
        /// Description: Verifies that event applications can be assigned to teams such that they can be fairly balanced.
        /// </summary> 
        [Fact]
        public void AssignTeams()
        {
            // Define
            var hackathonEvent = new HackathonEvent() {Name = "HackOkState" };
            var eventApplications = SampleData.SampleEventApplications_Distributed;
            var numTeams = 10;

            // Process
            var assignedTeams = HackathonEvent.AssignTeams(eventApplications, numTeams);
            hackathonEvent.Teams = assignedTeams;

            // Assert
            Assert.True(hackathonEvent.StdDevTeamHackathonExperience < 5);
            Assert.True(hackathonEvent.StdDevTeamCodingExperience < 5);
            Assert.True(hackathonEvent.StdDevTeamCommunicationExperience < 5);
            Assert.True(hackathonEvent.StdDevTeamOrganizationExperience < 5);
            Assert.True(hackathonEvent.StdDevTeamDocumentationExperience < 5);
            Assert.True(hackathonEvent.StdDevTeamBusinessExperience < 5);
            Assert.True(hackathonEvent.StdDevTeamCreativityExperience < 5);

        }

    }
}
