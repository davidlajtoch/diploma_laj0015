namespace DiplomaThesis.Shared.Contracts;

public class UserContract
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public Guid UserGroupId { get; set; }
    public List<RoleContract> Roles { get; init; } = null!;
}