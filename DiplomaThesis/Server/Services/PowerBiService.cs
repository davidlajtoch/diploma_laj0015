using DiplomaThesis.Server.Models.Options;
using Microsoft.Extensions.Options;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;

namespace DiplomaThesis.Server.Services;

public class PowerBiService
{
    private readonly AadService _aadService;
    private readonly IOptions<PowerBiOptions> _powerBiOptions;

    public PowerBiService(AadService aadService, IOptions<PowerBiOptions> powerBiOptions)
    {
        _aadService = aadService;
        _powerBiOptions = powerBiOptions;
    }

    private PowerBIClient GetPowerBiClient()
    {
        var tokenCredentials = new TokenCredentials(_aadService.GetAccessToken(), "Bearer");
        return new PowerBIClient(new Uri(_powerBiOptions.Value.ApiRoot), tokenCredentials);
    }

    public async Task<IEnumerable<Report>> GetReports()
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Reports.GetReportsInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId));

        return response.Response.IsSuccessStatusCode ? response.Body.Value : new List<Report>();
    }

    public async Task<Report?> GetReport(Guid reportId)
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Reports.GetReportInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            reportId);

        return response.Response.IsSuccessStatusCode ? response.Body : null;
    }

    public async Task<Report?> CloneReport(Guid reportId, string newReportName)
    {
        var powerBiClient = GetPowerBiClient();

        var cloneReportRequest = new CloneReportRequest(newReportName);

        var response = await powerBiClient.Reports.CloneReportInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            reportId,
            cloneReportRequest);

        return response.Response.IsSuccessStatusCode ? response.Body : null;
    }

    public async Task<bool> RebindReport(Guid reportId, Guid datasetId)
    {
        var powerBiClient = GetPowerBiClient();

        var rebindReportRequest = new RebindReportRequest(datasetId.ToString());

        var response = await powerBiClient.Reports.RebindReportInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            reportId,
            rebindReportRequest);

        return response.Response.IsSuccessStatusCode;
    }

    public string GetEmbedTokenForReport(Guid reportId, Guid datasetId, bool canEdit = false)
    {
        var powerBiClient = GetPowerBiClient();

        var tokenRequest = new GenerateTokenRequest(
            canEdit ? TokenAccessLevel.Edit : TokenAccessLevel.View,
            datasetId.ToString()
        );

        var response = powerBiClient.Reports.GenerateToken(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            reportId,
            tokenRequest);

        return response.Token;
    }

    public async Task<bool> DeleteReport(Guid reportId)
    {
        var powerBiClient = GetPowerBiClient();
        var response = await powerBiClient.Reports.DeleteReportInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            reportId
        );

        return response.Response.IsSuccessStatusCode;
    }

    public async Task<Dashboard?> GetDashboard(Guid dashboardId)
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Dashboards.GetDashboardInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            dashboardId);

        return response.Response.IsSuccessStatusCode ? response.Body : null;
    }

    public async Task<IEnumerable<Dashboard>> GetDashboards()
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Dashboards.GetDashboardsInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId));

        return response.Response.IsSuccessStatusCode ? response.Body.Value : new List<Dashboard>();
    }

    public async Task<Dashboard?> CreateDashboard(string name)
    {
        var powerBiClient = GetPowerBiClient();

        var addDashboardRequest = new AddDashboardRequest { Name = name };

        var response = await powerBiClient.Dashboards.AddDashboardInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            addDashboardRequest);

        return response.Response.IsSuccessStatusCode ? response.Body : null;
    }

    public async Task<Dataset?> GetDataset(Guid datasetId)
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Datasets.GetDatasetInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            datasetId.ToString());

        return response.Response.IsSuccessStatusCode ? response.Body : null;
    }

    public async Task<IEnumerable<Dataset>> GetDatasets()
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Datasets.GetDatasetsInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId));

        return response.Response.IsSuccessStatusCode ? response.Body.Value : new List<Dataset>();
    }

    public async Task<Dataset?> CreateDataset(string datasetName, IEnumerable<Column> columns)
    {
        var powerBiClient = GetPowerBiClient();

        var table = new Table("Data", columns.ToList());

        var createDatasetRequest = new CreateDatasetRequest(
            datasetName,
            new List<Table> { table },
            defaultMode: DatasetMode.Push);

        var response = await powerBiClient.Datasets.PostDatasetInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            createDatasetRequest
        );

        return response.Response.IsSuccessStatusCode ? response.Body : null;
    }

    public async Task<bool> PushRowsToDataset(Guid datasetId, List<object> rows, string tableName = "Data")
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Datasets.PostRowsInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            datasetId.ToString(),
            tableName,
            new PostRowsRequest(rows)
        );

        return response.Response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteDataset(Guid datasetId)
    {
        var powerBiClient = GetPowerBiClient();

        var response = await powerBiClient.Datasets.DeleteDatasetInGroupWithHttpMessagesAsync(
            Guid.Parse(_powerBiOptions.Value.GroupId),
            datasetId.ToString()
        );

        return response.Response.IsSuccessStatusCode;
    }
}