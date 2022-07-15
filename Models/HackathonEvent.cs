using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    public class HackathonEvent
    {
        [BsonId]
        public ObjectId Id {get; set;}
        
        [Required]
        [BsonElement("name")]
        public string Name { get; set; }

        [Required]
        [BsonElement("start_time")]
        public DateTime StartTime { get; set; }

        [Required]
        [BsonElement("end_time")]
        public DateTime EndTime { get; set; }

        [Required]
        [BsonElement("registration_settings")]
        public RegistrationSettings RegistrationSettings { get; set; }
    }
}
