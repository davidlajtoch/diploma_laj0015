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
            return createdAssignment!;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return new AssignmentContract();
    }

    public async Task<bool> DeleteAssignment(Guid assignmentId)
    {
        try
        {
            var response = await _http.DeleteAsJsonAsync(
                "Assignment/DeleteAssignment",
                new DeleteAssignmentCommand { AssignmentId = assignmentId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> UpdateAssignmentStep(Guid assignmentId, int byValue)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Assignment/UpdateAssignmentStep",
                new UpdateAssignmentStepCommand { AssignmentId = assignmentId, ByValue = byValue }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> UpdateAssignmentUrgency(Guid assignmentId, int urgency)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Assignment/UpdateAssignmentUrgency",
                new UpdateAssignmentUrgencyCommand { AssignmentId = assignmentId, Urgency = urgency }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> UpdateAssignmentName(Guid assignmentId, string name)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Assignment/UpdateAssignmentName",
                new UpdateAssignmentNameCommand { AssignmentId = assignmentId, Name = name }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> UpdateAssignmentDescription(Guid assignmentId, string description)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Assignment/UpdateAssignmentDescription",
                new UpdateAssignmentDescriptionCommand { AssignmentId = assignmentId, Description = description }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> AddUserToAssignment(Guid assignmentId, Guid userId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Assignment/AddUserToAssignment",
                new AddUserToAssignmentCommand { AssignmentId = assignmentId, UserId = userId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> RemoveUserFromAssignment(Guid assignmentId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Assignment/RemoveUserFromAssignment",
                new RemoveUserFromAssignmentCommand { AssignmentId = assignmentId }
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