using Devices.Api.Data;
using Devices.Api.DTOs;
using Devices.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Devices.Api.Services;

public sealed class DeviceService : IDeviceService
{
    // DI for the DevicesDbContext to interact with the database
    private readonly DevicesDbContext _dbContext;

    public DeviceService(DevicesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Method to create a new device in the database using Async/Await pattern
    public async Task<DeviceResponse> CreateAsync(
        CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var name = ValidateText(request.Name, "Name");
        var brand = ValidateText(request.Brand, "Brand");

        var device = new Device(name, brand, request.State);

        _dbContext.Devices.Add(device);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(device);
    }

    // Method to get all devices from the database using Async/Await pattern

    public async Task<IReadOnlyList<DeviceResponse>> GetAllAsync(
        string? brand,
        string? state,
        CancellationToken cancellationToken)
    {
        IQueryable<Device> query = _dbContext.Devices.AsNoTracking();

        if (brand is not null)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new DeviceValidationException(
                    "Brand filter cannot be empty.");
            }

            var normalizedBrand = brand.Trim();

            query = query.Where(device =>
                EF.Functions.Collate(device.Brand, "NOCASE")
                == normalizedBrand);
        }

        var parsedState = ParseState(state);

        if (parsedState is not null)
        {
            query = query.Where(device =>
                device.State == parsedState.Value);
        }

        var devices = await query
            .OrderBy(device => device.Name)
            .ToListAsync(cancellationToken);

        return devices.Select(MapToResponse).ToList();
    }


    // Method to get a device by its ID from the database using Async/Await pattern
    public async Task<DeviceResponse?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(
                device => device.Id == id,
                cancellationToken);

        return device is null ? null : MapToResponse(device);
    }

    // Method to update a device in the database using Async/Await pattern
    public async Task<DeviceResponse> UpdateAsync(
        Guid id,
        UpdateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = await FindTrackedDeviceAsync(id, cancellationToken);

        var name = ValidateText(request.Name, "Name");
        var brand = ValidateText(request.Brand, "Brand");

        if (request.State is null)
        {
            throw new DeviceValidationException(
                "State must be provided for a full update.");
        }

        EnsureNameAndBrandCanChange(device, name, brand);

        device.Name = name;
        device.Brand = brand;
        device.State = request.State.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(device);
    }


    // Method to partially update a device in the database using Async/Await pattern
    public async Task<DeviceResponse> PatchAsync(
        Guid id,
        PatchDeviceRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Name is null &&
            request.Brand is null &&
            request.State is null)
        {
            throw new DeviceValidationException(
                "At least one field must be provided for a partial update.");
        }

        var device = await FindTrackedDeviceAsync(id, cancellationToken);

        var name = request.Name is null
            ? device.Name
            : ValidateText(request.Name, "Name");

        var brand = request.Brand is null
            ? device.Brand
            : ValidateText(request.Brand, "Brand");

        EnsureNameAndBrandCanChange(device, name, brand);

        device.Name = name;
        device.Brand = brand;

        if (request.State is not null)
        {
            device.State = request.State.Value;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToResponse(device);
    }

    // Method to find a tracked device in the database by its ID using Async/Await pattern
    private async Task<Device> FindTrackedDeviceAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(
                device => device.Id == id,
                cancellationToken);

        return device ?? throw new DeviceNotFoundException(id);
    }

    // Method to validate text fields (name and brand) to ensure they are not empty or whitespace
    private static string ValidateText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DeviceValidationException(
                $"{fieldName} cannot be empty or whitespace.");
        }

        return value.Trim();
    }

    // Method to ensure that the name and brand of a device can be changed based on its state
    private static void EnsureNameAndBrandCanChange(
        Device device,
        string newName,
        string newBrand)
    {
        var nameChanged = !string.Equals(
            device.Name, newName, StringComparison.Ordinal);

        var brandChanged = !string.Equals(
            device.Brand, newBrand, StringComparison.Ordinal);

        if (device.State == DeviceState.InUse &&
            (nameChanged || brandChanged))
        {
            throw new DeviceConflictException(
                "Name and brand cannot be changed while the device is in use.");
        }
    }

    // Method to parse the state string into a DeviceState enum value, or return null if the state is not provided
    private static DeviceState? ParseState(string? state)
    {
        if (state is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new DeviceValidationException(
                "State filter cannot be empty.");
        }

        return state.Trim().ToLowerInvariant() switch
        {
            "available" => DeviceState.Available,
            "in-use" => DeviceState.InUse,
            "inactive" => DeviceState.Inactive,
            _ => throw new DeviceValidationException(
                "State must be available, in-use, or inactive.")
        };
    }

    // Method to delete a device from the database using Async/Await pattern
    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await FindTrackedDeviceAsync(
            id, cancellationToken);

        if (device.State == DeviceState.InUse)
        {
            throw new DeviceConflictException(
                "Devices in use cannot be deleted.");
        }

        _dbContext.Devices.Remove(device);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }


    // Method to map a Device entity to a DeviceResponse DTO
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
