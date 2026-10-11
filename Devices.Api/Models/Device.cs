namespace Devices.Api.Models;

public class Device
{
    // Private constructor to prevent direct instantiation of the Device class
    private Device()
    {
        Name = string.Empty;
        Brand = string.Empty;
    }

    //Parameters for the constructor of the Device class
    public Device(
        string name,
        string brand,
        DeviceState state = DeviceState.Available)
    {
        Id = Guid.NewGuid();
        Name = name;
        Brand = brand;
        State = state;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Properties of the Device class
    public Guid Id { get; private set; }

    public string Name { get; set; }

    public string Brand { get; set; }

    public DeviceState State { get; set; }

    public DateTimeOffset CreatedAt { get; private set; }
}
