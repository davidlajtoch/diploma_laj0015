using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IAssignmentService
{
    public Task<List<AssignmentContract>> GetUserGroupAssignments(Guid userGroupId);

    public Task<AssignmentContract> CreateAssignment(string newAssignmentName, Guid userGroupId);

    public Task<bool> DeleteAssignment(Guid assignmentId);

    public Task<bool> UpdateAssignmentStep(Guid assignmentId, int byValue);

    public Task<bool> UpdateAssignmentUrgency(Guid assignmentId, int urgency);

    public Task<bool> UpdateAssignmentName(Guid assignmentId, string name);

    public Task<bool> UpdateAssignmentDescription(Guid assignmentId, string description);

    public Task<bool> AddUserToAssignment(Guid assignmentId, Guid userId);

    public Task<bool> RemoveUserFromAssignment(Guid assignmentId);
}