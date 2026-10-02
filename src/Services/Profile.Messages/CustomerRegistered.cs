using Nimbus.MessageContracts;

namespace Profile.Messages;

// Published by Profile when someone signs up. It carries the name and email so the
// welcome email doesn't need an authenticated call back to Profile.
public class CustomerRegistered : IBusEvent
{
    public int CustomerId { get; set; }
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTimeOffset RegisteredAt { get; set; }
}
