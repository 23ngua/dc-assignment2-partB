namespace DataWebService.Models.Entities
{
    public class Account
    {
        public int AccNum { get; set; }
        public string? AccName { get; set; }
        public int OwnerID { get; set; } // foreign key to User.UserID
        public long Balance { get; set; } // long to represent cents
        public bool Active { get; set; } = true;
    }
}