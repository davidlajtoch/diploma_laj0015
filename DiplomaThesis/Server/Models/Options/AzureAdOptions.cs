namespace DiplomaThesis.Server.Models.Options;

public class AzureAdOptions
{
    public string AuthorityUri { get; set; } = null!;
    public string DirectoryId { get; set; } = null!;
    public string GrantType { get; set; } = null!;
    public string Resource { get; set; } = null!;
    public string ClientId { get; set; } = null!;
}