namespace DiplomaThesis.Shared.Contracts;

public class ActivityContract
{
    public Guid Id { get; set; }
    public string Message { get; init; }
    public DateTime? Created { get; init; }
    public string? UserGroupName { get; init; }
    public Guid? UserGroupId { get; init; }
    
}