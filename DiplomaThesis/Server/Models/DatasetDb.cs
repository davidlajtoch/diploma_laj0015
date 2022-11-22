namespace DiplomaThesis.Server.Models;

public class DatasetDb
{
    public Guid Id { get; set; }
    public Guid PowerBiId { get; set; }
    public IEnumerable<string> ColumnNames { get; set; } = null!;
    public IEnumerable<string> ColumnTypes { get; set; } = null!;
}