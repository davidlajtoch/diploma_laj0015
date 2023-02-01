using EntityFrameworkCore.EncryptColumn.Attribute;

namespace DiplomaThesis.Server.Models
{
    public class UserGroupMessage
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }

        [EncryptColumn]
        public string Message { get; set; }

        public DateTime DateSent { get; set; }
        public Guid UserGroupId { get; set; }
    }
}
