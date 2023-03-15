namespace DiplomaThesis.Server.Models;

public class DatasetRow
{
    public Guid Id { get; set; }
    public Guid DatasetPowerBiId { get; set; }
    public List<string> RowData { get; set; } = null!;
}