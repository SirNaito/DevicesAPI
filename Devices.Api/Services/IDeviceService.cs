using Devices.Api.DTOs;

namespace Devices.Api.Services;

public interface IDeviceService
{
    Task<DeviceResponse> CreateAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DeviceResponse>> GetAllAsync(
        CancellationToken cancellationToken);

    Task<DeviceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<DeviceResponse> UpdateAsync(
        Guid id,
        UpdateDeviceRequest request,
        CancellationToken cancellationToken);

    Task<DeviceResponse> PatchAsync(
        Guid id,
        PatchDeviceRequest request,
        CancellationToken cancellationToken);
}
