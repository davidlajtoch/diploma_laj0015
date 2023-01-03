namespace DiplomaThesis.Shared.Contracts;

public class UserGroupContract
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public IEnumerable<Guid>? Users { get; init; } = null!;
}