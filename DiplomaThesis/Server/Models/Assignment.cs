namespace DiplomaThesis.Server.Models;

public class Assignment
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; } 
    public UserGroup? UserGroup { get; set; }
    public ApplicationUser? User { get; set; }
}