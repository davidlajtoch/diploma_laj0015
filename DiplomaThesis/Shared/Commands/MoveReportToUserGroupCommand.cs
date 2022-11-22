namespace DiplomaThesis.Shared.Commands;

public class MoveReportToUserGroupCommand
{
    public Guid UserGroupId { get; init; }
    public Guid ReportId { get; init; }
}