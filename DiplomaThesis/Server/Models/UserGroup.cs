namespace DiplomaThesis.Server.Models;

public class UserGroup
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public List<ApplicationUser>? Users { get; set; }
    public ICollection<ReportDb>? Reports { get; set; }
}