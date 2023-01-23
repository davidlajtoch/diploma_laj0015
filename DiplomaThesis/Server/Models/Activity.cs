namespace DiplomaThesis.Server.Models
{
    public class Activity
    {
        public Guid Id { get; set; }
        public string Message { get; init; }
        public UserGroup? UserGroup { get; init; }
        public DateTime? Created { get; init; }
    }
}
