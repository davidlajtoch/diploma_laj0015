using DiplomaThesis.Shared.Contracts;

namespace DiplomaThesis.Client.Services.Interfaces;

public interface IReportService
{
    public Task<List<ReportContract>?> GetReportsFromBackend();
    public Task<bool> RebindReportToDataset(Guid reportId, Guid datasetId);
    public Task<bool> MoveReportToUserGroup(Guid reportId, Guid selectedUserGroupId);
    public Task<bool> RemoveReportFromUserGroup(Guid reportId, Guid selectedUserGroupId);
    public Task<bool> CloneReport(Guid reportId, string newReportName);
    public Task<bool> DeleteReport(Guid reportId);
}