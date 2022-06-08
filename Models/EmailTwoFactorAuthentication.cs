using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HackathonWebApp.Models
{
    public class EmailTwoFactorAuthentication<TUser> : IUserTwoFactorTokenProvider<TUser> where TUser : ApplicationUser
    {
        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<TUser> manager, TUser user)
        {
            if (manager != null && user != null)
            {
                return Task.FromResult(true);
            }
            else
            {
                return Task.FromResult(false);
            }
        }

        // Genereates a simple token based on the user id, email and another string.
        private string GenerateToken(ApplicationUser user, string purpose)
        {
            using (MD5 md5 = MD5.Create())
            {
                string sourceString = user.Email + purpose + user.Id;
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(sourceString));
                string secretString = new Guid(hash).ToString();
                return secretString;
            }
        }

        public Task<string> GenerateAsync(string purpose, UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(GenerateToken(user, purpose));
        }

        public Task<bool> ValidateAsync(string purpose, string token, UserManager<TUser> manager, TUser user)
        {
            return Task.FromResult(token == GenerateToken(user, purpose));
        }
    }
}
