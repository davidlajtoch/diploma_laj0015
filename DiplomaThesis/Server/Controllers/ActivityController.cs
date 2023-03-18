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

    [HttpGet]
    public async Task<ActionResult> GetAllActivity()
    {
        var acitvities = await _context.Activities.ToListAsync();
        var acitivtiesOrdered = acitvities.OrderByDescending(r => r.Created).ToList().Take(30);

        List<ActivityContract> result = new();
        foreach(var activity in acitivtiesOrdered)
        {
            result.Add(new ActivityContract
            {
                Id= activity.Id,
                Message=activity.Message,
                Created=activity.Created,
                UserGroupName=(activity.UserGroupName == null)? string.Empty : activity.UserGroupName,
            });
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetCurrentUserUserGroupActivity()
    {
        var activityAll = await _context.Activities.ToListAsync();

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var loggedInUser = await _context.Users.FindAsync(loggedInUserId);

        List<ActivityContract> result = new();
        if (loggedInUser == null || loggedInUser.UserGroupId == null || loggedInUser.UserGroupId == Guid.Empty)
        {
            return Ok(result);
        }

        var activities = activityAll.FindAll(a => a.UserGroupId == loggedInUser.UserGroupId);
        var acitivtiesOrdered = activities.OrderByDescending(r => r.Created).ToList().Take(30);

        foreach(var activity in acitivtiesOrdered)
        {
            result.Add(new ActivityContract
            {
                Id= activity.Id,
                Message=activity.Message,
                Created=activity.Created,
                UserGroupName=(activity.UserGroupName == null)? string.Empty : activity.UserGroupName,
            });
        }

        return Ok(result);
    }
}

