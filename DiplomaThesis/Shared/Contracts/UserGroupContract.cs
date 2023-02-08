namespace DiplomaThesis.Shared.Contracts;

public class UserGroupContract
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; } = null!;
    public List<UserContract>? Users { get; init; } = null!;
}