using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDbGenericRepository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using System;
using MongoDB.Bson;

namespace HackathonWebApp.Models
{
    [CollectionName("AwardCertificates")]
    public class AwardCertificate
    {
        //Tracking
        [BsonId]
        public ObjectId Id {get; set; }
        [BsonElement("user_id")]
        public string UserId { get; set; }
        [BsonElement("event_id")]
        public string EventId {get; set; }

        #region Certificate Receiver
        [Required(ErrorMessage = "First Name is required")]
        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [BsonElement("LastName")]
        public string LastName { get; set; }

        [Required]
        [BsonElement("award")]
        [BsonRepresentation(BsonType.String)]
        public AwardOption Award {get; set; }
        public enum AwardOption
        {
            [Description("First Place")]
            first_place,
            [Description("Second Place")]
            second_place,
            [Description("Third Place")]
            third_place,
            [Description("Fourth Place")]
            fourth_place,
            [Description("Fifth Place")]
            fifth_place,
            [Description("Participant")]
            participant
        }
        
        [Required]
        [BsonElement("creation_time")]
        public DateTime CreationTime { get; set; }
        #endregion

        #region Event Details
        [Required]
        [BsonElement("start_time")]
        public DateTime StartTime { get; set; }
        
        [Required]
        [BsonElement("end_time")]
        public DateTime EndTime { get; set; }
        
        [Required]
        [BsonElement("judges_count")]
        public int JudgesCount { get; set; }

        [Required]
        [BsonElement("participants_count")]
        public int ParticipantsCount { get; set; }
        #endregion

        #region Signatures
        [BsonElement("first_signature_name")]
        public string FirstSignatureName {get; set;}
        [BsonElement("first_signature_title")]
        public string FirstSignatureTitle {get; set;}
        [BsonElement("first_signature_organization")]
        public string FirstSignatureOrganization {get; set;}

        [BsonElement("second_signature_name")]
        public string SecondSignatureName {get; set;}
        [BsonElement("second_signature_title")]
        public string SecondSignatureTitle {get; set;}
        [BsonElement("second_signature_organization")]
        public string SecondSignatureOrganization {get; set;}
        #endregion
    }

    
}


