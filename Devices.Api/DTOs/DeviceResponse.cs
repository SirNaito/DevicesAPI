using Devices.Api.Models;

namespace Devices.Api.DTOs;

// This record represents the response for a device including his ID, name, brand, state, and creation timestamp
public sealed record DeviceResponse(
    Guid Id,
    string Name,
    string Brand,
    DeviceState State,
    DateTimeOffset CreatedAt
);
