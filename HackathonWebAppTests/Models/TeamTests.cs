using System;
using Xunit;
using MongoDB.Bson;
using HackathonWebApp.Models;


namespace HackathonWebAppTests
{
    public class TeamTests
    {
        private static class SampleData
        {
            /// <summary>
            /// Description: This team with embedded sample data is meant to be used with the tests that validate avg/sum/etc.
            /// of different experience scores.
            /// </summary> 
            public static Team SampleTeam_AvgsSums { get {
                var team = new Team();

                // Add users
                var user1 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_1" };
                var user2 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_2" };
                var user3 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_3" };
                var user4 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_4" };
                var user5 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_5" };
                team.TeamMembers.Add(user1.Id, user1);
                team.TeamMembers.Add(user2.Id, user2);
                team.TeamMembers.Add(user3.Id, user3);
                team.TeamMembers.Add(user4.Id, user4);
                team.TeamMembers.Add(user5.Id, user5);

                // Add users' event applications
                var eventApp1 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000001"), UserId = user1.Id,
                    HackathonExperience = 1,
                    CodingExperience = 1,
                    CommunicationExperience = 1,
                    OrganizationExperience = 1,
                    DocumentationExperience = 1,
                    BusinessExperience = 3,
                    CreativityExperience = 0
                };
                var eventApp2 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000002"), UserId = user2.Id,
                    HackathonExperience = 2,
                    CodingExperience = 4,
                    CommunicationExperience = 2,
                    OrganizationExperience = 2,
                    DocumentationExperience = 2,
                    BusinessExperience = 2,
                    CreativityExperience = 5
                };
                var eventApp3 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000002"), UserId = user3.Id,
                    HackathonExperience = 1,
                    CodingExperience = 1,
                    CommunicationExperience = 4,
                    OrganizationExperience = 5,
                    DocumentationExperience = 1,
                    BusinessExperience = 3,
                    CreativityExperience = 0
                };
                var eventApp4 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000002"), UserId = user4.Id,
                    HackathonExperience = 1,
                    CodingExperience = 3,
                    CommunicationExperience = 5,
                    OrganizationExperience = 3,
                    DocumentationExperience = 4,
                    BusinessExperience = 5,
                    CreativityExperience = 1
                };
                var eventApp5 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000002"), UserId = user5.Id,
                    HackathonExperience = 4,
                    CodingExperience = 5,
                    CommunicationExperience = 4,
                    OrganizationExperience = 4,
                    DocumentationExperience = 5,
                    BusinessExperience = 4,
                    CreativityExperience = 5
                };
                team.EventApplications.Add(user1.Id, eventApp1);
                team.EventApplications.Add(user2.Id, eventApp2);
                team.EventApplications.Add(user3.Id, eventApp3);
                team.EventApplications.Add(user4.Id, eventApp4);
                team.EventApplications.Add(user5.Id, eventApp5);

                return team;
            }}
        }

        /// <summary>
        /// Description: Verifies that sponsor details are not modified after being set.
        /// </summary> 
        [Fact]
        public void Team_Set_Properties()
        {
            // Define
            var user1 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david123" };
            var eventApp1 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000001"), UserId = user1.Id};

            // Process
            var team = new Team()
            {
                Id = ObjectId.Parse("123412341234123412341234"),
                Name = "The Shindigs"
            };
            team.TeamMembers.Add(user1.Id, user1);
            team.EventApplications.Add(user1.Id, eventApp1);

            // Assert
            Assert.Equal(ObjectId.Parse("123412341234123412341234"), team.Id);
            Assert.Equal("The Shindigs", team.Name);
            Assert.Equal(1, team.TeamMembers.Count);
            Assert.Equal(1, team.EventApplications.Count);

        }

        #region Calculated Results
        /// <summary>
        /// Description: Verifies that multiple score submission across several questions can be properally counted.
        /// </summary> 
        [Fact]
        public void Test_CountScoresByQuestionId() {
            // Define
            var team = new Team() { Id=ObjectId.GenerateNewId() };

            var ss1 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId1", 2},
                    { "questionId2", 1},
                    { "questionId3", 3}
                }
            };
            team.ScoringSubmissions.Add(ss1.ProjectId + ", " + ss1.UserId, ss1);

            var ss2 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId2", 5},
                    { "questionId3", 5},
                    { "questionId4", 5}
                }
            };
            team.ScoringSubmissions.Add(ss2.ProjectId + ", " + ss2.UserId, ss2);

            var ss3 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId3", 1},
                    { "questionId4", 1},
                    { "questionId5", 1}
                }
            };
            team.ScoringSubmissions.Add(ss3.ProjectId + ", " + ss3.UserId, ss3);
            
            var ss4 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId3", 5},
                    { "questionId4", 5},
                    { "questionId5", 2}
                }
            };
            team.ScoringSubmissions.Add(ss4.ProjectId + ", " + ss4.UserId, ss4);

            // Process
            var countScores = team.CountScoresByQuestionId;

            // Assert
            Assert.Equal(1, countScores["questionId1"]);
            Assert.Equal(2, countScores["questionId2"]);
            Assert.Equal(4, countScores["questionId3"]);
            Assert.Equal(3, countScores["questionId4"]);
            Assert.Equal(2, countScores["questionId5"]);
        }

        /// <summary>
        /// Description: Verifies that multiple score submission across several questions can be properally averaged.
        /// </summary> 
        [Fact]
        public void Test_AvgScoresByQuestionId() {
            // Define
            var team = new Team() { Id=ObjectId.GenerateNewId() };

            var ss1 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId1", 2},
                    { "questionId2", 1},
                    { "questionId3", 3}
                }
            };
            team.ScoringSubmissions.Add(ss1.ProjectId + ", " + ss1.UserId, ss1);

            var ss2 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId1", 5},
                    { "questionId2", 5},
                    { "questionId3", 5}
                }
            };
            team.ScoringSubmissions.Add(ss2.ProjectId + ", " + ss2.UserId, ss2);

            var ss3 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId3", 1},
                    { "questionId4", 1},
                    { "questionId5", 1}
                }
            };
            team.ScoringSubmissions.Add(ss3.ProjectId + ", " + ss3.UserId, ss3);
            
            var ss4 = new ScoringSubmission () { 
                ProjectId=team.Id.ToString(),
                UserId = Guid.NewGuid().ToString(),
                Scores = new System.Collections.Generic.Dictionary<string, int>() {
                    { "questionId3", 5},
                    { "questionId4", 5},
                    { "questionId5", 2}
                }
            };
            team.ScoringSubmissions.Add(ss4.ProjectId + ", " + ss4.UserId, ss4);

            // Process
            var avgScores = team.AvgScoresByQuestionId;

            // Assert
            Assert.Equal((2+5)/2.0,     avgScores["questionId1"]);
            Assert.Equal((1+5)/2.0,     avgScores["questionId2"]);
            Assert.Equal((3+5+1+5)/4.0, avgScores["questionId3"]);
            Assert.Equal((1+5)/2.0,     avgScores["questionId4"]);
            Assert.Equal((1+2)/2.0,     avgScores["questionId5"]);
        }
        #endregion

        # region Calculated Properties
        /// <summary>
        /// Description: Verifies that the combined hackathon experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void HackathonExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.HackathonExperience;

            // Assert
            Assert.Equal(sumExp, 9);
        }

        /// <summary>
        /// Description: Verifies that the combined coding experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void CodingExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.CodingExperience;

            // Assert
            Assert.Equal(sumExp, 14);
        }

        /// <summary>
        /// Description: Verifies that the combined communication experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void CommunicationExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.CommunicationExperience;

            // Assert
            Assert.Equal(sumExp, 16);
        }

        /// <summary>
        /// Description: Verifies that the combined organization experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void OrganizationExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.OrganizationExperience;

            // Assert
            Assert.Equal(sumExp, 15);
        }

        /// <summary>
        /// Description: Verifies that the combined documentation experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void DocumentationExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.DocumentationExperience;

            // Assert
            Assert.Equal(sumExp, 13);
        }

        /// <summary>
        /// Description: Verifies that the combined business experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void BusinessExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.BusinessExperience;

            // Assert
            Assert.Equal(sumExp, 17);
        }

        /// <summary>
        /// Description: Verifies that the combined creativity experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void CreativityExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var sumExp = team.CreativityExperience;

            // Assert
            Assert.Equal(sumExp, 11);
        }



        
        /// <summary>
        /// Description: Verifies that the average hackathon experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgHackathonExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgHackathonExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 1.80);
        }

        /// <summary>
        /// Description: Verifies that the average coding experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgCodingExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgCodingExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 2.80);
        }

        /// <summary>
        /// Description: Verifies that the average communication experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgCommunicationExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgCommunicationExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 3.20);
        }

        /// <summary>
        /// Description: Verifies that the average organization experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgOrganizationExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgOrganizationExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 3.00);
        }

        /// <summary>
        /// Description: Verifies that the average documentation experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgDocumentationExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgDocumentationExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 2.60);
        }

        /// <summary>
        /// Description: Verifies that the average business experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgBusinessExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgBusinessExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 3.40);
        }

        /// <summary>
        /// Description: Verifies that the average creativity experience for the team is calculated correctly.
        /// </summary> 
        [Fact]
        public void AvgCreativityExperience()
        {
            // Define
            var team = SampleData.SampleTeam_AvgsSums;

            // Process
            var avgExp = team.AvgCreativityExperience;

            // Assert
            Assert.Equal(Math.Round(avgExp, 2), 2.20);
        }
        #endregion
    }
}
