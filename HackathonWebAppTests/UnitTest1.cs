using System;
using Xunit;
using HackathonWebApp.Models;


namespace HackathonWebAppTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Define


            // Process
            var user = new User()
            {
                Name = "Christopher",
                Email = "chriswblake@gmail.com"
            };

            // Assert
            Assert.Equal("Christopher", user.Name);
            Assert.Equal("chriswblake@gmail.com", user.Email);

        }
    }
}
