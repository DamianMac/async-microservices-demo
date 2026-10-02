using System.Collections.Concurrent;

namespace PaymentGateway.Payments;

// A deliberately flaky pretend card processor. The first attempt at each order fails, and
// the retry takes 30 seconds before it succeeds, so the demo shows Nimbus retrying and the
// order sitting in Pending for a while. Nothing is ever declined.
//
// State is in memory, so a restart forgets which orders have been tried (and fails them
// again) and which have been charged.
public class MockCardProcessor(ILogger<MockCardProcessor> logger)
{
    private static readonly TimeSpan ProcessingTime = TimeSpan.FromSeconds(30);

    private readonly ConcurrentDictionary<Guid, int> _attempts = new();
    private readonly ConcurrentDictionary<Guid, string> _transactions = new();

    public async Task<string> Charge(Guid orderId, decimal amount, string currency, string cardBrand, string cardLast4)
    {
        // A redelivery of an order that's already been charged gets the same transaction back.
        if (_transactions.TryGetValue(orderId, out var existing))
        {
            logger.LogInformation("Order {OrderId} was already charged as {TransactionId}", orderId, existing);
            return existing;
        }

        var attempt = _attempts.AddOrUpdate(orderId, 1, (_, n) => n + 1);
        if (attempt == 1)
        {
            logger.LogWarning("Card processor unavailable charging {CardBrand} ending {CardLast4} for order {OrderId}", cardBrand, cardLast4, orderId);
            throw new CardProcessorUnavailableException(orderId);
        }

        logger.LogInformation("Charging {Amount} {Currency} to {CardBrand} ending {CardLast4} for order {OrderId} (attempt {Attempt})",
            amount, currency, cardBrand, cardLast4, orderId, attempt);
        await Task.Delay(ProcessingTime);

        var transactionId = $"txn_{Guid.NewGuid():N}";
        _transactions[orderId] = transactionId;
        return transactionId;
    }
}

public class CardProcessorUnavailableException(Guid orderId)
    : Exception($"The card processor didn't respond while charging order {orderId}.");
