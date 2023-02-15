namespace DiplomaThesis.Server.Models;

public class UserGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? LeaderId { get; set; }
    public List<ApplicationUser>? Users { get; set; }
    public List<ReportDb>? Reports { get; set; }
}