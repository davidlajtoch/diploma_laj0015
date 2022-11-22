namespace DiplomaThesis.Shared.Commands;

public class RemoveUserFromUserGroupCommand
{
    public Guid UserGroupId { get; init; }
    public Guid UserId { get; init; }
}