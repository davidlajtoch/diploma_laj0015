namespace DiplomaThesis.Shared.Commands;

public class AddUserToAssignmentCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
    public Guid UserId { get; init; } = Guid.Empty;
}