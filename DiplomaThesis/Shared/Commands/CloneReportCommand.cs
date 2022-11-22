namespace DiplomaThesis.Shared.Commands;

public class CloneReportCommand
{
    public Guid ReportId { get; init; }
    public string? NewName { get; init; }
}