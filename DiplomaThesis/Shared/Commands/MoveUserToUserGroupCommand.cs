namespace DiplomaThesis.Shared.Commands;

public class MoveUserToUserGroupCommand
{
    public Guid UserGroupId { get; init; }
    public Guid UserId { get; init; }
}