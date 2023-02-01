using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IChatService
{
    public Task<bool> AddUserGroupMessage(UserGroupMessageContract userGroupMessage);
    public Task<List<UserGroupMessageContract>> GetUserGroupMessages(Guid userGroupId);
}
