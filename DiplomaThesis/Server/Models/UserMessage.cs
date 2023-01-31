namespace DiplomaThesis.Server.Models
{
    public class UserMessage
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Message { get; set; }
        public bool CurrentUser { get; set; }
        public DateTime DateSent { get; set; }
    }
}
