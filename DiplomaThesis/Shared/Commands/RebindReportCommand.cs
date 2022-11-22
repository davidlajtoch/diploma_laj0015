namespace DiplomaThesis.Shared.Commands;

public class RebindReportCommand
{
    public Guid ReportId { get; init; }
    public Guid DatasetId { get; init; }
}