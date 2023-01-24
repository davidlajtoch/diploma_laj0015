namespace DiplomaThesis.Server.Models
{
    public class Activity
    {
        public Guid Id { get; set; }
        public string Message { get; init; }
        public DateTime? Created { get; init; }
        public Guid? UserGroupId { get; init; }
        public UserGroup? UserGroup { get; init; }
        
    }
}
