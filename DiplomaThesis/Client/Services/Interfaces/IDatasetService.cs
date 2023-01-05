using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IDatasetService
{
    public Task<List<DatasetContract>?> GetDatasets();
    public Task<bool> UploadNewDataset(string datasetName, string datasetJson);
    public Task<bool> UploadRowsToDataset(Guid datasetId, string datasetJson);
}