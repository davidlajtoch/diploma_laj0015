namespace DiplomaThesis.Shared.Commands;

public class RemoveReportFromUserGroupCommand
{
    public Guid UserGroupId { get; init; }
    public Guid ReportId { get; init; }
}