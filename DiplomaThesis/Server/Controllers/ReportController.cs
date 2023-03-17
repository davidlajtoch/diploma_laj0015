using System.Security.Claims;
using DiplomaThesis.Server.Data;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Server.Services;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DiplomaThesis.Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class ReportController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PowerBiService _service;

    private readonly UserManager<ApplicationUser> _userManager;

    public ReportController(ApplicationDbContext context, PowerBiService service,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _service = service;
        _userManager = userManager;
    }

    [Authorize(Roles = "Architect")]
    [HttpGet("{reportId}")]
    public async Task<ActionResult> GetReport(
        [FromRoute] Guid reportId
    )
    {
        var result = await _service.GetReport(reportId);

        if (result is null) return NotFound();

        var reportsInDb = await _context.Reports.ToListAsync();
        var reportInDb = reportsInDb.FirstOrDefault(
            reportDb => reportDb?.PowerBiId.Equals(reportId) ?? false,
            null);
        if (reportInDb is null)
        {
            var newReportDb = new ReportDb
            {
                Id = Guid.NewGuid(),
                PowerBiId = reportId,
            };
            _context.Reports.Add(newReportDb);
            await _context.SaveChangesAsync();

            reportInDb = newReportDb;
        }

        return Ok(new ReportContract
        {
            Id = result.Id,
            Name = result.Name,
            EmbedUrl = result.EmbedUrl,
            EmbedToken = _service.GetEmbedTokenForReport(result.Id, Guid.Parse(result.DatasetId)),
            UserGroupId = reportInDb.UserGroupId ?? Guid.Empty,
            DatasetId = Guid.Parse(result.DatasetId)
        });
    }

    [HttpGet]
    public async Task<ActionResult> GetAllReports()
    {
        var result = (await _service.GetReports()).ToList();
        var reportsInDb = await _context.Reports.ToListAsync();

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var loggedInUser = await _context.Users.FindAsync(loggedInUserId);
        var loggedInUsersGroupId = loggedInUser!.UserGroupId ?? Guid.Empty;

        var roles = await _userManager.GetRolesAsync(loggedInUser);

        var response = new List<ReportContract>();
        foreach (var reportInPowerBi in result)
        {
            var reportInDb = reportsInDb.FirstOrDefault(
                reportDb => reportDb?.PowerBiId.Equals(reportInPowerBi.Id) ?? false, null);
            if (reportInDb is null)
            {
                var newReportDb = new ReportDb
                {
                    Id = Guid.NewGuid(),
                    PowerBiId = reportInPowerBi.Id,
                };
                _context.Reports.Add(newReportDb);
                await _context.SaveChangesAsync();

                reportInDb = newReportDb;
            }

            if (roles.Contains("Architect"))
                response.Add(new ReportContract
                {
                    Id = reportInPowerBi.Id,
                    Name = reportInPowerBi.Name,
                    EmbedUrl = reportInPowerBi.EmbedUrl,
                    EmbedToken = _service.GetEmbedTokenForReport(reportInPowerBi.Id, Guid.Parse(reportInPowerBi.DatasetId), true),
                    UserGroupId = reportInDb.UserGroupId ?? Guid.Empty,
                    DatasetId = Guid.Parse(reportInPowerBi.DatasetId)
                });
            else if (reportInDb.UserGroupId?.Equals(loggedInUsersGroupId) ?? true)
                response.Add(new ReportContract
                {
                    Id = reportInPowerBi.Id,
                    Name = reportInPowerBi.Name,
                    EmbedUrl = reportInPowerBi.EmbedUrl,
                    EmbedToken = _service.GetEmbedTokenForReport(reportInPowerBi.Id, Guid.Parse(reportInPowerBi.DatasetId)),
                    UserGroupId = reportInDb.UserGroupId ?? Guid.Empty,
                    DatasetId = Guid.Parse(reportInPowerBi.DatasetId)
                });
        }

        return Ok(response);
    }

    [Authorize(Roles = "Architect")]
    [HttpPost]
    public async Task<ActionResult> CloneReport(
        [FromBody] CloneReportCommand cloneReportCommand
    )
    {
        var originalReport = await _service.GetReport(cloneReportCommand.ReportId);
        if (originalReport is null) return NotFound();

        if (cloneReportCommand.NewName is null || cloneReportCommand.NewName.Length == 0) return BadRequest();

        var report = await _service.CloneReport(
            cloneReportCommand.ReportId,
            cloneReportCommand.NewName);

        if (report is null) return StatusCode(500);

        var newReportDb = new ReportDb
        {
            Id = Guid.NewGuid(),
            PowerBiId = report.Id,
        };
        _context.Reports.Add(newReportDb);
        await _context.SaveChangesAsync();

        var response = new ReportContract
        {
            Id = report.Id,
            Name = report.Name,
            EmbedUrl = report.EmbedUrl,
            EmbedToken = _service.GetEmbedTokenForReport(report.Id, Guid.Parse(report.DatasetId)),
            UserGroupId = Guid.Empty,
            DatasetId = Guid.Parse(report.DatasetId)
        };

        return Ok(response);
    }

    [Authorize(Roles = "Architect")]
    [HttpPut]
    public ActionResult MoveReportToUserGroup(
        [FromBody] MoveReportToUserGroupCommand moveReportToUserGroupCommand
    )
    {
        var reports = _context.Reports.ToList();

        var report = reports.FirstOrDefault(reportDb => reportDb?.PowerBiId.Equals(moveReportToUserGroupCommand.ReportId) ?? false, null);
        if (report is null) return NotFound();

        if(report.UserGroupId != null && report.UserGroupId != Guid.Empty)
        {
            var userGroupOld = _context.UserGroups.Find(report.UserGroupId);
            if (userGroupOld != null && userGroupOld.Reports != null) { 
                userGroupOld.Reports!.Remove(report);   
            }
        }

        var userGroup = _context.UserGroups.Find(moveReportToUserGroupCommand.UserGroupId);
        if (userGroup is null) return NotFound();

        report.UserGroupId = userGroup.Id;
        report.UserGroup = userGroup;
        userGroup.Reports!.Add(report);

        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> RemoveReportFromUserGroup(
        [FromBody] RemoveReportFromUserGroupCommand removeReportFromUserGroupCommand)
    {
        var report = _context.Reports.Where(r => r.PowerBiId == removeReportFromUserGroupCommand.ReportId).FirstOrDefault();
        if (report is null) return NotFound();

        var userGroup = await _context.UserGroups.FindAsync(report.UserGroupId);
        if (userGroup is null) return NotFound();

        report.UserGroupId = null;
        report.UserGroup = null;
        userGroup.Reports!.Remove(report);

        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Architect")]
    [HttpPut]
    public async Task<ActionResult> RebindReportToDataset(
        [FromBody] RebindReportCommand rebindReportCommand
    )
    {
        var report = await _service.GetReport(rebindReportCommand.ReportId);
        if (report is null) return NotFound();

        if (rebindReportCommand.DatasetId.ToString().Equals(report.DatasetId)) return Ok();

        var dataset = await _service.GetDataset(rebindReportCommand.DatasetId);
        if (dataset is null) return NotFound();

        var result = await _service.RebindReport(rebindReportCommand.ReportId, rebindReportCommand.DatasetId);

        if (!result) return BadRequest();

        return Ok();
    }

    [Authorize(Roles = "Architect")]
    [HttpDelete("{reportId}")]
    public async Task<ActionResult> DeleteReport(
        [FromRoute] Guid reportId)
    {
        var report = await _service.GetReport(reportId);
        if (report is null) return NotFound();

        var result = await _service.DeleteReport(reportId);
        return result ? Ok() : StatusCode(500);
    }
}