
using System.Data;
using System.Text;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using System.IO;

namespace DiplomaThesis.Server.Services;
public class FileParsingService
{
    private int _datasetFileMaxSize = 500 * 1024 * 1024; //500MB

    public async Task<string> ParseFileToJson(string datasetFilePath, string extension)
    {

        return extension.ToLower() switch
        {
            "json" => await ReadJson(datasetFilePath),
            "csv" => await ParseCsvToJson(datasetFilePath),
            "xlsx" => await ParseXlsxToJson(datasetFilePath),
            _ => throw new NotImplementedException()
        };
    }

    public async Task<string> ReadJson(string datasetFilePath)
    {
        string fileContent;
        using (StreamReader streamReader = new StreamReader(datasetFilePath))
        {
            fileContent = await streamReader.ReadToEndAsync();
        }
        return fileContent;
    }

    public async Task<string> ParseCsvToJson(string datasetFilePath)
    {
        string fileContent;
        using (StreamReader streamReader = new StreamReader(datasetFilePath))
        {
            fileContent = await streamReader.ReadToEndAsync();
        }

        fileContent = fileContent.ReplaceLineEndings();
        var rows = fileContent.Split("\n");
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

    public async Task<string> ParseXlsxToJson(string datasetFilePath)
    {
        var sb = new StringBuilder("[\n");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        MemoryStream ms = new MemoryStream();
        using (FileStream fs = File.OpenRead(datasetFilePath))
        {
            await fs.CopyToAsync(ms);
        }

        using (var reader = ExcelReaderFactory.CreateReader(ms, new ExcelReaderConfiguration()
        { FallbackEncoding = Encoding.GetEncoding(1252) }))
        {
            List<string> columnNames = new List<string>();
            reader.Read();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                columnNames.Add(reader.GetString(i));
            }

            while (reader.Read())
            {
                sb.AppendLine("{");
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    sb.AppendLine("\"" + columnNames[i] + "\": \"" + reader.GetString(i) + "\",");
                }
                sb.Remove(sb.Length - 1, 1);
                sb.AppendLine("\n},");

            } while (reader.NextResult()) ;
        }

        sb.Remove(sb.Length - 1, 1);
        sb.AppendLine("\n]");
        return sb.ToString();
    }
}