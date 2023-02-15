using System.Net.Http.Json;
using DiplomaThesis.Client.Extensions;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DiplomaThesis.Client.Services.Implementations;

public class AssignmentService : IAssignmentService
{
    private readonly HttpClient _http;

    public AssignmentService(HttpClient http)
    {
        _http = http;
    }
    public async Task<List<AssignmentContract>> GetUserGroupAssignments(Guid userGroupId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<AssignmentContract>>($"Assignment/GetUserGroupAssignments/{userGroupId}");
            return response!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }
        return null;
    }

    public async Task<AssignmentContract> CreateAssignment(string newAssignmentName, Guid userGroupId)
    {
        if (newAssignmentName == string.Empty) return new AssignmentContract();

        try
        {
            var response = await _http.PostAsJsonAsync(
                "Assignment/CreateAssignment",
                new CreateAssignmentCommand { Name = newAssignmentName, UserGroupId = userGroupId }
            );

            if (!response.IsSuccessStatusCode)
            {
                return new AssignmentContract();
            }

            var createdAssignment = await response.Content.ReadFromJsonAsync<AssignmentContract>();

            response = await _http.PostAsJsonAsync(
                "Activity/CreateAssignment",
                new ActivityCommand
                {
                    Message = "was created",
                    ObjectId1 = createdAssignment!.Id,
                    UserGroupId = createdAssignment!.UserGroupId
                }
            );
            return createdAssignment!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return new AssignmentContract();
    }
}