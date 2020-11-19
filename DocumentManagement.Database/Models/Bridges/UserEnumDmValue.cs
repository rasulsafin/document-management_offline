namespace DocumentManagement.Database.Models
{
    public class UserEnumDmValue
    {
        public int EnumDmValueID { get; set; }
        public EnumDmValue EnumDmValue { get; set; }

        public int UserID { get; set; }
        public User User { get; set; }
    }
}
