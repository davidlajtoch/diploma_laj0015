namespace DiplomaThesis.Shared.Commands;

public class UpdateAssignmentUrgencyCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
    public int Urgency { get; init; } = 0;
}