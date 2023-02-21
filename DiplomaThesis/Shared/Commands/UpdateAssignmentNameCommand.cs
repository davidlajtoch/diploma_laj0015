namespace DiplomaThesis.Shared.Commands;

public class UpdateAssignmentNameCommand
{
    public Guid AssignmentId { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
}