namespace DiplomaThesis.Shared.Commands;

public class AddRoleCommand
{
    public Guid UserId { get; init; }
    public string RoleName { get; init; } = null!;
}