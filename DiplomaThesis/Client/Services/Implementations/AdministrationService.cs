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

    public async Task<List<UserContract>?> GetUsers()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<IEnumerable<UserContract>>("Administration/ListUsers");
            return response?.ToList();
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

    public async Task<UserGroupContract[]?> GetUserGroups()
    {
        try
        {
            var response =
                await _http.GetFromJsonAsync<IEnumerable<UserGroupContract>>("Administration/ListUserGroups");
            return response?.ToArray();
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

    public async Task<RoleContract[]?> GetRoles()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<IEnumerable<RoleContract>>("Administration/ListRoles");
            return response?.ToArray();
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
}