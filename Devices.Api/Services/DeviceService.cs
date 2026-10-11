using Devices.Api.Data;
using Devices.Api.DTOs;
using Devices.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Devices.Api.Services;

public sealed class DeviceService : IDeviceService
{
    //  Dependency injection of the DevicesDbContext
    private readonly DevicesDbContext _dbContext;

    public DeviceService(DevicesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    //  Method to create a new device using Async/Await pattern
    public async Task<DeviceResponse> CreateAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = new Device(
            request.Name.Trim(),
            request.Brand.Trim(),
            request.State);

        _dbContext.Devices.Add(device);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(device);
    }

    //  Method to retrieve all devices using Async/Await pattern
    public async Task<IReadOnlyList<DeviceResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var devices = await _dbContext.Devices
            .AsNoTracking()
            .OrderBy(device => device.Name)
            .ToListAsync(cancellationToken);

        return devices.Select(MapToResponse).ToList();
    }

    //  Method to retrieve a device by its ID using Async/Await pattern
    public async Task<DeviceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                device => device.Id == id,
                cancellationToken);

        return device is null
            ? null
            : MapToResponse(device);
    }

    //  Method to update an existing device
    private static DeviceResponse MapToResponse(Device device)
    {
        return new DeviceResponse(
            device.Id,
            device.Name,
            device.Brand,
            device.State,
            device.CreatedAt);
    }
}
