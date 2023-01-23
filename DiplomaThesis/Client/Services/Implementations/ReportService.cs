using System.Net.Http.Json;
using DiplomaThesis.Client.Services.Interfaces;
using DiplomaThesis.Shared.Commands;
using DiplomaThesis.Shared.Contracts;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace DiplomaThesis.Client.Services.Implementations;

public class ReportService : IReportService
{
    private readonly HttpClient _http;

    public ReportService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ReportContract>?> GetReportsFromBackend()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ReportContract>?>("Report/GetAllReports");
            return response;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
            return null;
        }
    }

    public async Task<bool> RebindReportToDataset(Guid reportId, Guid datasetId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Report/RebindReportToDataset",
                new RebindReportCommand { ReportId = reportId, DatasetId = datasetId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> MoveReportToUserGroup(Guid reportId, Guid selectedUserGroupId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Report/MoveReportToUserGroup",
                new MoveReportToUserGroupCommand { ReportId = reportId, UserGroupId = selectedUserGroupId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> RemoveReportFromUserGroup(Guid reportId, Guid selectedUserGroupId)
    {
        try
        {
            var response = await _http.PutAsJsonAsync(
                "Report/RemoveReportFromUserGroup",
                new RemoveReportFromUserGroupCommand { ReportId = reportId, UserGroupId = selectedUserGroupId }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> CloneReport(Guid reportId, string newReportName)
    {
        if (newReportName.Length == 0) return false;

        try
        {
            var response = await _http.PostAsJsonAsync(
                "Report/CloneReport",
                new CloneReportCommand { ReportId = reportId, NewName = newReportName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }

    public async Task<bool> DeleteReport(Guid reportId)
    {
        try
        {
            var response = await _http.DeleteAsync($"Report/DeleteReport/{reportId}");
            return response.IsSuccessStatusCode;
        }
        catch (AccessTokenNotAvailableException exception)
        {
            exception.Redirect();
        }

        return false;
    }
}