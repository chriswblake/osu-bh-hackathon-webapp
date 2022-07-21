using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Threading.Tasks;
using HackathonWebApp.Models;
 
namespace HackathonWebApp.TagHelpers
{
    [HtmlTargetElement("td", Attributes = "i-role")]
    public class RoleUsersTH : TagHelper
    {
        // Fields
        private UserManager<ApplicationUser> userManager;
        private RoleManager<ApplicationRole> roleManager;

        // Constructors
        public RoleUsersTH(UserManager<ApplicationUser> usermgr, RoleManager<ApplicationRole> rolemgr)
        {
            userManager = usermgr;
            roleManager = rolemgr;
        }

        // Properties
        [HtmlAttributeName("i-role")]
        public string Role { get; set; }

        // Methods
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            List<string> names = new List<string>();
            ApplicationRole role = await roleManager.FindByIdAsync(Role);
            if (role != null)
            {
                foreach (var user in userManager.Users)
                {
                    if (user != null && await userManager.IsInRoleAsync(user, role.Name))
                        names.Add(user.UserName);
                }
            }
            output.Content.SetContent(names.Count == 0 ? "No Users" : string.Join(", ", names));
        }
    }
}