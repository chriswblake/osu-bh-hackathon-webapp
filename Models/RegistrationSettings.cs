using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace HackathonWebApp.Models
{
    public class RegistrationSettings
    {
        /// <summary>
        /// Class: Represents the options for fields during registration of a participant.
        /// </summary>

        // Constructor
        public RegistrationSettings()
        {
            this.MajorOptions = new Dictionary<string, string>();
            this.TrainingsAcquiredOptions = new Dictionary<string, string>();
            this.TShirtSizeOptions = new Dictionary<string, string>();
        }

        // Schooling Fields
        /// <summary>
        /// Description: Key-Value pairs representing student academic majors.
        /// <para>Key: A normalized value for storage/lookup.</para>
        /// <para>Value: A human-friendly name, typically used for presentation.</para>
        /// </summary>
        [Required]
        [BsonElement("major_options")]
        public Dictionary<string,string> MajorOptions {get; set;}

        /// <summary>
        /// Description: Key-Value pairs representing year in university. (Fresh, Soph, etc.)
        /// <para>Key: A normalized value for storage/lookup.</para>
        /// <para>Value: A human-friendly name, typically used for presentation.</para>
        /// </summary>        
        [Required]
        [BsonIgnore]
        [BsonElement("school_year_options")]
        public Dictionary<string,string> SchoolYearOptions { get {
            return new Dictionary<string, string>()
            {
                { "first_year", "Freshman (1st Year)"},
                { "second_year", "Sophomore (2nd Year)"},
                { "third_year", "Junior (3rd Year)"},
                { "fourth_year", "Senior (4hth Year)"},
                { "fifth_year", "Super Senior (5th Year)"},
                { "six_plus_years", "Senior++ (6+ Years)"},
                { "graduate_student", "Graduate Student"},
                { "phd_student", "PhD Student"}
            };
        }}
    

        // Experience Fields
        /// <summary>
        /// Description: Key-Value pairs representing official trainings or certificates.
        /// <para>Key: A normalized value for storage/lookup.</para>
        /// <para>Value: A human-friendly name, typically used for presentation.</para>
        /// </summary> 
        [Required]
        [BsonElement("trainings_acquired_options")]
        public Dictionary<string,string> TrainingsAcquiredOptions {get; set;}

        // Preferences Fields
        /// <summary>
        /// Description: Key-Value pairs representing available shirt sizes.
        /// <para>Key: A normalized value for storage/lookup.</para>
        /// <para>Value: A human-friendly name, typically used for presentation.</para>
        /// </summary> 
        [Required]
        [BsonElement("tshirt_size_options")]
        public Dictionary<string,string> TShirtSizeOptions {get; set;}
    
    }
}
