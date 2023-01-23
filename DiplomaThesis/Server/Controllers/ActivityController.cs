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

    public ActivityController(
        ApplicationDbContext context
    )
    {
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
        var resultOrdered = result.OrderByDescending(r=>r.Created).ToList().Take(30);
        return Ok(resultOrdered);
    }

    [HttpPost]
    public ActionResult MoveUserToUserGroup(
        [FromBody] ActivityCommand activityCommand)
    {
        var object1 = _context.Users.Find(activityCommand.ObjectId1.ToString());
        if (object1 == null)
        {
            return NotFound();
        }

        var object2 =  _context.UserGroups.Find(activityCommand.ObjectId2);
        if (object2 == null)
        {
            return NotFound();
        }

        string message_complete = BuildMessage(object1.UserName, activityCommand.Message, object2.Name);

        Activity activity = new Activity{
            Message = message_complete,
            UserGroup = object2,
            Created = DateTime.Now
        };
        _context.Activities.Add(activity);
        _context.SaveChanges();

        return Ok();
    }
}

