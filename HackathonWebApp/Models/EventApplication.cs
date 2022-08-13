using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace HackathonWebApp.Models
{
    [BsonIgnoreExtraElements]
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
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreatedOn {get; set; }

        [Required]
        [BsonElement("confirmation_state")]
        [BsonRepresentation(BsonType.String)]
        public ConfirmationStateOption ConfirmationState {get; set; }
        public enum ConfirmationStateOption
        {
            [Description("Unconfirmed")] // They have applied to the event.
            unconfirmed,
            [Description("Request Sent")] // We are about to assemble teams. Request confirmation that you are free.
            request_sent,
            [Description("Unassigned")] // Applicant has confirmed but has not been assigned a team yet.
            unassigned,
            [Description("Assigned")] // Applicant has been assigned a team.
            assigned,
            [Description("Cancelled")] // Applicant did not confirm in time or rejected confirmation.
            cancelled,
            [Description("No Email")] // Applicant never verified their email.
            no_email
        }



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
