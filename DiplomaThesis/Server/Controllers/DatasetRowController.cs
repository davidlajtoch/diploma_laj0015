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
public class DatasetRowController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DatasetRowController(ApplicationDbContext context)
    {
        _context = context;
    }

    [Authorize(Roles = "Architect")]
    [HttpGet("{datasetId}")]
    public async Task<ActionResult> GetDatasetRowsByDatasetId(
        [FromRoute] Guid datasetId
    )
    {
        var datasetRows = _context.DatasetRows.Where(d => d.DatasetPowerBiId == datasetId).ToList();

        List<List<string>> datasetRowsData = new();
        if(datasetRows != null)
        {
            foreach(var row in datasetRows)
            {
                datasetRowsData.Add(row.RowData);
            }
        }
        return Ok(datasetRowsData);
    }
}