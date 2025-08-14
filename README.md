-----

# 🚀 Microservices - Event Store

This project demonstrates a microservices architecture leveraging the **Event Sourcing** and **CQRS (Command Query Responsibility Segregation)** patterns. It uses **EventStoreDB** as a central event log to ensure data integrity and provide a complete history of all changes within the system.

## 🎯 Project Goal

The primary goal of this project is to build a highly available and resilient system where business logic is centered around events. By utilizing Event Sourcing, we maintain an immutable, append-only ledger of all state changes, which enables features like temporal queries and state reconstruction.

## 🏗️ Architecture

The architecture is built around an **Event Store**, which serves as the single source of truth. Services publish events to the Event Store, and other services can subscribe to these events to update their own local state or trigger side effects.

## 📊 Project Workflow: Event Sourcing & CQRS

This diagram, which you can place in a folder like `docs/images/`, illustrates how events are generated and consumed to manage the state of a financial account.

### Flow Description

1.  **Event Creation:** Services (e.g., a Banking API) generate immutable events for every state change (e.g., `AccountCreatedEvent`, `MoneyDepositedEvent`).
2.  **Event Appending:** These events are appended as a continuous stream to **EventStoreDB**. This is the core of the **Event Sourcing** pattern.
3.  **Real-time Projection:** A separate service or a consumer within the application subscribes to the event stream. As new events arrive, it projects these events to build a denormalized **read model**.
4.  **State Reconstruction:** By re-reading all events from the beginning of the stream, the application can reconstruct the exact state of an entity at any point in time.

## ✅ Project Validation: Real-time Balance Projection

The following console output demonstrates how events are consumed in real-time to build and update the account's balance. Each event is processed sequentially, and the current balance is calculated based on the transaction history.

```
--------------------------------------------------
Event Type: AccountCreatedEvent
Transaction: Hesap oluşturuldu. Başlangıç bakiyesi: 0
Current Balance: 0
--------------------------------------------------
{
  "AccountId": "060719",
  "CostumerId": "98765",
  "StartBalance": 0,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyDepositedEvent
Transaction: 750 TL para yatırıldı.
Current Balance: 750
--------------------------------------------------
{
  "AccountId": "060719",
  "Amount": 750,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyDepositedEvent
Transaction: 1200 TL para yatırıldı.
Current Balance: 1950
--------------------------------------------------
{
  "AccountId": "060719",
  "Amount": 1200,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyWithdrawnEvent
Transaction: 300 TL para çekildi.
Current Balance: 1650
--------------------------------------------------
{
  "AccountId": "060719",
  "Amount": 300,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyDepositedEvent
Transaction: 800 TL para yatırıldı.
Current Balance: 2450
--------------------------------------------------
{
  "AccountId": "060719",
  "Amount": 800,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyTransferredEvent
Transaction: 450 TL para transfer edildi.
Current Balance: 2000
--------------------------------------------------
{
  "AccountId": "060719",
  "TargetAccountId": null,
  "Amount": 450,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyTransferredEvent
Transaction: 100 TL para transfer edildi.
Current Balance: 1900
--------------------------------------------------
{
  "AccountId": "060719",
  "TargetAccountId": null,
  "Amount": 100,
  "Date": "2025-08-14T00:00:00Z"
}

--------------------------------------------------
Event Type: MoneyDepositedEvent
Transaction: 1500 TL para yatırıldı.
Current Balance: 3400
--------------------------------------------------
{
  "AccountId": "060719",
  "Amount": 1500,
  "Date": "2025-08-14T00:00:00Z"
}
```

-----

## 📂 Project Structure

### **`EventStoreClient`**

  * **Purpose:** The core class for interacting with EventStoreDB.
  * **Key Components:**
      * `AppendToStreamAsync`: Publishes a batch of events to a specified stream.
      * `SubscribeToStreamAsync`: Sets up a subscription to a stream to receive events in real-time.
      * `GenerateEventData`: Helper method to serialize C\# objects into `EventData` for EventStoreDB.

### **`Shared`**

  * **Purpose:** A common library for shared message contracts (event classes) and settings.
  * **Key Components:**
      * `AccountCreatedEvent.cs`, `MoneyDepositedEvent.cs`, `MoneyWithdrawnEvent.cs`, etc.

### **`Projection`**

  * **Purpose:** The part of the code that rebuilds a simple read model (the `BalanceInfo` object) by consuming events.

-----

## ⚙️ Setup & Run

1.  Start **EventStoreDB** using Docker:
    ```bash
    docker run --name esdb-node -it -p 2113:2113 -p 1113:1113 eventstore/eventstore:22.10.5-jammy --insecure --run-projections=All --enable-atom-pub-over-http
    ```
2.  Ensure your `connectionString` in the project is correct: `esdb://admin:changeit@localhost:2113?tls=false&tlsVerifyCert=false`.
3.  Build and run the project from your IDE (e.g., Visual Studio).
4.  The application will automatically publish events and then subscribe to them, displaying the real-time balance on the console.
