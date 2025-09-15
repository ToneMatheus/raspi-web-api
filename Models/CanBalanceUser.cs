namespace api_raspi_web.Models
{
    public class CanBalanceUser
    {
        public int CanBalanceUserId { get; set; }
        public decimal Total { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
