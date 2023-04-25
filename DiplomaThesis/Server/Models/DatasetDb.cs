namespace DiplomaThesis.Server.Models;

public class DatasetDb
{
    public Guid Id { get; set; }
    public Guid PowerBiId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime DateUpdated { get; set; }
    public int NumberOfRows { get; set; }
    public List<string> ColumnNames { get; set; } = null!;
    public List<string> ColumnTypes { get; set; } = null!;
}