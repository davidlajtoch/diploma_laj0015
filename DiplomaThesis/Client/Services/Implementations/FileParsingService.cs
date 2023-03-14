using System.Data;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
using DiplomaThesis.Client.Services.Interfaces;
using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Newtonsoft.Json;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest.Serialization;
using System.Globalization;

namespace DiplomaThesis.Client.Services.Implementations;

public class FileParsingService : IFileParsingService
{
    private int _datasetFileMaxSize = 500 * 1024 * 1024; //500MB

    public async Task<string> ParseFileToJson(IBrowserFile datasetFile, string extension)
    {
        return extension.ToLower() switch
        {
            "json" => await ReadJson(datasetFile),
            "csv" => await ParseCsvToJson(datasetFile),
            "xlsx" => await ParseXlsxToJson(datasetFile),
            _ => throw new NotImplementedException()
        };
    }

    public async Task<string> ReadJson(IBrowserFile datasetFile)
    {
        return await new StreamReader(datasetFile.OpenReadStream(maxAllowedSize: _datasetFileMaxSize)).ReadToEndAsync();
    }

    public async Task<string> ParseCsvToJson(IBrowserFile datasetFile)
    {
        var datasetFileContent = await new StreamReader(datasetFile.OpenReadStream(maxAllowedSize: _datasetFileMaxSize)).ReadToEndAsync();

        datasetFileContent = datasetFileContent.ReplaceLineEndings();
        var rows = datasetFileContent.Split("\n");
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

    public async Task<string> ParseXlsxToJson(IBrowserFile datasetFile)
    {
        var sb = new StringBuilder("[\n");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        MemoryStream ms = new MemoryStream();
        await datasetFile.OpenReadStream().CopyToAsync(ms);

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

    public async Task<DataTable> ParseJsonToDataTable(string json)
    {
        return JsonConvert.DeserializeObject<DataTable>(json);
    }

    public static bool ChangeColumnDataType(DataTable table, string columnname, Type newtype)
    {
        if (table.Columns.Contains(columnname) == false)
            return false;

        DataColumn column = table.Columns[columnname];
        if (column.DataType == newtype)
            return true;

        try
        {
            DataColumn newcolumn = new DataColumn("temporary", newtype);
            table.Columns.Add(newcolumn);

            foreach (DataRow row in table.Rows)
            {
                try
                {
                    if (newtype == typeof(double))
                    {
                        row["temporary"] = (decimal)Convert.ChangeType(row[columnname], typeof(decimal), CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        row["temporary"] = Convert.ChangeType(row[columnname], newtype);
                    }

                }
                catch { }
            }
            newcolumn.SetOrdinal(column.Ordinal);
            table.Columns.Remove(columnname);
            newcolumn.ColumnName = columnname;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

}