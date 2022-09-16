using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
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
                            UserId = Guid.Parse(values[0]),
                            HackathonExperience = Convert.ToInt32(values[1]),
                            CodingExperience = Convert.ToInt32(values[2]),
                            CommunicationExperience = Convert.ToInt32(values[3]),
                            OrganizationExperience = Convert.ToInt32(values[4]),
                            DocumentationExperience = Convert.ToInt32(values[5]),
                            BusinessExperience = Convert.ToInt32(values[6]),
                            CreativityExperience = Convert.ToInt32(values[7]),
                            Major = values[8],
                            SchoolYear = values[9]
                        });
                    }
                }                
                return eventApplications;
            }}
        
            /// <summary>
            /// Description: This an example hackathon event preloaded with event applications and teams. It is used for testing calculations for team experience.
            /// </summary> 
            public static HackathonEvent SampleHackthonEvent { get {
                // Result variable
                var hackathonEvent = new HackathonEvent();

                // Load sample applications
                var sampleEventApplications = SampleEventApplications_Distributed;

                // Users create accounts
                Dictionary<string, ApplicationUser> users = new Dictionary<string, ApplicationUser>();
                foreach(var eventApplication in sampleEventApplications){
                    var user = new ApplicationUser() { Id = eventApplication.UserId };
                    users.Add(user.Id.ToString(), user);
                }

                // Users apply to hackathon event
                foreach(var eventApplication in sampleEventApplications){
                    hackathonEvent.EventApplications.Add(eventApplication.Id.ToString(), eventApplication);
                }

                // Teams are created
                Team currTeam = null;
                for(int i=0; i<sampleEventApplications.Count; i+=1)
                {
                    // Create a new team every 5 people
                    if (i % 5 == 0) {
                        currTeam = new Team(){Id=ObjectId.GenerateNewId(), ReferenceEvent=hackathonEvent};
                        hackathonEvent.Teams.Add(currTeam.Id.ToString(), currTeam);
                    }
                    // Assign an event application to a team
                    var eventApplication = sampleEventApplications[i];
                    hackathonEvent.EventAppTeams[eventApplication.Id.ToString()] = currTeam.Id.ToString();
                }

                return hackathonEvent;
            }}
        }
        

        # region Experience
        /// <summary>
        /// Description: Verifies that the combined hackathon experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void HackathonExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.HackathonExperience;

            // Assert
            Assert.Equal(sumExp, 103);
        }

        /// <summary>
        /// Description: Verifies that the combined coding experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void CodingExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.CodingExperience;

            // Assert
            Assert.Equal(sumExp, 214);
        }

        /// <summary>
        /// Description: Verifies that the combined communication experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void CommunicationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.CommunicationExperience;

            // Assert
            Assert.Equal(sumExp, 120);
        }

        /// <summary>
        /// Description: Verifies that the combined organization experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void OrganizationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.OrganizationExperience;

            // Assert
            Assert.Equal(sumExp, 144);
        }

        /// <summary>
        /// Description: Verifies that the combined documentation experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void DocumentationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.DocumentationExperience;

            // Assert
            Assert.Equal(sumExp, 122);
        }

        /// <summary>
        /// Description: Verifies that the combined business experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void BusinessExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.BusinessExperience;

            // Assert
            Assert.Equal(sumExp, 103);
        }

        /// <summary>
        /// Description: Verifies that the combined creativity experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void CreativityExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var sumExp = hackathonEvent.CreativityExperience;

            // Assert
            Assert.Equal(sumExp, 73);
        }


        /// <summary>
        /// Description: Verifies that the average hackathon experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamHackathonExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamHackathonExperience;

            // Assert
            Assert.Equal(7.36, Math.Round(avgExp, 2));
        }

        /// <summary>
        /// Description: Verifies that the average coding experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamCodingExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamCodingExperience;

            // Assert
            Assert.Equal(15.29, Math.Round(avgExp, 2));
        }

        /// <summary>
        /// Description: Verifies that the average communication experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamCommunicationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamCommunicationExperience;

            // Assert
            Assert.Equal(8.57, Math.Round(avgExp, 2));
        }

        /// <summary>
        /// Description: Verifies that the average organization experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamOrganizationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamOrganizationExperience;

            // Assert
            Assert.Equal(10.29, Math.Round(avgExp, 2));
        }

        /// <summary>
        /// Description: Verifies that the average documentation experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamDocumentationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamDocumentationExperience;

            // Assert
            Assert.Equal(8.71, Math.Round(avgExp, 2));
        }

        /// <summary>
        /// Description: Verifies that the average business experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamBusinessExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamBusinessExperience;

            // Assert
            Assert.Equal(7.36, Math.Round(avgExp, 2));
        }

        /// <summary>
        /// Description: Verifies that the average creativity experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgTeamCreativityExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var avgExp = hackathonEvent.AvgTeamCreativityExperience;

            // Assert
            Assert.Equal(5.21, Math.Round(avgExp, 2));
        }
        
        

        /// <summary>
        /// Description: Verifies the standard deviation of hackathon experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamHackathonExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamHackathonExperience;

            // Assert
            Assert.Equal(5.33, Math.Round(stdDevExp, 2));
        }
        /// <summary>
        /// Description: Verifies the standard deviation of coding experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamCodingExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamCodingExperience;

            // Assert
            Assert.Equal(3.30, Math.Round(stdDevExp, 2));
        }
        /// <summary>
        /// Description: Verifies the standard deviation of communication experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamCommunicationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamCommunicationExperience;

            // Assert
            Assert.Equal(4.30, Math.Round(stdDevExp, 2));
        }
        /// <summary>
        /// Description: Verifies the standard deviation of organization experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamOrganizationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamOrganizationExperience;

            // Assert
            Assert.Equal(4.15, Math.Round(stdDevExp, 2));
        }
        /// <summary>
        /// Description: Verifies the standard deviation of documentation experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamDocumentationExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamDocumentationExperience;

            // Assert
            Assert.Equal(2.81, Math.Round(stdDevExp, 2));
        }
        /// <summary>
        /// Description: Verifies the standard deviation of business experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamBusinessExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamBusinessExperience;

            // Assert
            Assert.Equal(3.88, Math.Round(stdDevExp, 2));
        }
        /// <summary>
        /// Description: Verifies the standard deviation of creativity experience across teams is calculated correctly.
        /// </summary> 
        [Fact]
        public void StdDevTeamCreativityExperience()
        {
            // Define
            var hackathonEvent = SampleData.SampleHackthonEvent;

            // Process
            var stdDevExp = hackathonEvent.StdDevTeamCreativityExperience;

            // Assert
            Assert.Equal(3.10, Math.Round(stdDevExp, 2));
        }
        [Fact]
        public void StandardDeviation()
        {
            // Define
            double[] values = new double[] {3, 4, 5, 7, 8, 6, 1, 2, 1};

            // Process
            double stddev = HackathonEvent.StandardDeviation(values);

            // Assert
            Assert.Equal(2.42, Math.Round(stddev, 2));
        }


        
        #endregion


        /// <summary>
        /// Description: Verifies that event applications can be assigned to teams such that they can be fairly balanced.
        /// </summary> 
        [Fact]
        public void AssignTeams()
        {
            // Define
            var hackathonEvent = new HackathonEvent() {Name = "HackOkState" };
            hackathonEvent.EventApplications = SampleData.SampleEventApplications_Distributed.ToDictionary(p=> p.UserId.ToString(), p=> p);
            var numTeams = 10;
            var maxPerTeam = 5;

            // Process
            hackathonEvent.AssignTeams(numTeams, maxPerTeam);

            // Assert - That it's less than half a person's experience different
            Assert.True(hackathonEvent.AvgOfStdDevAllExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamHackathonExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamCodingExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamCommunicationExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamOrganizationExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamDocumentationExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamBusinessExperience < 2.5);
            Assert.True(hackathonEvent.StdDevTeamCreativityExperience < 2.5);
        }

    }
}
