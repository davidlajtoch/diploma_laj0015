using System.Net.Http.Json;
using DiplomaThesis.Client.Extensions;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using static System.Net.WebRequestMethods;

namespace DiplomaThesis.Client.Services.Implementations;

public class ChatService : IChatService
{
    private readonly HttpClient _http;

    public ChatService(HttpClient http)
    {
        _http = http;
    }
    public async Task<bool> AddUserGroupMessage(UserGroupMessageContract userGroupMessage)
    {
        if (userGroupMessage.UserId == Guid.Empty || userGroupMessage.UserName == string.Empty ||
            userGroupMessage.Message == string.Empty || userGroupMessage.UserGroupId == Guid.Empty) return false;

        try
        {
            var response = await _http.PostAsJsonAsync(
                "Chat/AddUserGroupMessage",
                new AddUserGroupMessageCommand
                { 
                    UserId = userGroupMessage.UserId,
                    UserName = userGroupMessage.UserName,
                    Message = userGroupMessage.Message,
                    DateSent = userGroupMessage.DateSent,
                    UserGroupId = userGroupMessage.UserGroupId
                }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<List<UserGroupMessageContract>> GetUserGroupMessages(Guid userGroupId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<UserGroupMessageContract>>(
                $"Chat/GetUserGroupMessages/{userGroupId}");
            return response!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }
}

