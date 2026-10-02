namespace Orders.Domain;

public class OrderLine
{
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public string Sku { get; set; } = "";
    public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
