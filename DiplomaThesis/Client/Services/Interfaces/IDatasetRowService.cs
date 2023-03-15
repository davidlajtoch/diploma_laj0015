using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IDatasetRowService
{
    public Task<List<List<string>>?> GetDatasetRowsByDatasetId(Guid datasetId);
}