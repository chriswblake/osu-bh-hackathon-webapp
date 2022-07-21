using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackathonWebApp.Models;
 
namespace HackathonWebApp.TagHelpers
{
    [HtmlTargetElement("td", Attributes = "i-user")]
    public class UserRolesTH : TagHelper
    {
        // Fields
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;

        // Constructors
        public UserRolesTH(UserManager<ApplicationUser> usermgr, RoleManager<ApplicationRole> rolemgr)
        {
            userManager = usermgr;
            roleManager = rolemgr;
        }

        // Properties
        [HtmlAttributeName("i-user")]
        public string UserID { get; set; }

        // Methods
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            List<string> roles = new List<string>();
            ApplicationUser user = await userManager.FindByIdAsync(UserID);
            if (user != null)
            {
                foreach (var roleID in user.Roles)
                {
                    if (roleID != null)
                    {
                        ApplicationRole role = roleManager.FindByIdAsync(roleID.ToString()).Result;
                        roles.Add(role.Name);
                    }
                }
            }
            output.Content.SetContent(roles.Count == 0 ? "No Roles" : string.Join(", ", roles));
        }
    }
}