namespace DiplomaThesis.Server.Models;

public class Assignment
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public int Urgency { get; set; } = 0;
    public int Step { get; set; } = 0;
    public Guid UserGroupId { get; set; }
    public ApplicationUser? User { get; set; }
}