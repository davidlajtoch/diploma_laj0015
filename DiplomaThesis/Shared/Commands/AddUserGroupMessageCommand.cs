namespace DiplomaThesis.Shared.Commands;

public class AddUserGroupMessageCommand
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; }
    public DateTime DateSent { get; set; }
    public Guid UserGroupId { get; set; }
}