using System;
using Xunit;
using MongoDB.Bson;
using HackathonWebApp.Models;


namespace HackathonWebAppTests
{
    public class SponsorTests
    {
        /// <summary>
        /// Description: Verifies that sponsor details are not modified after being set.
        /// </summary> 
        [Fact]
        public void Sponsor_Set_Properties()
        {
            // Define

            // Process
            var sponsor = new Sponsor()
            {
                Id = ObjectId.Parse("123412341234123412341234"),
                Name = "Google",
                Tier = "Gold",
                WebsiteUrl = "https://google.com",
                Logo = "some 64 encoding of an image"
            };

            // Assert
            Assert.Equal(ObjectId.Parse("123412341234123412341234"), sponsor.Id);
            Assert.Equal("Google", sponsor.Name);
            Assert.Equal("Gold", sponsor.Tier);
            Assert.Equal("https://google.com", sponsor.WebsiteUrl);
            Assert.Equal("some 64 encoding of an image", sponsor.Logo);

        }
    }
}
