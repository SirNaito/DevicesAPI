using System.ComponentModel.DataAnnotations;
using Devices.Api.Models;

namespace Devices.Api.DTOs;

public sealed class PatchDeviceRequest
{
    // Device Properties (Name, Brand and State) with optional values for patching
    [StringLength(120, MinimumLength = 1)]
    public string? Name { get; init; }

    [StringLength(80, MinimumLength = 1)]
    public string? Brand { get; init; }

    public DeviceState? State { get; init; }
}
