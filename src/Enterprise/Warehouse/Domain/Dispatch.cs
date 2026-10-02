namespace Warehouse.Domain;

// Marten document: an order to pick, pack and send. The order id is the document id
// (see Program.cs), so a redelivered OrderConfirmedEvent finds the existing dispatch.
public class Dispatch
{
    public Guid OrderId { get; set; }
    public string CustomerName { get; set; } = "";
    public DispatchAddress DeliveryAddress { get; set; } = new();
    public List<PickLine> PickList { get; set; } = [];
    public DispatchStatus Status { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? DispatchedAt { get; set; }

    public int Units => PickList.Sum(l => l.Quantity);

    // The short reference the shop shows customers: the first 8 characters of the id.
    public string Reference => OrderId.ToString("N")[..8].ToUpperInvariant();
}

public class DispatchAddress
{
    public string RecipientName { get; set; } = "";
    public string Line1 { get; set; } = "";
    public string? Line2 { get; set; }
    public string Suburb { get; set; } = "";
    public string State { get; set; } = "";
    public string Postcode { get; set; } = "";
    public string Country { get; set; } = "";
    public string? Phone { get; set; }
}

public class PickLine
{
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
}

public enum DispatchStatus
{
    Pending,
    Dispatched,
}
