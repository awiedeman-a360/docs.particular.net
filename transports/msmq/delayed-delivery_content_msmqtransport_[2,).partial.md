Because MSMQ lacks a mechanism for sending delayed messages, the MSMQ transport uses an external store for delayed messages. Messages that are to be delivered later (e.g. [saga timeouts](/nservicebus/sagas/timeouts.md) or [delayed retries](/nservicebus/recoverability/configure-delayed-retries.md)) are persisted in the delayed message store until they are due. When a message is due, it is retreived from the store and dispatched to its destination.

The MSMQ transport requires explicit configuration to enable delayed message delivery. For example:

snippet: delayed-delivery

The SQL Server delayed message store (`SqlServerDelayedMessageStore`) is the only delayed message store that ships with the MSMQ transport.

### How it works

A delayed message store implements the `IDelayedMessageStore` interface. Delayed message delivery has two parts:

### Storing of delayed messages

A delayed message is stored using the `Store` method.

### Polling and dispatching of delayed messages

The message store is polled for due delayed messages in a background task which periodically calls `FetchNextDueTimeout`. If the method returns a message, the message is sent (see next paragraph), and the method is immediately called again. If the method returns `null`, `Next` is called, which returns either a `DateTimeOffset` indicating when next message will be due, or `null` if there are no delayed messages. `FetchNextDueTimeout` will be called. If another delayed message is persisted in the meantime, using the `Store` method.

When a due delayed message is returned by `FetchNextDueTimeout`, the message is sent to the destination and then removed from the store using the `Remove` method. In case of an unexpected exception during forwarding the failure is registered using `IncrementFailureCount`. If the configured number of retries is exhausted the message is forwarded to the configured `error` queue.

## Using a custom delayed message store

Implement the `IDelayedMessageStore` interface and pass

### Consistency

In the `TransactionScope` [transaction mode](/transports/transactions.md) the delayed message store is expected to enlist in the `TransactionScope` to ensure exact-once semantics. The `FetchNextDueTimeout` and `Remove` storage operations and the dispatch to the destination queue transport operation are executed in a single distributed transaction. The built-in SQL Server store supports this mode of operation.

In lower transaction modes the dispatch behavior is **At-least-once**. The `FetchNextDueTimeout` and `Remove` storage operations are executed in the same storage `TransactionScope` but the dispatching is executed in a separate (inner) transport scope. In case of failure in `Remove`, the message will be sent to the destination multiple times. The destination endpoint has to handle the duplicates, either via the [Outbox](/nservicebus/outbox/) or custom deduplication mechanism.

The built-in SQL Server store applies a pessimistic lock on the delayed message row in the `FetchNextDueTimeout` operation to prevent multiple instances of the same endpoint from attempting to deliver the same due delayed message. Custom implementation of the store are also expected to use some form of a lock mechanism (e.g. Blob Storage lease locks).
