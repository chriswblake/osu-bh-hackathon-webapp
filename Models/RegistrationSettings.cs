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
        [BsonIgnore]
        [BsonElement("hackathon_experience_options")]
        public Dictionary<int,string> HackathonExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                {0, "Zero. This would be my first. 😋"},
                {1, "Familiar. I've read a stuff and want to try. 🤔"},
                {2, "Tried it once before. I want to try again. 🤓"},
                {3, "A few. I'm sorta liking these. 🤓"},
                {4, "I've been in several. It's fun! 😎"},
                {5, "I've helped organize them! 🦸‍♀️"}
            };
        }}

        [Required]
        [BsonIgnore]
        [BsonElement("coding_experience_options")]
        public Dictionary<int,string> CodingExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                {0, "No doubt, I am a Youngling (no experience)"},
                {1, "I'm definitely a Padawan (please teach me)"},
                {2, "As a Jedi Knight, I got skills (can do stuff)"},
                {3, "I'm a Jedi Master (ready for anything)"},
                {4, "Experienced Jedi Master (worked on team projects)"},
                {5, "Jedi Trainer (able to teach others)"}
            };
        }}

        [Required]
        [BsonIgnore]
        [BsonElement("coding_experience_options")]
        public Dictionary<int,string> CommunicationExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                { 0, "Just tell me what to do. I'll get it done, alone. 👩‍💻"},
                { 1, "Prefer alone or with people I know. 😎"},
                { 2, "I can get the team on the same page. 🧐"},
                { 3, "Presentations are fun. 👩‍🏫"},
                { 4, "I've been on stage infront of 100's of people. 🕺"},
                { 5, "Charisma is my thing! 🤩"}
            };
        }}

        [Required]
        [BsonIgnore]
        [BsonElement("coding_experience_options")]
        public Dictionary<int,string> OrganizationExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                { 0, "All my files are named 'temp123'. 🚮"},
                { 1, "All my files have a folder and legit name. 🗃"},
                { 2, "I copy the folder to backup my code/work. 🎁"},
                { 3, "I use GitHub. 🔓"},
                { 4, "I can use branches on GitHub!🎋"},
                { 5, "I'm probably too organized... actually, haha.🕸"}
            };
        }}

        [Required]
        [BsonIgnore]
        [BsonElement("documentation_experience_options")]
        public Dictionary<int,string> DocumentationExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                { 0, "Instructions! I don't need those...🙈"},
                { 1, "Instuctions...they're more like guidelines.🧻"},
                { 2, "I usually follow the instuctions.📄"},
                { 3, "I ♥ documentation. (stack overflow 😅)"},
                { 4, "I like to fix mistakes and comment on docs.✏"},
                { 5, "It ain't done unless there are docs. 🧐"},
            };
        }}

        [Required]
        [BsonIgnore]
        [BsonElement("business_experience_options")]
        public Dictionary<int,string> BusinessExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                { 0, "Work? I prefer to avoid that... ugh. 😝"},
                { 1, "I've never worked before. 👼"},
                { 2, "I've contributed to a team at work. 👩‍🔧"},
                { 3, "I've led a project at work. 🕵️‍♀️"},
                { 4, "I've managed a team at work. 🥋"},
                { 5, "Actually, I have my own business. 💼"}
            };
        }}

        [Required]
        [BsonIgnore]
        [BsonElement("creativity_experience_options")]
        public Dictionary<int,string> CreativityExperienceOptions { get {
            return new Dictionary<int, string>()
            {
                {0, "I'm a task-doer robot. 🤖"},
                {1, "I appreciate art. 🖼"},
                {2, "Cool stuff comes from brainstorming! 🦄"},
                {3, "I tend to get distracted by new ideas. ✨"},
                {4, "I garden multiple ideas at the same time. 🌱"},
                {5, "Life is a pickle, but at least it's spicy. 🎭"}
            };
        }}
        
        // Trainging Fields
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
