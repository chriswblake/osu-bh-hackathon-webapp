using System;
using System.Linq;
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
                var hackathonEvent = new HackathonEvent();
                // A team is added to the hackathon event.
                var team = new Team() { Id = ObjectId.GenerateNewId(), ReferenceEvent=hackathonEvent };
                hackathonEvent.Teams.Add(team.Id.ToString(), team);

                // Users create accounts
                var user1 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_1" };
                var user2 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_2" };
                var user3 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_3" };
                var user4 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_4" };
                var user5 = new ApplicationUser() { Id = Guid.NewGuid(), UserName = "david_5" };

                // Users apply to hackathon event
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
                var eventApp3 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000003"), UserId = user3.Id,
                    HackathonExperience = 1,
                    CodingExperience = 1,
                    CommunicationExperience = 4,
                    OrganizationExperience = 5,
                    DocumentationExperience = 1,
                    BusinessExperience = 3,
                    CreativityExperience = 0
                };
                var eventApp4 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000004"), UserId = user4.Id,
                    HackathonExperience = 1,
                    CodingExperience = 3,
                    CommunicationExperience = 5,
                    OrganizationExperience = 3,
                    DocumentationExperience = 4,
                    BusinessExperience = 5,
                    CreativityExperience = 1
                };
                var eventApp5 = new EventApplication() { Id = ObjectId.Parse("000000000000000000000005"), UserId = user5.Id,
                    HackathonExperience = 4,
                    CodingExperience = 5,
                    CommunicationExperience = 4,
                    OrganizationExperience = 4,
                    DocumentationExperience = 5,
                    BusinessExperience = 4,
                    CreativityExperience = 5
                };
                hackathonEvent.EventApplications.Add(eventApp1.Id.ToString(), eventApp1);
                hackathonEvent.EventApplications.Add(eventApp2.Id.ToString(), eventApp2);
                hackathonEvent.EventApplications.Add(eventApp3.Id.ToString(), eventApp3);
                hackathonEvent.EventApplications.Add(eventApp4.Id.ToString(), eventApp4);
                hackathonEvent.EventApplications.Add(eventApp5.Id.ToString(), eventApp5);

                // Applications are assigned to a team
                hackathonEvent.EventAppTeams.Add(eventApp1.Id.ToString(), team.Id.ToString());
                hackathonEvent.EventAppTeams.Add(eventApp2.Id.ToString(), team.Id.ToString());
                hackathonEvent.EventAppTeams.Add(eventApp3.Id.ToString(), team.Id.ToString());
                hackathonEvent.EventAppTeams.Add(eventApp4.Id.ToString(), team.Id.ToString());
                hackathonEvent.EventAppTeams.Add(eventApp5.Id.ToString(), team.Id.ToString());

                return team;
            }}
        
            /// <summary>
            /// Description: This hackathon event is used for checking count, sum, and weight sum of scores.
            /// </summary>
            public static HackathonEvent SampleHackathonEvent_Scoring { get {
                // Create a hackathon
                var hackathonEvent = new HackathonEvent();
                // Add users
                var u1 = new ApplicationUser() {Id= Guid.NewGuid(), FirstName="MyJudge" };
                var u2 = new ApplicationUser() {Id= Guid.NewGuid(), FirstName="MyMentor" };
                var u3 = new ApplicationUser() {Id= Guid.NewGuid(), FirstName="Participant1" };
                var u4 = new ApplicationUser() {Id= Guid.NewGuid(), FirstName="Participant2" };
                // Add questions
                var q1 = new ScoreQuestion() { Id = ObjectId.Parse("000000000000000000000001"), PossiblePoints =  5 , Title="q1"};
                var q2 = new ScoreQuestion() { Id = ObjectId.Parse("000000000000000000000002"), PossiblePoints = 10 , Title="q2"};
                var q3 = new ScoreQuestion() { Id = ObjectId.Parse("000000000000000000000003"), PossiblePoints = 15 , Title="q3"};
                var q4 = new ScoreQuestion() { Id = ObjectId.Parse("000000000000000000000004"), PossiblePoints = 20 , Title="q4"};
                var q5 = new ScoreQuestion() { Id = ObjectId.Parse("000000000000000000000005"), PossiblePoints = 25 , Title="q5"};
                hackathonEvent.ScoringQuestions.Add(q1.Id.ToString(), q1);
                hackathonEvent.ScoringQuestions.Add(q2.Id.ToString(), q2);
                hackathonEvent.ScoringQuestions.Add(q3.Id.ToString(), q3);
                hackathonEvent.ScoringQuestions.Add(q4.Id.ToString(), q4);
                hackathonEvent.ScoringQuestions.Add(q5.Id.ToString(), q5);
                // Add Roles
                var r1 = new ScoringRole() {Name = "Judge", ScoringWeight = 0.4, Id=ObjectId.GenerateNewId() };
                var r2 = new ScoringRole() {Name = "Mentor", ScoringWeight = 0.1, Id=ObjectId.GenerateNewId() };
                var r3 = new ScoringRole() {Name = "Participant", ScoringWeight = 0.5, Id=ObjectId.GenerateNewId() };
                hackathonEvent.ScoringRoles[r1.Id.ToString()] = r1;
                hackathonEvent.ScoringRoles[r2.Id.ToString()] = r2;
                hackathonEvent.ScoringRoles[r3.Id.ToString()] = r3;
                // Assign users to roles
                hackathonEvent.UserScoringRoles[u1.Id.ToString()] = r1.Id.ToString();
                hackathonEvent.UserScoringRoles[u2.Id.ToString()] = r2.Id.ToString();
                hackathonEvent.UserScoringRoles[u3.Id.ToString()] = r3.Id.ToString();
                hackathonEvent.UserScoringRoles[u4.Id.ToString()] = r3.Id.ToString();
                // Add a team
                var team = new Team() { Id=ObjectId.Parse("000000000000000000000111"), ReferenceEvent=hackathonEvent};
                hackathonEvent.Teams.Add(team.Id.ToString(), team);
                // Add score submissions to the team
                var ss1 = new ScoringSubmission () { 
                    TeamId = team.Id.ToString(),
                    UserId = u1.Id.ToString(),
                    Scores = new System.Collections.Generic.Dictionary<string, int>() {
                        { q1.Id.ToString(), 2 },
                        { q2.Id.ToString(), 1 },
                        { q3.Id.ToString(), 3 }
                    }
                };
                team.ScoringSubmissions.Add(ss1.UserId, ss1);
                
                var ss2 = new ScoringSubmission () { 
                    TeamId = team.Id.ToString(),
                    UserId = u2.Id.ToString(),
                    Scores = new System.Collections.Generic.Dictionary<string, int>() {
                        { q2.Id.ToString(), 5 },
                        { q3.Id.ToString(), 5 },
                        { q4.Id.ToString(), 5 }
                    }
                };
                team.ScoringSubmissions.Add(ss2.UserId, ss2);
                
                var ss3 = new ScoringSubmission () { 
                    TeamId = team.Id.ToString(),
                    UserId = u3.Id.ToString(),
                    Scores = new System.Collections.Generic.Dictionary<string, int>() {
                        { q3.Id.ToString(), 1 },
                        { q4.Id.ToString(), 1 },
                        { q5.Id.ToString(), 1 }
                    }
                };
                team.ScoringSubmissions.Add(ss3.UserId, ss3);
                
                var ss4 = new ScoringSubmission () { 
                    TeamId = team.Id.ToString(),
                    UserId = u4.Id.ToString(),
                    Scores = new System.Collections.Generic.Dictionary<string, int>() {
                        { q3.Id.ToString(), 5 },
                        { q4.Id.ToString(), 5 },
                        { q5.Id.ToString(), 2 }
                    }
                };
                team.ScoringSubmissions.Add(ss4.UserId, ss4);

                return hackathonEvent;
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

            // Process
            var team = new Team()
            {
                Id = ObjectId.Parse("123412341234123412341234"),
                Name = "The Shindigs"
            };
            team.TeamMembers.Add(user1.Id, user1);

            // Assert
            Assert.Equal(ObjectId.Parse("123412341234123412341234"), team.Id);
            Assert.Equal("The Shindigs", team.Name);
            Assert.Equal(1, team.TeamMembers.Count);
        }

        #region Scoring
        /// <summary>
        /// Description: Verifies that multiple score submission across several questions can be properally counted.
        /// </summary> 
        [Fact]
        public void CountScoresByQuestionId() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];

            // Process
            var countScores = team.CountScoresByQuestionId;

            // Assert
            Assert.Equal(1, countScores["000000000000000000000001"]);
            Assert.Equal(2, countScores["000000000000000000000002"]);
            Assert.Equal(4, countScores["000000000000000000000003"]);
            Assert.Equal(3, countScores["000000000000000000000004"]);
            Assert.Equal(2, countScores["000000000000000000000005"]);
        }

        /// <summary>
        /// Description: Verifies that multiple score submission across several questions can be properally averaged.
        /// </summary> 
        [Fact]
        public void AvgUnweightedScoresByQuestionId() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];

            // Process
            var avgScores = team.AvgUnweightedScoresByQuestionId;

            // Assert
            Assert.Equal(2,             avgScores["000000000000000000000001"]);
            Assert.Equal((1+5)/2.0,     avgScores["000000000000000000000002"]);
            Assert.Equal((3+5+1+5)/4.0, avgScores["000000000000000000000003"]);
            Assert.Equal((5+1+5)/3.0,   avgScores["000000000000000000000004"]);
            Assert.Equal((1+2)/2.0,     avgScores["000000000000000000000005"]);
        }
        
        /// <summary>
        /// Description: Verifies that multiple score submission across several questions can be properally averaged, incorporating the weight of the question.
        /// </summary> 
        [Fact]
        public void AvgWeightedScoresByQuestionId() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];

            // Process
            var avgScores = team.AvgWeightedScoresByQuestionId;

            // Assert   score/max_score*PossiblePoints
            Assert.Equal(2              /5.0 *5,  avgScores["000000000000000000000001"]);
            Assert.Equal((1+5)/2.0      /5.0 *10, avgScores["000000000000000000000002"]);
            Assert.Equal((3+5+1+5)/4.0  /5.0 *15, avgScores["000000000000000000000003"]);
            Assert.Equal((5+1+5)/3.0    /5.0 *20, avgScores["000000000000000000000004"]);
            Assert.Equal((1+2)/2.0      /5.0 *25, avgScores["000000000000000000000005"]);
        }
        /// <summary>
        /// Description: Verifies that a rollup of all scoring for this team across all questions and their weights is valid.
        /// </summary> 
        [Fact]
        public void CombinedScore() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];

            // Process
            var score = team.CombinedScore;

            // Assert   score/max_score*PossiblePoints
            Assert.Equal(40.67, Math.Round(score, 2));
        }
        
        
        /// <summary>
        /// Description: Verifies that multiple score submission across several questions can be properally counted.
        /// </summary> 
        [Fact]
        public void WeightedScores() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];

            // Process
            var weightedScores = team.WeightedScores;

            // Assert
            Assert.Equal((2/5.0)*5*0.4,  weightedScores[0].CalculatedScore);
            Assert.Equal((1/5.0)*10*0.4, weightedScores[1].CalculatedScore);
            Assert.Equal((3/5.0)*15*0.4, weightedScores[2].CalculatedScore);
            Assert.Equal((5/5.0)*10*0.1, weightedScores[3].CalculatedScore);
            Assert.Equal((5/5.0)*15*0.1, weightedScores[4].CalculatedScore);
            Assert.Equal((5/5.0)*20*0.1, weightedScores[5].CalculatedScore);
            Assert.Equal((1/5.0)*15*0.5, weightedScores[6].CalculatedScore);
            Assert.Equal((1/5.0)*20*0.5, weightedScores[7].CalculatedScore);
            Assert.Equal((1/5.0)*25*0.5, weightedScores[8].CalculatedScore);
            Assert.Equal((5/5.0)*15*0.5, weightedScores[9].CalculatedScore);
            Assert.Equal((5/5.0)*20*0.5, weightedScores[10].CalculatedScore);
            Assert.Equal((2/5.0)*25*0.5, weightedScores[11].CalculatedScore);
        }
        
        [Fact]
        public void AvgWeightedScoresByQuestionGroupedByRole() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];
            // Get roles
            var roleJudge = hackathonEvent.ScoringRoles.Values.Where(r=> r.Name == "Judge").First();
            var roleMentor = hackathonEvent.ScoringRoles.Values.Where(r=> r.Name == "Mentor").First();
            var roleParticipant = hackathonEvent.ScoringRoles.Values.Where(r=> r.Name == "Participant").First();
            // Get questions
            var q1 = hackathonEvent.ScoringQuestions.Values.Where(q=> q.Title == "q1").First();
            var q2 = hackathonEvent.ScoringQuestions.Values.Where(q=> q.Title == "q2").First();
            var q3 = hackathonEvent.ScoringQuestions.Values.Where(q=> q.Title == "q3").First();
            var q4 = hackathonEvent.ScoringQuestions.Values.Where(q=> q.Title == "q4").First();
            var q5 = hackathonEvent.ScoringQuestions.Values.Where(q=> q.Title == "q5").First();

            // Process
            var avgScoresByRole = team.AvgWeightedScoresByQuestionGroupedByRole;
            var judgeScoresByQuestion = avgScoresByRole[roleJudge];
            var mentorScoresByQuestion = avgScoresByRole[roleMentor];
            var participantScoresByQuestion = avgScoresByRole[roleParticipant];

            // Assert - Structure
            Assert.Equal(3, avgScoresByRole.Count());

            Assert.Equal(3, judgeScoresByQuestion.Count());
            Assert.Equal(3, mentorScoresByQuestion.Count());
            Assert.Equal(3, participantScoresByQuestion.Count());

            Assert.True(judgeScoresByQuestion.ContainsKey(q1));
            Assert.True(judgeScoresByQuestion.ContainsKey(q2));
            Assert.True(judgeScoresByQuestion.ContainsKey(q3));

            Assert.True(mentorScoresByQuestion.ContainsKey(q2));
            Assert.True(mentorScoresByQuestion.ContainsKey(q3));
            Assert.True(mentorScoresByQuestion.ContainsKey(q4));

            Assert.True(participantScoresByQuestion.ContainsKey(q3));
            Assert.True(participantScoresByQuestion.ContainsKey(q4));
            Assert.True(participantScoresByQuestion.ContainsKey(q5));

            // Assert - Calculations
            Assert.Equal(0.8, judgeScoresByQuestion[q1]);
            Assert.Equal(0.8, judgeScoresByQuestion[q2]);
            Assert.Equal(3.6, judgeScoresByQuestion[q3]);

            Assert.Equal(1.0, mentorScoresByQuestion[q2]);
            Assert.Equal(1.5, mentorScoresByQuestion[q3]);
            Assert.Equal(2.0, mentorScoresByQuestion[q4]);

            Assert.Equal(4.5, participantScoresByQuestion[q3]);
            Assert.Equal(6, participantScoresByQuestion[q4]);
            Assert.Equal(3.75, participantScoresByQuestion[q5]);
        }
        
        [Fact]
        public void AvgWeightedScoresByQuestionId2() {
            // Define
            var hackathonEvent = SampleData.SampleHackathonEvent_Scoring;
            var team = hackathonEvent.Teams["000000000000000000000111"];

            // Process
            var avgScores = team.AvgWeightedScoresByQuestionId2;

            // Assert
            Assert.Equal(0.8,  avgScores["000000000000000000000001"]);
            Assert.Equal(1.8,  avgScores["000000000000000000000002"]);
            Assert.Equal(9.6,  avgScores["000000000000000000000003"]);
            Assert.Equal(8.0,  avgScores["000000000000000000000004"]);
            Assert.Equal(3.75, avgScores["000000000000000000000005"]);
        }
        #endregion

        # region Experience
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
