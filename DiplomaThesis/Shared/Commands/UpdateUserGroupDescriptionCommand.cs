namespace DiplomaThesis.Shared.Commands;

public class UpdateUserGroupDescriptionCommand
{
    public Guid UserGroupId { get; init; }
    public string Description { get; init; } = null!;
}