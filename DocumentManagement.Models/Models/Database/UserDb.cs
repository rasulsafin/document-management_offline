using System.Collections.Generic;

namespace DocumentManagement.Models.Database
{
    public class UserDb
    {
        public int Id { get; set; }

        public string Login { get; set; }
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public ICollection<ProjectUsers> Projects { get; set; }
        public ICollection<TaskDmDb> Tasks { get; set; }
    }
}
