using System.Linq;
using System.Runtime.CompilerServices;
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

    private readonly int[] _allowedAssignmentSteps = { 0, 1, 2 };

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
        //_context.SaveChanges();

        return Ok();
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
            var user = await _userManager.FindByIdAsync(assignment.UserId.ToString());


            result.Add(new AssignmentContract
            {
                Id = assignment.Id,
                Name = assignment.Name,
                Description = assignment.Description,
                Created = assignment.Created,
                Urgency = assignment.Urgency,
                Step = assignment.Step,
                UserGroupId = assignment.UserGroupId,
                User = (user == null) ? null : await ApplicationUserToUserContract(user)
            });
        }

        var resultOrdered = result.OrderByDescending(r => r.Created).ToList();

        return Ok(resultOrdered);
    }

    [HttpPost]
    public async Task<ActionResult> CreateAssignment(
        [FromBody] CreateAssignmentCommand createAssignmentCommand)
    {
        if (createAssignmentCommand.Name == string.Empty || createAssignmentCommand.Name == null || createAssignmentCommand.UserGroupId == Guid.Empty) return NotFound();

        var assignment = new Assignment
        {
            Name = createAssignmentCommand.Name,
            Created = DateTime.Now,
            UserGroupId = createAssignmentCommand.UserGroupId
        };

        var result = _context.Assignments.Add(assignment);

        await RecordActivity("Assignment " + assignment.Name + " was created", assignment.UserGroupId);

        _context.SaveChanges();

        return Ok(new AssignmentContract
        {
            Id = result.Entity.Id,
            Name = result.Entity.Name,
            Description = result.Entity.Description,
            Created = result.Entity.Created,
            Urgency = result.Entity.Urgency,
            Step = result.Entity.Step,
            UserGroupId = result.Entity.UserGroupId,
            User = null
        });
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteAssignment(
    [FromBody] DeleteAssignmentCommand deleteAssignmentCommand)
    {
        var assignment = await _context.Assignments.FindAsync(deleteAssignmentCommand.AssignmentId);
        if (assignment is null) return NotFound();

        _context.Assignments.Remove(assignment);

        await RecordActivity("Assignment " + assignment.Name + " was deleted", assignment.UserGroupId);

        _context.SaveChanges();

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAssignmentStep(
        [FromBody] UpdateAssignmentStepCommand updateAssignmentStepCommand)
    {
        if (updateAssignmentStepCommand.AssignmentId == Guid.Empty) return BadRequest();

        int[] allowedValues = { -1, 1 };
        if (!allowedValues.Contains(updateAssignmentStepCommand.ByValue)) return BadRequest();

        var assignment = await _context.Assignments.FindAsync(updateAssignmentStepCommand.AssignmentId);
        if (assignment == null) return NotFound();

        var resultStep = assignment.Step + updateAssignmentStepCommand.ByValue;

        if (!_allowedAssignmentSteps.Contains(resultStep)) return BadRequest();

        assignment.Step += updateAssignmentStepCommand.ByValue;

        await RecordActivity("Assignment " + assignment.Name + " had it's step " + ((updateAssignmentStepCommand.ByValue == -1) ? "decresed" : "increased"), assignment.UserGroupId);

        _context.SaveChanges();

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAssignmentUrgency(
    [FromBody] UpdateAssignmentUrgencyCommand updateAssignmentUrgencyCommand)
    {
        if (updateAssignmentUrgencyCommand.AssignmentId == Guid.Empty) return BadRequest();

        int[] allowedValues = { 0, 1, 2 };
        if (!allowedValues.Contains(updateAssignmentUrgencyCommand.Urgency)) return BadRequest();

        var assignment = await _context.Assignments.FindAsync(updateAssignmentUrgencyCommand.AssignmentId);
        if (assignment == null) return NotFound();

        assignment.Urgency = updateAssignmentUrgencyCommand.Urgency;

        await RecordActivity("Assignment " + assignment.Name + " had it's urgency set to " + updateAssignmentUrgencyCommand.Urgency, assignment.UserGroupId);

        _context.SaveChanges();

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAssignmentName(
    [FromBody] UpdateAssignmentNameCommand updateAssignmentNameCommand)
    {
        if (updateAssignmentNameCommand.AssignmentId == Guid.Empty || updateAssignmentNameCommand.Name == null) return BadRequest();

        var assignment = await _context.Assignments.FindAsync(updateAssignmentNameCommand.AssignmentId);
        if (assignment == null) return NotFound();

        var previousName = assignment.Name;

        assignment.Name = updateAssignmentNameCommand.Name;

        await RecordActivity("Assignment " + previousName + " was renamed to " + assignment.Name, assignment.UserGroupId);

        _context.SaveChanges();

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> UpdateAssignmentDescription(
        [FromBody] UpdateAssignmentDescriptionCommand updateAssignmentDescriptionCommand)
    {
        if (updateAssignmentDescriptionCommand.AssignmentId == Guid.Empty || updateAssignmentDescriptionCommand.Description == null) return BadRequest();

        var assignment = await _context.Assignments.FindAsync(updateAssignmentDescriptionCommand.AssignmentId);
        if (assignment == null) return NotFound();

        assignment.Description = updateAssignmentDescriptionCommand.Description;

        await RecordActivity("Assignment " + assignment.Name + " had it's description changed", assignment.UserGroupId);

        _context.SaveChanges();

        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> AddUserToAssignment(
        [FromBody] AddUserToAssignmentCommand addUserToAssignmentCommand)
    {
        if (addUserToAssignmentCommand.AssignmentId == Guid.Empty || addUserToAssignmentCommand.UserId == Guid.Empty) return BadRequest();

        var assignment = await _context.Assignments.FindAsync(addUserToAssignmentCommand.AssignmentId);
        if (assignment == null) return NotFound();

        var user = await _userManager.FindByIdAsync(addUserToAssignmentCommand.UserId.ToString());
        if (user == null) return NotFound();

        assignment.UserId = Guid.Parse(user.Id);

        await RecordActivity("Assignment " + assignment.Name + " was assigned to " + user.UserName, assignment.UserGroupId);

        _context.SaveChanges();
        return Ok();
    }

    [HttpPut]
    public async Task<ActionResult> RemoveUserFromAssignment(
    [FromBody] RemoveUserFromAssignmentCommand removeUserFromAssignmentCommand)
    {
        if (removeUserFromAssignmentCommand.AssignmentId == Guid.Empty) return BadRequest();

        var assignment = await _context.Assignments.FindAsync(removeUserFromAssignmentCommand.AssignmentId);
        if (assignment == null) return NotFound();

        if (assignment.UserId == null) return BadRequest();

        var previousUser = await _userManager.FindByIdAsync(assignment.UserId.ToString()); ;

        assignment.UserId = null;

        await RecordActivity("Assignment " + assignment.Name + " was disassigned from " + previousUser.UserName, assignment.UserGroupId);

        _context.SaveChanges();
        return Ok();
    }
}

