using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace HackathonWebApp.Models
{
    public class RoleEdit
    {
        public ApplicationRole Role { get; set; }
        public IEnumerable<ApplicationUser> Members { get; set; }
        public IEnumerable<ApplicationUser> NonMembers { get; set; }
    }
}
