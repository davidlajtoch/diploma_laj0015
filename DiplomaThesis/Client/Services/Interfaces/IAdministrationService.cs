using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IAdministrationService
{
    public Task<UserContract?> GetUser(Guid userId);

    public Task<List<UserContract>?> GetAllUsers();

    public Task<bool> DeleteUser(string userName);

    public Task<List<RoleContract>?> GetAllRoles();

    public Task<bool> AddRole(Guid userId, string roleName);

    public Task<bool> RemoveRole(Guid userId, string roleName);

    public Task<UserGroupContract?> GetUserGroup(Guid user_group_id);

    public Task<List<UserGroupContract>?> GetAllUserGroups();

    public Task<UserGroupContract?> GetUserUserGroup(Guid userId);

    public Task<bool> CreateUserGroup(string newUserGroupName);

    public Task<bool> DeleteUserGroup(Guid userGroupId);

    public Task<List<UserContract>?> GetUserGroupMembers(Guid userGroupId);

    public Task<List<UserContract>?> GetUserGroupNonMembers(Guid userGroupId);

    public Task<bool> MoveUserToUserGroup(Guid userId, Guid userGroupId);

    public Task<bool> RemoveUserFromUserGroup(Guid userId, Guid userGroupId);

    public Task<bool> UpdateUserGroupDescription(Guid userGroupId, string description);
   
}