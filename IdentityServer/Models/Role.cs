using Microsoft.AspNetCore.Identity;

namespace IdentityServer.Models
{
    public class Role : IdentityRole<int>
    {
        //public string Name { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }

    }
}
