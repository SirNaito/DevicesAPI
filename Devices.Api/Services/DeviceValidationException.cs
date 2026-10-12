namespace Devices.Api.Services;

public sealed class DeviceValidationException : Exception
{
    // Treatment in case the device is not valid for any reason
    public DeviceValidationException(string message)
        : base(message)
    {
    }
}
