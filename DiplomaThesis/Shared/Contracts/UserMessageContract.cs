namespace DiplomaThesis.Shared.Contracts;

public class UserMessageContract
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Message { get; set; }
    public DateTime DateSent { get; set; }
}
