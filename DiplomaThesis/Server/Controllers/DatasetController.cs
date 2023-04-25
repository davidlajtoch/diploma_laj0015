using DiplomaThesis.Server.Data;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Server.Services;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;

namespace DiplomaThesis.Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class DatasetController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PowerBiService _service;

    private const int _datasetRowInsertLimit = 5;
    private const string _datasetServerFilesPath = @"..\Server\DatasetFiles";

    public DatasetController(PowerBiService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
    }

    private async Task<ActionResult> RecordActivity(string message, Guid? userGroupId)
    {
        var userGroup = await _context.UserGroups.FindAsync(userGroupId);

        Activity newActivity = new Activity
        {
            Message = message,
            Created = DateTime.Now,
            UserGroupName = (userGroup == null) ? null : userGroup.Name,
            UserGroupId = userGroupId
        };

        _context.Activities.Add(newActivity);

        return Ok();
    }

    [Authorize(Roles = "Architect")]
    [HttpGet("{datasetId}")]
    public async Task<ActionResult> GetDataset(
        [FromRoute] Guid datasetId
    )
    {
        var datasetInPowerBi = await _service.GetDataset(datasetId);
        if (datasetInPowerBi is null) return NotFound();

        var datasetsInDb = await _context.Datasets.ToListAsync();
        var datasetInDb = datasetsInDb.Find(d => d.PowerBiId.Equals(Guid.Parse(datasetInPowerBi.Id)));

        return Ok(new DatasetContract
        {
            Id = Guid.Parse(datasetInPowerBi.Id),
            Name = datasetInPowerBi.Name,
            DateUpdated = datasetInDb?.DateUpdated,
            NumberOfRows = datasetInDb?.NumberOfRows,
            ColumnNames = datasetInDb?.ColumnNames ?? new List<string>(),
            ColumnTypes = datasetInDb?.ColumnTypes ?? new List<string>()
        });
    }

    [Authorize(Roles = "Architect")]
    [HttpGet]
    public async Task<ActionResult> GetDatasetsAll()
    {
        var datasetsInPowerBi = await _service.GetDatasets();
        var datasetsInDb = await _context.Datasets.ToListAsync();

        var result = new List<DatasetContract>();

        foreach (var datasetInPowerBi in datasetsInPowerBi)
        {
            var datasetInDb = datasetsInDb.Find(d => d.PowerBiId.Equals(Guid.Parse(datasetInPowerBi.Id)));

            result.Add(new DatasetContract
            {
                Id = Guid.Parse(datasetInPowerBi.Id),
                PowerBiId = Guid.Parse(datasetInPowerBi.Id),
                Name = datasetInPowerBi.Name,
                DateUpdated = datasetInDb?.DateUpdated,
                NumberOfRows = datasetInDb?.NumberOfRows,
                ColumnNames = datasetInDb?.ColumnNames ?? new List<string>(),
                ColumnTypes = datasetInDb?.ColumnTypes ?? new List<string>()
            });
        }

        return Ok(result);
    }

    [Authorize(Roles = "Architect")]
    [HttpGet]
    public async Task<ActionResult> GetServerDatasetFileNames()
    {
        DirectoryInfo datasetFilesDirectory = new DirectoryInfo(_datasetServerFilesPath);
        FileInfo[] datasetFiles = datasetFilesDirectory.GetFiles("*.*");

        List<string> datasetFileNames = new List<string>();
        foreach (FileInfo file in datasetFiles)
        {
            datasetFileNames.Add(file.Name);
        }

        return Ok(datasetFileNames);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost]
    public async Task<ActionResult> CreateDataset(
        [FromBody] CreateDatasetCommand createDatasetCommand
    )
    {
        if (createDatasetCommand.Name.Length == 0 || !createDatasetCommand.Columns.Any()) return BadRequest();

        var result = await _service.CreateDataset(
            createDatasetCommand.Name,
            createDatasetCommand.Columns.Select(name => new Microsoft.PowerBI.Api.Models.Column(name, "string"))
        );

        if (result is null) return StatusCode(500);

        return Ok(result);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost("{datasetName}")]
    public async Task<ActionResult> UploadNewDataset(
        [FromRoute] string datasetName,
        [FromBody] List<object> rows
    )
    {
        if (datasetName.Length == 0 || rows.Count == 0) return BadRequest();

        var columns = new List<Microsoft.PowerBI.Api.Models.Column>();
        foreach (var element in ((JObject)rows[0]).Properties())
        {
            var elementValueString = element.Value.ToString();
            if (DateTime.TryParse(elementValueString, out _))
                columns.Add(new Microsoft.PowerBI.Api.Models.Column(element.Name, "DateTime"));
            else if (double.TryParse(elementValueString, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                columns.Add(new Microsoft.PowerBI.Api.Models.Column(element.Name, "Double"));
            else if (bool.TryParse(elementValueString, out _))
                columns.Add(new Microsoft.PowerBI.Api.Models.Column(element.Name, "Bool"));
            else
                columns.Add(new Microsoft.PowerBI.Api.Models.Column(element.Name, "String"));
        }

        var dataset = await _service.CreateDataset(datasetName, columns);
        if (dataset is null) return StatusCode(500);

        var result = await _service.PushRowsToDataset(Guid.Parse(dataset.Id), rows);

        if (result)
        {
            var datasetInDb = new DatasetDb
            {
                Id = Guid.NewGuid(),
                PowerBiId = Guid.Parse(dataset.Id),
                Name = dataset.Name,
                DateUpdated = DateTime.Now,
                NumberOfRows = rows.Count,
                ColumnNames = columns.Select(column => column.Name).ToList(),
                ColumnTypes = columns.Select(column => column.DataType).ToList()
            };

            for (int r = 0; r < _datasetRowInsertLimit; r++)
            {
                List<string> datasetRowData = new();
                foreach (var element in ((JObject)rows[r]).Values())
                {
                    datasetRowData.Add(element.ToString());
                }
                _context.DatasetRows.Add(new DatasetRow
                {
                    DatasetPowerBiId = datasetInDb.PowerBiId,
                    RowData = datasetRowData
                });
            }

            _context.Datasets.Add(datasetInDb);
            await RecordActivity("New dataset " + datasetName + " was created", null);
            await _context.SaveChangesAsync();
        }

        return result ? Ok() : StatusCode(500);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost("{datasetId}")]
    public async Task<ActionResult> UploadRowsToDataset(
        [FromRoute] Guid datasetId,
        [FromBody] List<object> rows
    )
    {
        var datasetPbi = await _service.GetDataset(datasetId);
        if (datasetPbi is null) return NotFound();

        var result = await _service.PushRowsToDataset(datasetId, rows);

        var datasetsInDb = await _context.Datasets.ToListAsync();
        var datasetInDb = datasetsInDb.Find(d => d.PowerBiId.Equals(datasetId));

        if (datasetInDb is null) return NotFound();

        datasetInDb.DateUpdated = DateTime.Now;
        datasetInDb.NumberOfRows += rows.Count;

        await RecordActivity("Rows were added to dataset " + datasetPbi.Name, null);
        _context.SaveChanges();

        return result ? Ok() : StatusCode(500);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost]
    public async Task<ActionResult> UploadRowsToDatasetByServerFileIndex(
    [FromBody] int datasetFileIndex
    )
    {
        DirectoryInfo datasetFilesDirectory = new DirectoryInfo(_datasetServerFilesPath);
        FileInfo datasetFile = datasetFilesDirectory.GetFiles("*.*").ToList()[datasetFileIndex];

        FileParsingService fileParsingService = new FileParsingService();
        string datasetFileJson = await fileParsingService.ParseFileToJson(datasetFile.FullName, datasetFile.Extension[1..]);
        List<object> datasetRows = JsonConvert.DeserializeObject<List<object>>(datasetFileJson);

        if (datasetRows == null) return BadRequest();

        string datasetName = datasetFile.Name.Split('.')[0];

        var datasetInDb = _context.Datasets.Where(d => d.Name == datasetName).FirstOrDefault();
        if (datasetInDb is null) return NotFound();

        var datasetPowerBi = await _service.GetDataset(datasetInDb.PowerBiId);
        if (datasetPowerBi is null) return NotFound();

        var result = await _service.PushRowsToDataset(Guid.Parse(datasetPowerBi.Id), datasetRows);

        if (result)
        {
            await RecordActivity("Rows were added to dataset " + datasetPowerBi.Name, null);
            datasetInDb.DateUpdated = DateTime.Now;
            datasetInDb.NumberOfRows += datasetRows.Count;
            _context.SaveChanges();
        }
        return result ? Ok() : StatusCode(500);

    }

    [Authorize(Roles = "Architect")]
    [HttpDelete]
    public async Task<ActionResult> DeleteDataset(
        [FromBody] DeleteDatasetCommand deleteDatasetCommand
    )
    {
        var datasetPowerBi = await _service.GetDataset(deleteDatasetCommand.PowerBiId);
        if (datasetPowerBi != null)
        {
            var result = await _service.DeleteDataset(deleteDatasetCommand.PowerBiId);
            if (!result) return StatusCode(500);
        }

        var datasetDb = _context.Datasets.Where(d => d.PowerBiId == Guid.Parse(datasetPowerBi.Id)).FirstOrDefault();

        if (datasetDb != null)
        {
            var datasetDbRows = _context.DatasetRows.Where(d => d.DatasetPowerBiId == datasetDb.Id);

            if (datasetDbRows.Any())
            {
                foreach (var row in datasetDbRows)
                {
                    _context.DatasetRows.Remove(row);
                }
            }
            _context.Datasets.Remove(datasetDb);

            await RecordActivity("Dataset " + datasetPowerBi.Name + " was deleted", null);

            _context.SaveChanges();
        }

        return Ok();
    }
}