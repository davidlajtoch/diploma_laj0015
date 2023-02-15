namespace DiplomaThesis.Shared.Commands;

public class CreateAssignmentCommand
{
    public string Name { get; init; } = null!;
    public Guid UserGroupId { get; init; } = Guid.Empty;
}