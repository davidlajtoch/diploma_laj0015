using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IAssignmentService
{
    public Task<List<AssignmentContract>> GetUserGroupAssignments(Guid userGroupId);

    public Task<AssignmentContract> CreateAssignment(string newAssignmentName, Guid userGroupId);

    public Task<bool> UpdateAssignmentStep(Guid assignmentId, int byValue);

    public Task<bool> UpdateAssignmentUrgency(Guid assignmentId, int urgency);

    public Task<bool> AddUserToAssignment(Guid assignmentId, Guid userId);

    public Task<bool> RemoveUserFromAssignment(Guid assignmentId);
}