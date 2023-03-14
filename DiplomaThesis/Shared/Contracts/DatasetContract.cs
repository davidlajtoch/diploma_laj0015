namespace DiplomaThesis.Shared.Contracts;

public class DatasetContract
{
    public Guid Id { get; init; }
    public Guid PowerBiId { get; init; }
    public string Name { get; init; } = null!;
    public List<string> ColumnNames { get; init; } = null!;
    public List<string> ColumnTypes { get; init; } = null!;
}