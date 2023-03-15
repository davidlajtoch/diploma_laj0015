using DiplomaThesis.Server.Data;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Server.Services;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.PowerBI.Api.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace DiplomaThesis.Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class DatasetController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PowerBiService _service;

    private const int _datasetRowInsertLimit = 5;

    public DatasetController(PowerBiService service, ApplicationDbContext context)
    {
        _service = service;
        _context = context;
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
                ColumnNames = datasetInDb?.ColumnNames ?? new List<string>(),
                ColumnTypes = datasetInDb?.ColumnTypes ?? new List<string>()
            });
        }

        return Ok(result);
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

        var datasetInDb = new DatasetDb
        {
            Id = Guid.NewGuid(),
            PowerBiId = Guid.Parse(dataset.Id),
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
        await _context.SaveChangesAsync();

        var result = await _service.PushRowsToDataset(Guid.Parse(dataset.Id), rows);
        return result ? Ok() : StatusCode(500);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost("{datasetId}")]
    public async Task<ActionResult> UploadRowsToDataset(
        [FromRoute] Guid datasetId,
        [FromBody] List<object> rows
    )
    {
        var dataset = await _service.GetDataset(datasetId);
        if (dataset is null) return NotFound();

        var result = await _service.PushRowsToDataset(datasetId, rows);
        return result ? Ok() : StatusCode(500);
    }

    [Authorize(Roles = "Architect")]
    [HttpDelete]
    public async Task<ActionResult> DeleteDataset(
        [FromBody] DeleteDatasetCommand deleteDatasetCommand
    )
    {
        var datasetPowerBi = await _service.GetDataset(deleteDatasetCommand.PowerBiId);
        if (datasetPowerBi is null) return StatusCode(500);

        var result = await _service.DeleteDataset(deleteDatasetCommand.PowerBiId);
        if (!result) return StatusCode(500);

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
            _context.SaveChanges();
        }

        return Ok();
    }
}