using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HackathonWebApp.Models
{
    public class EventApplication
    {
        // Tracking Info
        [BsonId]
        public ObjectId Id {get; set;}

        [Required]
        [BsonElement("user_id")]
        public Guid UserId { get; set; }
        
        [Required]
        [BsonElement("created_on")]
        public DateTime CreatedOn {get; set; }



        // Registration during application
        [BsonIgnore]
        public ApplicationUser AssociatedUser {get; set;}



        // Schooling Information
        [Required]
        [BsonElement("major")]
        public string Major { get; set; }

        [Required]
        [BsonElement("school_year")]
        public string SchoolYear {get; set;}



        // Skills Information
        [Required]
        [BsonElement("hackathon_experience")]
        [Range(0, 5)]
        public int HackathonExperience { get; set; }

        [Required]
        [BsonElement("coding_experience")]
        [Range(0, 5)]
        public int CodingExperience { get; set; }

        [Required]
        [BsonElement("communication_experience")]
        [Range(0, 5)]
        public int CommunicationExperience { get; set; }

        [Required]
        [BsonElement("organization_experience")]
        [Range(0, 5)]
        public int OrganizationExperience { get; set; }

        [Required]
        [BsonElement("documentation_experience")]
        [Range(0, 5)]
        public int DocumentationExperience { get; set; }

        [Required]
        [BsonElement("business_experience")]
        [Range(0, 5)]
        public int BusinessExperience { get; set; }

        [Required]
        [BsonElement("creativity_experience")]
        [Range(0, 5)]
        public int CreativityExperience { get; set; }



        // Trainings
        [BsonElement("trainings_acquired")]
        public List<string> TrainingsAcquired { get; set; }



        // Customization Information
        [Required]
        [BsonElement("tshirt_size")]
        public string TShirtSize { get; set; }

        [BsonElement("dietary_restrictions")]
        public string DietaryRestrictions { get; set; }
    }
}
