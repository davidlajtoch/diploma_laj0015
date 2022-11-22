using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IAdministrationService
{
    public Task<UserContract?> GetUser(Guid userId);

    public Task<UserContract[]?> GetUsers();

    public Task<bool> DeleteUser(string userName);

    public Task<bool> MoveUserToUserGroup(Guid userId, Guid userGroupId);

    public Task<UserGroupContract[]?> GetUserGroups();

    public Task<bool> CreateUserGroup(string newUserGroupName);

    public Task<RoleContract[]?> GetRoles();

    public Task<bool> AddRole(string userName, string roleName);

    public Task<bool> RemoveRole(string userName, string roleName);
}