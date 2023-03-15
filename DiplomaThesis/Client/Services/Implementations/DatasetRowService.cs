using System.Net.Http.Json;
using System.Text;
using DiplomaThesis.Client.Extensions;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DiplomaThesis.Client.Services.Implementations;

public class DatasetRowService : IDatasetRowService
{
    private readonly HttpClient _http;

    public DatasetRowService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<List<string>>?> GetDatasetRowsByDatasetId(Guid datasetId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<List<string>>?>($"DatasetRow/GetDatasetRowsByDatasetId/{datasetId}");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }
}