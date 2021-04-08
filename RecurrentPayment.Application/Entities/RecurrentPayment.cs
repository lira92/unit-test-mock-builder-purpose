using System;

namespace RecurrentPayment.Application.Entities
{
    public class RecurrentPayment
    {
        public RecurrentPayment(int clientId, decimal value, Guid transactionId)
        {
            Id = Guid.NewGuid();
            ClientId = clientId;
            Value = value;
            CreatedAt = DateTime.Now;
            TransactionId = transactionId;
        }

        public Guid Id { get; }
        public int ClientId { get; }
        public decimal Value { get; }
        public Guid TransactionId { get; }
        public DateTime CreatedAt { get; }
    }
}
