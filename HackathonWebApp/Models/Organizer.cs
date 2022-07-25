using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace HackathonWebApp.Models
{
    public class Organizer
    {
        [BsonId]
        public ObjectId Id {get; set;}

        [BsonElement("profile_image")]
        public string ProfileImage { get; set; }

        [Required]
        [BsonElement("display_name")]
        public string DisplayName { get; set; }

        [Required]
        [BsonElement("role")]
        public string Role { get; set; }

        [Required]
        [BsonElement("job_title")]
        public string JobTitle { get; set; }

        [Required]
        [BsonElement("display_team")]
        public string DisplayTeam { get; set; }

        [BsonElement("linkedin_url")]
        public string LinkedInUrl { get; set; }

        [BsonElement("github_url")]
        public string GitHubUrl { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email")]
        public string Email { get; set; }
    }
}
