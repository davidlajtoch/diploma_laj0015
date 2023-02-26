using System.Data;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IFileParsingService
{
    public string ParseToJson(string datasetFile, string extension);
    public DataTable ParseJsonToDataTable(string json);
}