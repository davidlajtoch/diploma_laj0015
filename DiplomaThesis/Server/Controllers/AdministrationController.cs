using System.Data;
using System.Security.Claims;
using DiplomaThesis.Server.Data;
using DiplomaThesis.Server.Models;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Duende.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        return Ok();
    }

    private async Task<List<UserContract>> ApplicationUsersToUserContracts(List<ApplicationUser>? applicationUsers)
    {
        if (applicationUsers.IsNullOrEmpty())
        {
            return new List<UserContract>();
        }

        var userGroups = await _context.UserGroups.ToListAsync();

        var result = new List<UserContract>();
        foreach (var user in applicationUsers!)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var userGroupId = userGroups.FirstOrDefault(
                group =>
                {
                    if (group is null || group.Users is null) return false;
                    return group.Users.Any(userInGroup => userInGroup.Id.Equals(user.Id));
                }, null)?.Id ?? Guid.Empty;

            var resultRoles = roles.Select(x => new RoleContract { Name = x }).ToList();

            result.Add(new UserContract
            {
                Id = Guid.Parse(user.Id),
                Name = user.UserName,
                UserGroupId = userGroupId,
                Roles = resultRoles
            });
        }
        return result;
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

        var resultRoles = roles.Select(x => new RoleContract { Name = x }).ToList();

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
    public async Task<ActionResult> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var result = await ApplicationUsersToUserContracts(users);
        result = result.OrderBy(r => r.Name).ToList();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<ActionResult> DeleteUser(
        [FromBody] DeleteUserCommand deleteUserCommand)
    {
        var user = await _userManager.FindByIdAsync(deleteUserCommand.UserId.ToString());
        if (user is null) return NotFound();

        var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (loggedInUserId.Equals(user.Id)) return BadRequest();

        var result = await _userManager.DeleteAsync(user);

        await RecordActivity("User " + user.UserName + " was deleted", null);

        _context.SaveChanges();

        if (!result.Succeeded)
            return StatusCode(500);

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public ActionResult GetAllRoles()
    {
        var result = _roleManager.Roles.ToList();

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> AddRole(
        [FromBody] AddRoleCommand addRoleCommand)
    {
        var checkRoleExists = await _roleManager.RoleExistsAsync(addRoleCommand.RoleName);
        if (!checkRoleExists) return BadRequest();

        var user = await _userManager.FindByIdAsync(addRoleCommand.UserId.ToString());
        if (user is null) return NotFound();

        var checkUserHasRole = await _userManager.IsInRoleAsync(user, addRoleCommand.RoleName);
        if (checkUserHasRole) return Ok();

        var result = await _userManager.AddToRoleAsync(user, addRoleCommand.RoleName);
        if (!result.Succeeded) return BadRequest();

        await RecordActivity(user.UserName + " was added to role " + addRoleCommand.RoleName, user.UserGroupId);
        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> RemoveRole(
        [FromBody] RemoveRoleCommand removeRoleCommand)
    {
        if (removeRoleCommand.RoleName == "Admin") return BadRequest();

        var user = await _userManager.FindByIdAsync(removeRoleCommand.UserId.ToString());
        if (user is null) return NotFound();

        var checkUserHasRole = await _userManager.IsInRoleAsync(user, removeRoleCommand.RoleName);
        if (!checkUserHasRole) return Ok();

        var result = await _userManager.RemoveFromRoleAsync(user, removeRoleCommand.RoleName);
        if (!result.Succeeded) return BadRequest();

        await RecordActivity(user.UserName + " was removed from role " + removeRoleCommand.RoleName, user.UserGroupId);

        _context.SaveChanges();

        return Ok();
    }

    [HttpGet("{userGroupId}")]
    public async Task<ActionResult> GetUserGroup(
        [FromRoute] Guid userGroupId
    )
    {
        var userGroup = await _context.UserGroups.FindAsync(userGroupId);
        if (userGroup is null) return NotFound();

        var users = await _context.Users.ToListAsync();
        if (users is null) return NotFound();

        var usersMembers = await ApplicationUsersToUserContracts(users.FindAll(u => u.UserGroupId == userGroupId));

        var result = new UserGroupContract
        {
            Id = userGroup.Id,
            Name = userGroup.Name,
            Description = userGroup.Description,
            LeaderId = userGroup.LeaderId,
            Users = usersMembers
        };
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult> GetAllUserGroups()
    {
        var userGroups = _context.UserGroups;

        var result = new List<UserGroupContract>();
        foreach (var userGroup in userGroups)
        {
            var users = await _userManager.Users.ToListAsync();
            if (users is null) return NotFound();

            var usersMembers = await ApplicationUsersToUserContracts(users.FindAll(u => u.UserGroupId == userGroup.Id));

            result.Add(
                new UserGroupContract
                {
                    Id = userGroup.Id,
                    Name = userGroup.Name,
                    Description = userGroup.Description,
                    LeaderId = userGroup.LeaderId,
                    Users = usersMembers
                }
            );
        }

        result = result.OrderBy(r => r.Name).ToList();
        return Ok(result);
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult> GetUserGroupByUserId(
        [FromRoute] Guid userId
    )
    {
        var user = await _context.Users.FindAsync(userId.ToString());
        if (user is null) return NotFound();
        if (user.UserGroupId == null || user.UserGroupId == Guid.Empty)
        {
            return Ok(new JsonResult(new object()));
        }

        var userGroup = await _context.UserGroups.FindAsync(user.UserGroupId);

        if (userGroup is null) return NotFound();

        var userGroupUsers = await ApplicationUsersToUserContracts(userGroup.Users);

        var result = new UserGroupContract
        {
            Id = userGroup.Id,
            Name = userGroup.Name,
            Description = userGroup.Description,
            LeaderId = userGroup.LeaderId,
            Users = userGroupUsers
        };

        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult> CreateUserGroup(
        [FromBody] CreateUserGroupCommand createUserGroupCommand)
    {
        if (_context.UserGroups.Any(group => group.Name.Equals(createUserGroupCommand.Name))) return BadRequest();

        var userGroup = new UserGroup
        {
            Id = Guid.NewGuid(),
            Name = createUserGroupCommand.Name,
            Users = new List<ApplicationUser>()
        };

        _context.UserGroups.Add(userGroup);

        await RecordActivity("User group " + createUserGroupCommand.Name + " was created", userGroup.Id);

        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<ActionResult> DeleteUserGroup(
        [FromBody] DeleteUserGroupCommand deleteUserGroupCommand)
    {
        var userGroup = await _context.UserGroups.FindAsync(deleteUserGroupCommand.UserGroupId);
        if (userGroup is null) return NotFound();

        var users = _userManager.Users.Where(u => u.UserGroupId == deleteUserGroupCommand.UserGroupId);

        foreach (var user in users)
        {
            user.UserGroupId = null;
        }

        _context.UserGroups.Remove(userGroup);

        await RecordActivity("User group " + userGroup.Name + " was deleted", null);

        _context.SaveChanges();

        return Ok();
    }

    [HttpGet("{userGroupId}")]
    public async Task<ActionResult> GetUserGroupMembers(
        [FromRoute] Guid userGroupId
    )
    {
        var user_group = await _context.UserGroups.FindAsync(userGroupId);
        if (user_group is null) return NotFound();

        var users = await _context.Users.ToListAsync();
        if (users is null) return NotFound();

        var usersMembers = users.FindAll(u => u.UserGroupId == userGroupId);

        var result = new List<UserContract>();
        foreach (var user in usersMembers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var resultRoles = roles.Select(x => new RoleContract { Name = x }).ToList();

            result.Add(new UserContract
            {
                Id = Guid.Parse(user.Id),
                Name = user.UserName,
                UserGroupId = userGroupId,
                Roles = resultRoles
            });
        }

        result = result.OrderBy(r => r.Name).ToList();
        return Ok(result);
    }

    [HttpGet("{userGroupId}")]
    public async Task<ActionResult> GetUserGroupNonMembers(
        [FromRoute] Guid userGroupId
    )
    {
        var user_group = await _context.UserGroups.FindAsync(userGroupId);
        if (user_group is null) return NotFound();

        var users = await _context.Users.ToListAsync();
        if (users is null) return NotFound();

        var usersNonMembers = users.FindAll(u => u.UserGroupId != userGroupId);

        var result = new List<UserContract>();
        foreach (var user in usersNonMembers)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var resultRoles = roles.Select(x => new RoleContract { Name = x }).ToList();

            result.Add(new UserContract
            {
                Id = Guid.Parse(user.Id),
                Name = user.UserName,
                UserGroupId = userGroupId,
                Roles = resultRoles
            });
        }

        result = result.OrderBy(r => r.Name).ToList();
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> MoveUserToUserGroup(
        [FromBody] MoveUserToUserGroupCommand moveUserToUserGroupCommand)
    {
        var user = await _context.Users.FindAsync(moveUserToUserGroupCommand.UserId.ToString());
        if (user is null) return NotFound();

        var newUserGroup = _context.UserGroups.Find(moveUserToUserGroupCommand.UserGroupId);
        if (newUserGroup is null) return NotFound();

        if(user.UserGroup != null || user.UserGroupId != null)
        {
            var oldUserGroup = _context.UserGroups.Find(user.UserGroupId);
            if (oldUserGroup is null) return NotFound();

            if(oldUserGroup.Users != null)
            {
                oldUserGroup.Users.Remove(user);

                if(oldUserGroup.LeaderId != null && oldUserGroup.LeaderId == Guid.Parse(user.Id))
                {
                    oldUserGroup.LeaderId = null;
                }
            }
        }

        user.UserGroup = newUserGroup;
        user.UserGroupId = newUserGroup.Id;

        await RecordActivity("User " + user.UserName + " was moved to user group " + newUserGroup.Name, newUserGroup.Id);

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

        var userGroup = _context.UserGroups.Find(removeUserFromUserGroupCommand.UserGroupId);

        if (user.UserGroupId != userGroup!.Users!.FirstOrDefault(u => u.UserGroupId == user.UserGroupId)!.UserGroupId) return NotFound();

        if (userGroup.LeaderId != null)
        {
            if (userGroup.LeaderId.ToString() == user.Id)
            {
                userGroup.LeaderId = null;
            }
        }

        user.UserGroup = null;
        user.UserGroupId = null;
        userGroup.Users!.Remove(user);

        await RecordActivity("User " + user.UserName + " was removed from user group " + userGroup.Name, userGroup.Id);

        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> UpdateUserGroupDescription(
        [FromBody] UpdateUserGroupDescriptionCommand updateUserGroupDescriptionCommand)
    {
        if (updateUserGroupDescriptionCommand.UserGroupId == Guid.Empty) return BadRequest();

        var userGroup = await _context.UserGroups.FindAsync(updateUserGroupDescriptionCommand.UserGroupId);
        if (userGroup is null) return NotFound();

        userGroup.Description = updateUserGroupDescriptionCommand.Description;

        await RecordActivity("User group " + userGroup.Name + " had it's description changed", userGroup.Id);

        _context.SaveChanges();

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPut]
    public async Task<ActionResult> UpdateUserGroupLeader(
    [FromBody] UpdateUserGroupLeaderCommand updateUserGroupLeaderCommand)
    {
        if (updateUserGroupLeaderCommand.UserId == Guid.Empty || updateUserGroupLeaderCommand.UserGroupId == Guid.Empty) return BadRequest();

        var userGroup = await _context.UserGroups.FindAsync(updateUserGroupLeaderCommand.UserGroupId);
        if (userGroup is null) return NotFound();

        var user = await _context.Users.FindAsync(updateUserGroupLeaderCommand.UserId.ToString());
        if (user is null) return NotFound();

        userGroup.LeaderId = updateUserGroupLeaderCommand.UserId;

        await RecordActivity("User group " + userGroup.Name + " had it's leader changed to " + user.UserName, userGroup.Id);

        _context.SaveChanges();

        return Ok();
    }
}