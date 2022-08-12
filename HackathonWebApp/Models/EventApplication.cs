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

        [Required]
        [BsonElement("confirmation_state")]
        public string ConfirmationState {get; set; }
        [BsonIgnore]
        public Dictionary<string,string> ConfirmationStateOptions {get {
            return new Dictionary<string,string> {
                {"unconfirmed","Unconfirmed"}, // They have applied to the event.
                {"request_sent","Request Sent"}, // We are about to assemble teams. Request confirmation that you are free.
                {"unassigned","Unassigned"}, // Applicant has confirmed but has not been assigned a team yet.
                {"assigned","Assigned"}, // Applicant has been assigned a team.
                {"cancelled","Cancelled"}, // Applicant did not confirm in time or rejected confirmation.
                {"no_email","No Email"}, // Applicant never verified their email.
            };
        }}



        // Create account during application for event
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
