namespace DiplomaThesis.Shared.Commands;

public class DeleteAssignmentCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
}