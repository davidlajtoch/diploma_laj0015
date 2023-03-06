using Microsoft.AspNetCore.Components.Forms;
using System.Data;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IFileParsingService
{
    public Task<string> ParseFileToJson(IBrowserFile datasetFile, string extension);

    public Task<string> ReadJson(IBrowserFile datasetFile);
    public Task<string> ParseCsvToJson(IBrowserFile datasetFile);
    public Task<string> ParseXlsxToJson(IBrowserFile datasetFile);
    public DataTable ParseJsonToDataTable(string json);
}