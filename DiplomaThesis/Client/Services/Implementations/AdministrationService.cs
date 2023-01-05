using System.Net.Http.Json;
using DiplomaThesis.Client.Extensions;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DiplomaThesis.Client.Services.Implementations;

public class AdministrationService : IAdministrationService
{
    private readonly HttpClient _http;

    public AdministrationService(HttpClient http)
    {
        _http = http;
    }

    public async Task<UserContract?> GetUser(Guid userId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<UserContract>($"Administration/GetUser/{userId}");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return null;
    }

    public async Task<List<UserContract>?> GetAllUsers()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<UserContract>>("Administration/GetAllUsers");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<bool> DeleteUser(string userName)
    {
        try
        {
            var response = await _http.DeleteAsJsonAsync(
                "Administration/DeleteUser",
                new DeleteUserCommand { UserName = userName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<List<RoleContract>?> GetAllRoles()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<RoleContract>>("Administration/GetAllRoles");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<bool> AddRole(string userName, string roleName)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Administration/AddRole",
                new AddRoleCommand { UserName = userName, RoleName = roleName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> RemoveRole(string userName, string roleName)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Administration/RemoveRole",
                new RemoveRoleCommand { UserName = userName, RoleName = roleName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    /*public async Task<List<RoleContract>?> GetUserRoles()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<RoleContract>>("Administration/GetUserRoles");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<List<RoleContract>?> GetNonUserRoles()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<RoleContract>>("Administration/GetNonUserRoles");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }*/

    public async Task<UserGroupContract?> GetUserGroup(Guid user_group_id)
    {
        try
        {
            var response =
                await _http.GetFromJsonAsync<UserGroupContract>($"Administration/GetUserGroup/{user_group_id}");
            return response!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        return null;
    }

    public async Task<List<UserGroupContract>?> GetAllUserGroups()
    {
        try
        {
            var response =
                await _http.GetFromJsonAsync<List<UserGroupContract>>("Administration/GetAllUserGroups");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<bool> CreateUserGroup(string newUserGroupName)
    {
        if (newUserGroupName.Length == 0) return false;

        try
        {
            var response = await _http.PostAsJsonAsync(
                "Administration/CreateUserGroup",
                new CreateUserGroupCommand { Name = newUserGroupName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> DeleteUserGroup(Guid userGroupId)
    {
        try
        {
            var response = await _http.DeleteAsJsonAsync(
                "Administration/DeleteUserGroup",
                new DeleteUserGroupCommand { UserGroupId = userGroupId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<List<UserContract>?> GetUserGroupMembers(Guid userGroupId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<UserContract>?>(
                $"Administration/GetUserGroupMembers/{userGroupId}");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<List<UserContract>?> GetUserGroupNonMembers(Guid userGroupId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<UserContract>?>(
                $"Administration/GetUserGroupNonMembers/{userGroupId}");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<bool> MoveUserToUserGroup(Guid userId, Guid userGroupId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Administration/MoveUserToUserGroup",
                new MoveUserToUserGroupCommand { UserId = userId, UserGroupId = userGroupId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> RemoveUserFromUserGroup(Guid userId, Guid userGroupId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Administration/RemoveUserFromUserGroup",
                new RemoveUserFromUserGroupCommand { UserId = userId, UserGroupId = userGroupId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }
}