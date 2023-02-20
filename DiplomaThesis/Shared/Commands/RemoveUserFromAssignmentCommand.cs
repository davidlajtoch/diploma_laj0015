namespace DiplomaThesis.Shared.Commands;

public class RemoveUserFromAssignmentCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
}