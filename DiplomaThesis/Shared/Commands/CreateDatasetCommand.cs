namespace DiplomaThesis.Shared.Commands;

public class CreateDatasetCommand
{
    public string Name { get; init; } = null!;
    public IEnumerable<string> Columns { get; init; } = null!;
}