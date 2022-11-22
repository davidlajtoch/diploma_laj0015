namespace DiplomaThesis.Shared.Contracts;

public class DatasetContract
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public IEnumerable<string> ColumnNames { get; init; } = null!;
    public IEnumerable<string> ColumnTypes { get; init; } = null!;
}