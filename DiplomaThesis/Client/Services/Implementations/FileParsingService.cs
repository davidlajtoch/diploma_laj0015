using System.Data;
using System.Text;
using DiplomaThesis.Client.Services.Interfaces;
using Newtonsoft.Json;

namespace DiplomaThesis.Client.Services.Implementations;

public class FileParsingService : IFileParsingService
{
    public string ParseToJson(string datasetFile, string extension)
    {
        return extension.ToLower() switch
        {
            "json" => datasetFile,
            "csv" => ParseCsvToJson(datasetFile),
            "xlsx" => ParseXlsxToJson(datasetFile),
            _ => throw new NotImplementedException()
        };
    }

    private static string ParseCsvToJson(string datasetFile)
    {
        datasetFile = datasetFile.ReplaceLineEndings();
        var rows = datasetFile.Split("\n");
        var columnNames = rows[0].Split(",");

        var sb = new StringBuilder("[\n");

        for (var i = 1; i < rows.Length; i++)
        {
            sb.AppendLine("{");

            for (var j = 0; j < columnNames.Length; j++)
            {
                var row = rows[i].Split(",");
                sb.AppendLine("\"" + columnNames[j] + "\": \"" + row[j] + "\",");
            }

            sb.Remove(sb.Length - 1, 1);
            sb.AppendLine("\n},");
        }

        sb.Remove(sb.Length - 1, 1);
        sb.AppendLine("\n]");

        return sb.ToString();
    }

    public DataTable ParseJsonToDataTable(string json)
    {
        return JsonConvert.DeserializeObject<DataTable>(json);
    }

    private static string ParseXlsxToJson(string datasetFile)
    {
        throw new NotImplementedException();
    }
}