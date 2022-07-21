using System;
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
            public static List<EventApplication> SampleEventApplications = new List<EventApplication> { 
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0},
                new EventApplication(){ Id = ObjectId.Parse("000000000000000000000001"), UserId = Guid.NewGuid(), HackathonExperience = 1, CodingExperience = 1, CommunicationExperience = 1, OrganizationExperience = 1, DocumentationExperience = 1, BusinessExperience = 3, CreativityExperience = 0}
            };

        }

        /// <summary>
        /// Description: Verifies that event applications can be assigned to teams such that they can be fairly balanced.
        /// </summary> 
        [Fact]
        public void AssignTeams()
        {
            // Define
            var hackathonEvent = new HackathonEvent() {Name = "HackOkState" };
            var eventApplications = SampleData.SampleEventApplications;
            var numTeams = 10;

            // Process
            var assignedTeams = hackathonEvent.AssignTeams(eventApplications, numTeams);

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
