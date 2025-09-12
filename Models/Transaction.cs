namespace api_raspi_web.Models
{
    public class Transaction
    {
        public int TransactionId { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int BalanceId { get; set; }
        public Balance Balance { get; set; }

        public DateTime TransactionDate { get; set; }
    }
}
