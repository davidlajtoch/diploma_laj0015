using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Shared.Commands;

public class ActivityCommand
{
    public string Message { get; init; }
    public Guid? ObjectId1 { get; init; }
    public Guid? ObjectId2 { get; init; }
    public Guid? UserGroupId { get; init; }
}