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
using Microsoft.VisualBasic;

namespace DiplomaThesis.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ChatController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ChatController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> AddUserGroupMessage(
        [FromBody] AddUserGroupMessageCommand addUserGroupMessageCommand)
    {
        var user = await _context.Users.FindAsync(addUserGroupMessageCommand.UserId.ToString());
        if (user is null) return NotFound();

        var userGroup = await _context.UserGroups.FindAsync(addUserGroupMessageCommand.UserGroupId);
        if (userGroup is null) return NotFound();

        UserGroupMessage message = new UserGroupMessage
        {
            UserId = Guid.Parse(user.Id),
            UserName = user.UserName,
            Message = addUserGroupMessageCommand.Message,
            DateSent = addUserGroupMessageCommand.DateSent,
            UserGroupId = addUserGroupMessageCommand.UserGroupId
        };

        _context.UserGroupMessages.Add(message);
        _context.SaveChanges();

        return Ok();
    }

    [HttpGet("{userGroupId}")]
    public async Task<ActionResult> GetUserGroupMessages(
        [FromRoute] Guid userGroupId    
    )
    {
        var userGroupMessages = await _context.UserGroupMessages.Where(m => m.UserGroupId == userGroupId).ToListAsync();

        var result = new List<UserGroupMessageContract>();
        foreach(var message in userGroupMessages)
        {
            result.Add(
                new UserGroupMessageContract
                {
                    UserId = message.UserId,
                    UserName = message.UserName,
                    Message = message.Message,
                    DateSent = message.DateSent,
                    UserGroupId = message.UserGroupId
                }    
            );
        }
        return Ok(result);
    }
}