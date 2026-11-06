namespace BankWebApp.Models.DTOs
{
    public class AccountDTO
    {
        public int AccNum { get; set; }
        public string AccName { get; set; } = "";
        public int OwnerID { get; set; }
        public long Balance { get; set; }
        public bool Active { get; set; }
    }
}
