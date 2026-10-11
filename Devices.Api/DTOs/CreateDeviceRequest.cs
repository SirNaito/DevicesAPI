using System.ComponentModel.DataAnnotations;
using Devices.Api.Models;

namespace Devices.Api.DTOs;

public sealed class CreateDeviceRequest
{
    // Device Name, Brand and State (and Length set)  
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Brand { get; init; } = string.Empty;

    public DeviceState State { get; init; } = DeviceState.Available;
}
