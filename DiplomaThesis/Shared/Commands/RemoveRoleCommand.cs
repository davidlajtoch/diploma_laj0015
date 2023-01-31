namespace DiplomaThesis.Shared.Commands;

public class RemoveRoleCommand
{
    public Guid UserId { get; init; }
    public string RoleName { get; init; } = null!;
}