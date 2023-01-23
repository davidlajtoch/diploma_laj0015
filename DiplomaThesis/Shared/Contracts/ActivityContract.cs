namespace DiplomaThesis.Shared.Contracts;

public class ActivityContract
{
    public Guid Id { get; set; }
    public string Message { get; init; }
    public UserGroupContract? UserGroup { get; init; }
    public DateTime? Created { get; init; }
}