using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IActivityService
{
    public Task<List<ActivityContract>?> GetAllActivity();
}