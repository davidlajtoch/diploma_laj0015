using Microsoft.AspNetCore.Identity;

namespace DiplomaThesis.Server.Models;

public class ApplicationUser : IdentityUser
{
    public Guid? UserGroupId { get; set; }
    public UserGroup? UserGroup { get; set; }
}