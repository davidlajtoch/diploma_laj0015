namespace DiplomaThesis.Shared.Commands;

public class UpdateAssignmentStepCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
    public int ByValue { get; init; } = 0;
}