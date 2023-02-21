namespace DiplomaThesis.Shared.Commands;

public class UpdateAssignmentDescriptionCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
    public string Description { get; init; } = string.Empty;
}