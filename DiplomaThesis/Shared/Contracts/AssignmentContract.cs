namespace DiplomaThesis.Shared.Contracts;

public class AssignmentContract
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? Created { get; set; }
    public int Urgency { get; set; } = 0;
    public int Step { get; set; } = 0;
    public Guid UserGroupId { get; set; } = Guid.Empty;
    public UserContract? User { get; set; }
    
}