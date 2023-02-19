namespace DiplomaThesis.Server.Models
{
    public class Activity
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime Created { get; set; }
        public string? UserGroupName { get; set; }
        public Guid? UserGroupId { get; set; } 
    }
}
