using System.Linq;
using System.Security.Claims;
using DiplomaThesis.Server.Data;
using DiplomaThesis.Server.Data.Migrations;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.PowerBI.Api.Models;

namespace DiplomaThesis.Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]/[action]")]
public class AdministrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdministrationController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUser(
        [FromRoute] Guid userId
    )
    {
        var user = await _context.Users.FindAsync(userId.ToString());

        if (user is null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);

        var userGroups = await _context.UserGroups.ToListAsync();
        var userGroupId = userGroups.FirstOrDefault(
            group =>
            {
                if (group is null || group.Users is null) return false;
                return group.Users.Any(userInGroup => userInGroup.Id.Equals(userId.ToString()));
            }, null)?.Id ?? Guid.Empty;

        var resultRoles = roles.Select(x => new RoleContract { Name = x });

        var result = new UserContract
        {
            Id = Guid.Parse(user.Id),
            Name = user.UserName,
            UserGroupId = userGroupId,
            Roles = resultRoles
        };
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult> ListUsers()
    {
        var users = await _userManager.Users.ToListAsync();

        var userGroups = await _context.UserGroups.ToListAsync();

        var result = new List<UserContract>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var userGroupId = userGroups.FirstOrDefault(
                group =>
                {
                    if (group is null || group.Users is null) return false;
                    return group.Users.Any(userInGroup => userInGroup.Id.Equals(user.Id));
                }, null)?.Id ?? Guid.Empty;

            var resultRoles = roles.Select(x => new RoleContract { Name = x });

            result.Add(new UserContract
            {
                Id = Guid.Parse(user.Id),
                Name = user.UserName,
                UserGroupId = userGroupId,
                Roles = resultRoles
            });
        }

        return Ok(result.AsEnumerable());
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult ListRoles()
    {
        var result = _roleManager.Roles;

        return Ok(result.AsEnumerable());
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> AddRole(
        [FromBody] AddRoleCommand addRoleCommand)
    {
        var checkRoleExists = await _roleManager.RoleExistsAsync(addRoleCommand.RoleName);
        if (!checkRoleExists) return BadRequest();

        var user = await _userManager.FindByNameAsync(addRoleCommand.UserName);
        if (user is null) return NotFound();

        var checkUserHasRole = await _userManager.IsInRoleAsync(user, addRoleCommand.RoleName);
        if (checkUserHasRole) return Ok();

        var result = await _userManager.AddToRoleAsync(user, addRoleCommand.RoleName);
        if (!result.Succeeded) return BadRequest();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> RemoveRole(
        [FromBody] RemoveRoleCommand removeRoleCommand)
    {
        if (removeRoleCommand.RoleName == "Admin") return BadRequest();

        var user = await _userManager.FindByNameAsync(removeRoleCommand.UserName);
        if (user is null) return NotFound();

        var checkUserHasRole = await _userManager.IsInRoleAsync(user, removeRoleCommand.RoleName);
        if (!checkUserHasRole) return Ok();

        var result = await _userManager.RemoveFromRoleAsync(user, removeRoleCommand.RoleName);
        if (!result.Succeeded) return BadRequest();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(
        [FromBody] DeleteUserCommand deleteUserCommand)
    {
        var user = await _userManager.FindByNameAsync(deleteUserCommand.UserName);
        if (user is null) return NotFound();

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (loggedInUserId.Equals(user.Id)) return BadRequest();

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
            return StatusCode(500);

        return Ok();
    }

    [HttpGet("{user_group_id}")]
    public async Task<ActionResult> GetUserGroup(
        [FromRoute] Guid user_group_id    
    )
    {
        var user_group = await _context.UserGroups.FindAsync(user_group_id);
        if (user_group is null) return NotFound();

        var users = await _context.Users.ToListAsync();
        if (users is null) return NotFound();

        var users_group_ids = users.FindAll(u => u.UserGroupId == user_group_id).Select(u => Guid.Parse(u.Id)).ToList();

        var result = new UserGroupContract
        {
            Id = user_group.Id,
            Name = user_group.Name,
            Users = users_group_ids.AsEnumerable()
        };
        return Ok(result);
    }

    [HttpGet]
    public ActionResult ListUserGroups()
    {
        var result = _context.UserGroups.Select(group => new UserGroupContract
        {
            Id = group.Id,
            Name = group.Name,
            Users = group.Users.Select(user => Guid.Parse(user.Id))
        }).ToList();

        return Ok(result.AsEnumerable());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public ActionResult CreateUserGroup(
        [FromBody] CreateUserGroupCommand createUserGroupCommand)
    {
        if (_context.UserGroups.Any(group => group.Name.Equals(createUserGroupCommand.Name))) return BadRequest();

        var userGroup = new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = createUserGroupCommand.Name,
            Users = new List<ApplicationUser>()
        };

        var result = _context.UserGroups.Add(userGroup);
        _context.SaveChanges();

        return Ok(new UserGroupContract
        {
            Id = result.Entity.Id,
            Name = result.Entity.Name,
            Users = new List<Guid>()
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public ActionResult DeleteUserGroup(
        [FromRoute] Guid userGroupId)
    {
        var userGroup = _context.UserGroups.Find(userGroupId.ToString());
        if (userGroup is null) return NotFound();

        _context.UserGroups.Remove(userGroup);
        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> MoveUserToUserGroup(
        [FromBody] MoveUserToUserGroupCommand moveUserToUserGroupCommand)
    {
        var user = await _context.Users.FindAsync(moveUserToUserGroupCommand.UserId.ToString());
        if (user is null) return NotFound();

        if (moveUserToUserGroupCommand.UserGroupId.Equals(Guid.Empty))
        {
            var userGroups = _context.UserGroups.ToList();
            var userGroup = userGroups.FirstOrDefault(
                group =>
                {
                    if (group is null || group.Users is null) return false;
                    return group.Users.Any(userInGroup => userInGroup.Id.Equals(user.Id));
                }, null);
            user.UserGroup = null;
            userGroup?.Users.Remove(user);
        }
        else
        {
            var userGroup = _context.UserGroups.Find(moveUserToUserGroupCommand.UserGroupId);
            if (userGroup is null) return NotFound();

            user.UserGroup = userGroup;
            user.UserGroupId = userGroup.Id;
        }

        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> RemoveUserFromUserGroup(
        [FromBody] RemoveUserFromUserGroupCommand removeUserFromUserGroupCommand)
    {
        var user = await _context.Users.FindAsync(removeUserFromUserGroupCommand.UserId.ToString());
        if (user is null) return NotFound();

        var user_group = _context.UserGroups.Find(removeUserFromUserGroupCommand.UserGroupId);

        if(user.UserGroupId != user_group.Users.FirstOrDefault(u => u.UserGroupId == user.UserGroupId).UserGroupId) return NotFound();

        user.UserGroup = null;
        user.UserGroupId = null;
        user_group.Users.Remove(user);

        _context.SaveChanges();

        return Ok();
    }
}