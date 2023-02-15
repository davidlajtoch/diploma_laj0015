using System.Linq;
using System.Security.Claims;
using DiplomaThesis.Client.Shared;
using DiplomaThesis.Server.Data;
//using DiplomaThesis.Server.Data.Migrations;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;

namespace DiplomaThesis.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AssignmentController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignmentController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    private async Task<UserContract> ApplicationUserToUserContract(ApplicationUser applicationUser)
    {
        var userGroup = _context.UserGroups.FirstOrDefault(ug => ug.Id == applicationUser.UserGroupId);

        var roles = await _userManager.GetRolesAsync(applicationUser);
        var resultRoles = roles.Select(x => new RoleContract { Name = x }).ToList();

        var result = new UserContract
        {
            Id = Guid.Parse(applicationUser.Id),
            Name = applicationUser.UserName,
            UserGroupId = userGroup!.Id,
            Roles = resultRoles
        };
        return result;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllAssignments()
    {
        var result = await _context.Assignments.ToListAsync();
        var resultOrdered = result.OrderByDescending(r => r.Created).ToList();
        return Ok(resultOrdered);
    }

    [HttpGet("{userGroupId}")]
    public async Task<ActionResult> GetUserGroupAssignments(
        [FromRoute] Guid userGroupId
    )
    {
        var userGroup = await _context.UserGroups.FindAsync(userGroupId);
        if (userGroup == null) return NotFound();

        var assignmentsUserGroup = await _context.Assignments.Where(a => a.UserGroupId == userGroupId).ToListAsync();

        var result = new List<AssignmentContract>();
        foreach (var assignment in assignmentsUserGroup)
        {
            result.Add(new AssignmentContract
            {
                Id = assignment.Id,
                Name = assignment.Name,
                Description = assignment.Description,
                Created = assignment.Created,
                Urgency = assignment.Urgency,
                Step = assignment.Step,
                UserGroupId = assignment.UserGroupId,
                User = (assignment.User == null) ? null : await ApplicationUserToUserContract(assignment.User)
            });
        }

        var resultOrdered = result.OrderByDescending(r => r.Created).ToList();

        return Ok(resultOrdered);
    }

    [HttpPost]
    public ActionResult CreateAssignment(
        [FromBody] CreateAssignmentCommand createAssignmentCommand)
    {
        if(createAssignmentCommand.Name == string.Empty || createAssignmentCommand.Name == null || createAssignmentCommand.UserGroupId == Guid.Empty) return NotFound();

        var assignment = new Assignment
        {
            Name = createAssignmentCommand.Name,
            Created = DateTime.Now,
            UserGroupId = createAssignmentCommand.UserGroupId
        };

        var result = _context.Assignments.Add(assignment);
        _context.SaveChanges();

        return Ok(new AssignmentContract
        {
            Id = result.Entity.Id,
            Name= result.Entity.Name,
            Description = result.Entity.Description,
            Created = result.Entity.Created,
            Urgency = result.Entity.Urgency,
            Step = result.Entity.Step,
            UserGroupId = result.Entity.UserGroupId,
            User = null
        });
    }
}

