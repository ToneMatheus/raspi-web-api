namespace api_raspi_web.Models
{
    public class CanTransaction
    {
        public int CanTransactionId { get; set; }

        public int CanItemId { get; set; }
        public Item CanItem { get; set; }

        public int CanBalanceId { get; set; }
        public Balance CanBalance { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
