namespace DiplomaThesis.Shared.Commands;

public class UpdateUserGroupLeaderCommand
{
    public Guid UserId { get; init; }
    public Guid UserGroupId { get; init; }
}