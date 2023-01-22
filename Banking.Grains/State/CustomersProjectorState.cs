using Banking.Grains.Events;
using System.Text.Json.Serialization;

namespace Banking.Grains.State
{
    [GenerateSerializer]
    public class CustomersProjectorState
    {
        [JsonInclude]
        [Id(0)]
        public int CustomerCount { get; private set; }

        [JsonInclude]
        [Id(1)]
        public int AccountCount { get; private set; }

        [JsonInclude]
        [Id(2)]
        public int TransactionCount { get; private set; }

        [JsonInclude]
        [Id(3)]
        public decimal AccountBalanceSum { get; private set; }
        
        public void Apply(CustomerCreatedEvent _)
        {
            CustomerCount++;
        }

        public void Apply(AccountAddedEvent _)
        {
            AccountCount++;
        }

        public void Apply(AccountRemovedEvent _)
        {
            AccountCount--;
        }

        public void Apply(TransactionPostedEvent @event)
        {
            TransactionCount++;
            AccountBalanceSum += @event.Amount;
        }
    }
}
