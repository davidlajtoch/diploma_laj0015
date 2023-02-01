using DiplomaThesis.Client;
using DiplomaThesis.Client.Factories;
using DiplomaThesis.Client.Services.Implementations;
using DiplomaThesis.Client.Services.Interfaces;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("DiplomaThesis.ServerAPI",
        client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddSingleton<IFileParsingService, FileParsingService>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DiplomaThesis.ServerAPI"));
builder.Services.AddScoped<IDatasetService, DatasetService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdministrationService, AdministrationService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
builder.Services.AddScoped<IChatService, ChatService>();

builder.Services.AddApiAuthorization()
    .AddAccountClaimsPrincipalFactory<UserWithRolesFactory>();

await builder.Build().RunAsync();