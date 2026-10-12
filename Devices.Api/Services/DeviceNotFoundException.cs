namespace Devices.Api.Services;

public sealed class DeviceNotFoundException : Exception
{
    // Treatment for the case when a device with the specified ID is not found in the database.
    public DeviceNotFoundException(Guid id)
        : base($"Device with the ID '{id}' was not found.")
    {
    }
}
