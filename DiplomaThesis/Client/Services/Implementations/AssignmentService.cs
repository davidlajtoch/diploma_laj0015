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
}