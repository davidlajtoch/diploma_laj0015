using System.Net.Http.Json;
using System.Text;
using DiplomaThesis.Client.Extensions;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DiplomaThesis.Client.Services.Implementations;

public class DatasetService : IDatasetService
{
    private readonly HttpClient _http;

    public DatasetService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<DatasetContract>?> GetDatasets()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<DatasetContract>?>("Dataset/GetDatasetsAll");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<bool> UploadNewDataset(string datasetName, string datasetJson)
    {
        try
        {
            var content = new StringContent(datasetJson, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"Dataset/UploadNewDataset/{datasetName}", content);

            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> UploadRowsToDataset(Guid datasetId, string datasetJson)
    {
        try
        {
            var content = new StringContent(datasetJson, Encoding.UTF8, "application/json");
            var response = await _http.PostAsync($"Dataset/UploadRowsToDataset/{datasetId}", content);

            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> DeleteDataset(Guid datasetPowerBiId)
    {
        try
        {
            var response = await _http.DeleteAsJsonAsync(
                "Dataset/DeleteDataset",
                new DeleteDatasetCommand { PowerBiId = datasetPowerBiId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }
}