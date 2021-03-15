using System;

namespace MRS.DocumentManagement.Database.Models
{
    public class Synchronization
    {
        public int ID { get; set; }

        public int UserID { get; set; }

        public User User { get; set; }

        public DateTime Date { get; set; }
    }
}
