using System.Net.Http.Json;
using DiplomaThesis.Client.Extensions;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DiplomaThesis.Client.Services.Implementations;

public class ActivityService : IActivityService
{
    private readonly HttpClient _http;

    public ActivityService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ActivityContract>?> GetAllActivity()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ActivityContract>>("Activity/GetAllActivity");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<List<ActivityContract>?> GetCurrentUserUserGroupActivity()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ActivityContract>>("Activity/GetCurrentUserUserGroupActivity");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }
}