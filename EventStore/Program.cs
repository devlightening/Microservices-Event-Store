#region İnceleme
//using EventStore.Client;
//using System.Text.Json;

//string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false";
//var settings = EventStoreClientSettings.Create(connectionString);
//var client = new EventStoreClient(settings);


//OrderPlacedEvent orderPlacedEvent = new()
//{
//    OrderId = 1,
//    TotalAmount = 1000
//};

////while (true)
////{
////    EventData eventData = new(
////     eventId: Uuid.NewUuid(),
////     type: orderPlacedEvent.GetType().Name,
////     data: JsonSerializer.SerializeToUtf8Bytes(orderPlacedEvent)
////    );

////    await client.AppendToStreamAsync(
////        streamName: "order-stream",
////        expectedState: StreamState.Any,
////        eventData: new[] { eventData }
////        );
////}

////var events = client.ReadStreamAsync(
////    streamName: "order-stream",
////    direction: Direction.Forwards,
////    revision: StreamPosition.Start
////    );

////var datas = await events.ToListAsync();
////Console.WriteLine(  );

//await client.SubscribeToStreamAsync(
//    streamName: "order-stream",
//    start: FromStream.Start,
//    eventAppeared: async (streamSubscription, resolvedEvent, cancellationToken) =>
//    {
//        OrderPlacedEvent @event = JsonSerializer.Deserialize<OrderPlacedEvent>(resolvedEvent.Event.Data.ToArray());
//        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(@event));
//    },
//    subscriptionDropped: (streamSubscription, subscriptionDroppedReason, exception) => Console.WriteLine("Disconnected")
//    );

//Console.Read();

//class OrderPlacedEvent
//{
//    public int OrderId { get; set; }
//    public int TotalAmount { get; set; }
//}
#endregion


#region Bakiye Örnek

using EventStore.Client;
using System.Text.Json;
using System.Text.Json.Serialization;

EventStoreService eventStoreService = new();

AccountCreatedEvent accountCreatedEvent = new()
{
    AccountId = "060719",
    CostumerId = "98765",
    StartBalance = 0,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent1 = new()
{
    AccountId = "060719",
    Amount = 750,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent2 = new()
{
    AccountId = "060719",
    Amount = 1200,
    Date = DateTime.UtcNow.Date
};
MoneyWithdrawnEvent moneyWithdrawnEvent = new()
{
    AccountId = "060719",
    Amount = 300,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent3 = new()
{
    AccountId = "060719",
    Amount = 800,
    Date = DateTime.UtcNow.Date
};
MoneyTransferredEvent moneyTransferredEvent1 = new()
{
    AccountId = "060719",
    Amount = 450,
    Date = DateTime.UtcNow.Date
};
MoneyTransferredEvent moneyTransferredEvent2 = new()
{
    AccountId = "060719",
    Amount = 100,
    Date = DateTime.UtcNow.Date
};
MoneyDepositedEvent moneyDepositedEvent4 = new()
{
    AccountId = "060719",
    Amount = 1500,
    Date = DateTime.UtcNow.Date
};


await eventStoreService.AppendToStreamAsync(
    streamName: $"costumer-{accountCreatedEvent.CostumerId}-stream",
    new[] {
        eventStoreService.GenerateEventData(accountCreatedEvent),
        eventStoreService.GenerateEventData(moneyDepositedEvent1),
        eventStoreService.GenerateEventData(moneyDepositedEvent2),
        eventStoreService.GenerateEventData(moneyWithdrawnEvent),
        eventStoreService.GenerateEventData(moneyDepositedEvent3),
        eventStoreService.GenerateEventData(moneyTransferredEvent1),
        eventStoreService.GenerateEventData(moneyTransferredEvent2),
        eventStoreService.GenerateEventData(moneyDepositedEvent4)
    }
);

BalanceInfo balanceInfo = new();
await eventStoreService.SubscribeToStreamAsync(
    streamName: $"costumer-{accountCreatedEvent.CostumerId}-stream",
    async (ss, re, ct) =>
    {
        string eventType = re.Event.EventType;
        object @event = JsonSerializer.Deserialize(re.Event.Data.ToArray(), Type.GetType(eventType));

        string transactionDescription = "";

        switch (@event)
        {
            case AccountCreatedEvent e:
                balanceInfo.AccountId = e.AccountId;
                balanceInfo.Balance = e.StartBalance;
                transactionDescription = $"Hesap oluşturuldu. Başlangıç bakiyesi: {e.StartBalance}";
                break;
            case MoneyDepositedEvent e:
                balanceInfo.Balance += e.Amount;
                transactionDescription = $"{e.Amount} TL para yatırıldı.";
                break;
            case MoneyWithdrawnEvent e:
                balanceInfo.Balance -= e.Amount;
                transactionDescription = $"{e.Amount} TL para çekildi.";
                break;
            case MoneyTransferredEvent e:
                balanceInfo.Balance -= e.Amount;
                transactionDescription = $"{e.Amount} TL para transfer edildi.";
                break;
        }

       
        await Console.Out.WriteLineAsync("--------------------------------------------------");
        await Console.Out.WriteLineAsync($"Event Type: {eventType}");
        await Console.Out.WriteLineAsync($"Transaction: {transactionDescription}");
        await Console.Out.WriteLineAsync($"Current Balance: {balanceInfo.Balance}");
        await Console.Out.WriteLineAsync("--------------------------------------------------");
        await Console.Out.WriteLineAsync(JsonSerializer.Serialize(@event, new JsonSerializerOptions { WriteIndented = true })); // Olay verisini daha okunaklı formatta yazdır
        await Console.Out.WriteLineAsync();
    }
);

Console.Read();


class EventStoreService
{
    EventStoreClientSettings GetEventStoreClientSettings(string connectionString = "esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false")
        => EventStoreClientSettings.Create(connectionString);
    EventStoreClient Client { get => new EventStoreClient(GetEventStoreClientSettings()); }
    public async Task AppendToStreamAsync(string streamName, IEnumerable<EventData> eventData)
        => await Client.AppendToStreamAsync(
            streamName: streamName,
            eventData: eventData,
            expectedState: StreamState.Any
            );
    public EventData GenerateEventData(object @event)
        => new(
            eventId: Uuid.NewUuid(),
            type: @event.GetType().Name,
            data: JsonSerializer.SerializeToUtf8Bytes(@event)
            );

    public async Task SubscribeToStreamAsync(string streamName, Func<StreamSubscription, ResolvedEvent, CancellationToken, Task> eventAppeared)
        => Client.SubscribeToStreamAsync(
            streamName: streamName,
            start: FromStream.Start,
            eventAppeared: eventAppeared,
            subscriptionDropped: (x, y, z) => Console.WriteLine("Disconnected!")
            );
}

class BalanceInfo
{
    public string AccountId { get; set; }
    public int Balance { get; set; }
}

class AccountCreatedEvent
{
    public string AccountId { get; set; }
    public string CostumerId { get; set; }
    public int StartBalance { get; set; }
    public DateTime Date { get; set; }
}

class MoneyDepositedEvent
{
    public string AccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}

class MoneyWithdrawnEvent
{
    public string AccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}

class MoneyTransferredEvent
{
    public string AccountId { get; set; }
    public string TargetAccountId { get; set; }
    public int Amount { get; set; }
    public DateTime Date { get; set; }
}

#endregion