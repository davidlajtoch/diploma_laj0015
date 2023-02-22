namespace DiplomaThesis.Shared.Commands;

public class DeleteUserCommand
{
    public Guid UserId { get; set; } = Guid.Empty;
}