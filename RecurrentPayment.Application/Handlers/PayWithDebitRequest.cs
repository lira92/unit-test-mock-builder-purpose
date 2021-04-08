namespace RecurrentPayment.Application.Handlers
{
    public class PayWithDebitRequest
    {
        public int ClientId { get; set; }
        public decimal Value { get; set; }
    }
}
