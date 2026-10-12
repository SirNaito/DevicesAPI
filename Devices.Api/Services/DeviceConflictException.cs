namespace Devices.Api.Services;

public sealed class DeviceConflictException : Exception
{
    // Treatment in case that a device with the specified ID already exists in the database.
    public DeviceConflictException(string message)
        : base(message)
    {
    }
}
