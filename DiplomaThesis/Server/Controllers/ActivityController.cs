using System.Linq;
using System.Security.Claims;
using DiplomaThesis.Client.Shared;
using DiplomaThesis.Server.Data;
//using DiplomaThesis.Server.Data.Migrations;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;

namespace DiplomaThesis.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ActivityController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ActivityController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    private string BuildMessage(string objectName1, string message, string objectName2)
    {
        return objectName1 + " " + message + " " + objectName2;
    }

    [HttpGet]
    public async Task<ActionResult> GetAllActivity()
    {
        var result = await _context.Activities.ToListAsync();
        var resultOrdered = result.OrderByDescending(r => r.Created).ToList().Take(30);
        return Ok(resultOrdered);
    }

    [HttpGet]
    public async Task<ActionResult> GetCurrentUserUserGroupActivity()
    {
        var activityAll = await _context.Activities.ToListAsync();

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var loggedInUser = await _context.Users.FindAsync(loggedInUserId);

        if (loggedInUser == null || loggedInUser.UserGroupId == null || loggedInUser.UserGroupId == Guid.Empty)
        {
            return Ok(new List<ActivityContract>());
        }

        var result = activityAll.FindAll(a => a.UserGroupId == loggedInUser.UserGroupId);
        var resultOrdered = result.OrderByDescending(r => r.Created).ToList().Take(30);

        return Ok(resultOrdered);
    }

    [HttpPost]
    public ActionResult AddRole(
        [FromBody] ActivityCommand activityCommand)
    {
        var user = _context.Users.Find(activityCommand.ObjectId1.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userUserGroup = _context.UserGroups.Find(user.UserGroupId);

        string message_complete = BuildMessage(user.UserName, activityCommand.Message, activityCommand.ObjectName2!);

        Activity activity = new Activity
        {
            Message = message_complete,
            Created = DateTime.Now,
            UserGroupName = (userUserGroup == null)? string.Empty : userUserGroup.Name,
            UserGroupId = user.UserGroupId,
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public ActionResult RemoveRole(
        [FromBody] ActivityCommand activityCommand)
    {
        var user = _context.Users.Find(activityCommand.ObjectId1.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userUserGroup = _context.UserGroups.Find(user.UserGroupId);

        string message_complete = BuildMessage(user.UserName, activityCommand.Message, activityCommand.ObjectName2!);

        Activity activity = new Activity
        {
            Message = message_complete,
            Created = DateTime.Now,
            UserGroupName = (userUserGroup == null)? string.Empty : userUserGroup.Name,
            UserGroupId = user.UserGroupId,
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public ActionResult MoveUserToUserGroup(
        [FromBody] ActivityCommand activityCommand)
    {
        var user = _context.Users.Find(activityCommand.ObjectId1.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userGroup = _context.UserGroups.Find(activityCommand.ObjectId2);
        if (userGroup == null)
        {
            return NotFound();
        }

        string message_complete = BuildMessage(user.UserName, activityCommand.Message, userGroup.Name);

        Activity activity = new Activity
        {
            Message = message_complete,
            Created = DateTime.Now,
            UserGroupName = userGroup.Name,
            UserGroupId = userGroup.Id,
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public ActionResult RemoveUserFromUserGroup(
        [FromBody] ActivityCommand activityCommand)
    {
        var user = _context.Users.Find(activityCommand.ObjectId1.ToString());
        if (user == null)
        {
            return NotFound();
        }

        var userGroup = _context.UserGroups.Find(activityCommand.ObjectId2);
        if (userGroup == null)
        {
            return NotFound();
        }

        string message_complete = BuildMessage(user.UserName, activityCommand.Message, userGroup.Name);

        Activity activity = new Activity
        {
            Message = message_complete,
            Created = DateTime.Now,
            UserGroupName = userGroup.Name,
            UserGroupId = userGroup.Id,
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public ActionResult CreateUserGroup(
        [FromBody] ActivityCommand activityCommand)
    {
        var userGroup = _context.UserGroups.Find(activityCommand.ObjectId1);
        if (userGroup == null)
        {
            return NotFound();
        }

        string message_complete = BuildMessage(userGroup.Name, activityCommand.Message, "");

        Activity activity = new Activity
        {
            Message = message_complete,
            Created = DateTime.Now,
            UserGroupName = userGroup.Name,
            UserGroupId = userGroup.Id,
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }

    [HttpPost]
    public ActionResult UserGroupDeleted(
    [FromBody] ActivityCommand activityCommand)
    {
        var userGroup = _context.UserGroups.Find(activityCommand.ObjectId1);
        if (userGroup == null)
        {
            return NotFound();
        }

        string message_complete = BuildMessage(userGroup.Name, activityCommand.Message, "");

        Activity activity = new Activity
        {
            Message = message_complete,
            Created = DateTime.Now,
            UserGroupName = userGroup.Name,
            UserGroupId = null,
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }
}

