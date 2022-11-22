namespace DiplomaThesis.Server.Models;

public class ReportDb
{
    public Guid Id { get; set; }
    public Guid PowerBiId { get; set; }
    public Guid? UserGroupId { get; set; }
    public UserGroup? UserGroup { get; set; }
}