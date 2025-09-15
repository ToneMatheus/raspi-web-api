namespace api_raspi_web.Models
{
    public class CanTransactionUser
    {
        public int CanTransactionUserId { get; set; }

        public int CanItemUserId { get; set; }
        public CanItemUser CanItemUser { get; set; }

        public int CanBalanceUserId { get; set; }
        public CanBalanceUser CanBalanceUser { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
