using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IAssignmentService
{
    public Task<List<AssignmentContract>> GetUserGroupAssignments(Guid userGroupId);

    public Task<AssignmentContract> CreateAssignment(string newAssignmentName, Guid userGroupId);

    public Task<bool> UpdateAssignmentStep(Guid assignmentId, int byValue);
}