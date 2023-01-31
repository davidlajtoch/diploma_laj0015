using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Shared.Commands;

public class ActivityCommand
{
    public string Message { get; init; }
    public Guid? ObjectId1 { get; init; } = null!;
    public Guid? ObjectId2 { get; init; } = null!;
    public string? ObjectName1 { get; init; } = null;
    public string? ObjectName2 { get; init; } = null;
    public Guid? UserGroupId { get; init; }
}