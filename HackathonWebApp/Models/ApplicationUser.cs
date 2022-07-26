using AspNetCore.Identity.MongoDbCore.Models;
using System.ComponentModel.DataAnnotations;
using MongoDbGenericRepository.Attributes;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace HackathonWebApp.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        [Required(ErrorMessage = "First Name is required")]
        [BsonElement("FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [BsonElement("LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public override string Email {
            get { return base.Email; }
            set { base.Email = value; }
        }

        [Required]
        [BsonIgnore]
        public string Password { get; set; }
    }
}
