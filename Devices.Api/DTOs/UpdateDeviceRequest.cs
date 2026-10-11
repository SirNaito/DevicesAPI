using System.ComponentModel.DataAnnotations;
using Devices.Api.Models;

namespace Devices.Api.DTOs;

public sealed class UpdateDeviceRequest
{
    // Properties for the UpdateDeviceRequest class with validation attributes
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [StringLength(80, MinimumLength = 1)]
    public string Brand { get; init; } = string.Empty;

    [Required]
    public DeviceState? State { get; init; }
}
