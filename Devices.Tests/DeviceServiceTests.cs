using Devices.Api.Data;
using Devices.Api.DTOs;
using Devices.Api.Models;
using Devices.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Devices.Tests;

public sealed class DeviceServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DevicesDbContext _dbContext;
    private readonly DeviceService _service;

    // Initialize the in-memory SQLite database and the DeviceService for testing
    public DeviceServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseSqlite(_connection)
            .Options;

        _dbContext = new DevicesDbContext(options);
        _dbContext.Database.EnsureCreated();

        _service = new DeviceService(_dbContext);
    }

    // Test to ensure that creating a device persists it in the database
    [Fact]
    public async Task CreateAsync_PersistsDevice()
    {
        var result = await _service.CreateAsync(
            new CreateDeviceRequest
            {
                Name = "iPhone 15",
                Brand = "Apple"
            },
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("iPhone 15", result.Name);
        Assert.Equal("Apple", result.Brand);
        Assert.Equal(DeviceState.Available, result.State);
        Assert.NotEqual(default, result.CreatedAt);

        var stored = await _dbContext.Devices
            .AsNoTracking()
            .SingleAsync(device => device.Id == result.Id);

        Assert.Equal(result.Id, stored.Id);
    }

    // Test to ensure that GetAllAsync returns all devices when no filters are applied
    [Fact]
    public async Task GetAllAsync_WithoutFilters_ReturnsAllDevices()
    {
        await AddDeviceAsync("Galaxy", "Samsung");
        await AddDeviceAsync("iPhone", "Apple");

        var result = await _service.GetAllAsync(
            null, null, CancellationToken.None);

        Assert.Equal(2, result.Count);
    }

    // Test to ensure that GetAllAsync filters devices by brand in a case-insensitive manner
    [Fact]
    public async Task GetAllAsync_WithBrandFilter_IsCaseInsensitive()
    {
        var samsung = await AddDeviceAsync("Galaxy", "Samsung");
        await AddDeviceAsync("iPhone", "Apple");

        var result = await _service.GetAllAsync(
            "samsung", null, CancellationToken.None);

        var device = Assert.Single(result);
        Assert.Equal(samsung.Id, device.Id);
    }


    // Test to ensure that GetAllAsync filters devices by state correctly
    [Fact]
    public async Task GetAllAsync_WithStateFilter_ReturnsMatchingDevices()
    {
        await AddDeviceAsync(
            "Available Device", "Brand A", DeviceState.Available);

        var inactive = await AddDeviceAsync(
            "Inactive Device", "Brand B", DeviceState.Inactive);

        var result = await _service.GetAllAsync(
            null, "inactive", CancellationToken.None);

        var device = Assert.Single(result);
        Assert.Equal(inactive.Id, device.Id);
    }

    // Test to ensure that GetAllAsync throws a validation exception when an invalid state filter is provided
    [Fact]
    public async Task GetAllAsync_WithInvalidState_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<DeviceValidationException>(
            async () => await _service.GetAllAsync(
                null, "broken", CancellationToken.None));
    }

    // Test to ensure that GetByIdAsync returns null when a device with the specified ID does not exist
    [Fact]
    public async Task GetByIdAsync_WhenDeviceDoesNotExist_ReturnsNull()
    {
        var result = await _service.GetByIdAsync(
            Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    // Test to ensure that Creation time is preserved when updating a device
    [Fact]
    public async Task UpdateAsync_UpdatesFieldsAndPreservesCreationTime()
    {
        var device = await AddDeviceAsync("Old Name", "Old Brand");
        var originalCreatedAt = device.CreatedAt;

        var result = await _service.UpdateAsync(
            device.Id,
            new UpdateDeviceRequest
            {
                Name = "New Name",
                Brand = "New Brand",
                State = DeviceState.Inactive
            },
            CancellationToken.None);

        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Brand", result.Brand);
        Assert.Equal(DeviceState.Inactive, result.State);
        Assert.Equal(originalCreatedAt, result.CreatedAt);

        var stored = await _dbContext.Devices
            .AsNoTracking()
            .SingleAsync(item => item.Id == device.Id);

        Assert.Equal(originalCreatedAt, stored.CreatedAt);
    }

    // Test to ensure that UpdateAsync throws a conflict exception when trying to change the name of a device that is in use
    [Fact]
    public async Task UpdateAsync_WhenNameChangesInUse_ThrowsConflict()
    {
        var device = await AddDeviceAsync(
            "Device", "Brand", DeviceState.InUse);

        await Assert.ThrowsAsync<DeviceConflictException>(
            async () => await _service.UpdateAsync(
                device.Id,
                new UpdateDeviceRequest
                {
                    Name = "New Name",
                    Brand = "Brand",
                    State = DeviceState.Available
                },
                CancellationToken.None));
    }

    // Test to ensure that only the provided fields are updated when using PatchAsync
    [Fact]
    public async Task PatchAsync_UpdatesOnlyProvidedFields()
    {
        var device = await AddDeviceAsync(
            "Original Name", "Original Brand", DeviceState.Available);

        var result = await _service.PatchAsync(
            device.Id,
            new PatchDeviceRequest
            {
                Brand = "New Brand"
            },
            CancellationToken.None);

        Assert.Equal("Original Name", result.Name);
        Assert.Equal("New Brand", result.Brand);
        Assert.Equal(DeviceState.Available, result.State);
    }

    // Test to ensure that PatchAsync throws a conflict exception when trying to change the brand of a device that is in use
    [Fact]
    public async Task PatchAsync_WhenBrandChangesInUse_ThrowsConflict()
    {
        var device = await AddDeviceAsync(
            "Device", "Original Brand", DeviceState.InUse);

        await Assert.ThrowsAsync<DeviceConflictException>(
            async () => await _service.PatchAsync(
                device.Id,
                new PatchDeviceRequest { Brand = "New Brand" },
                CancellationToken.None));
    }

    // Test to ensure that PatchAsync allows changing the state of a device even when it is in use
    [Fact]
    public async Task PatchAsync_AllowsStateOnlyChangeWhenInUse()
    {
        var device = await AddDeviceAsync(
            "Device", "Brand", DeviceState.InUse);

        var result = await _service.PatchAsync(
            device.Id,
            new PatchDeviceRequest
            {
                State = DeviceState.Available
            },
            CancellationToken.None);

        Assert.Equal(DeviceState.Available, result.State);
        Assert.Equal("Device", result.Name);
        Assert.Equal("Brand", result.Brand);
    }

    // Test to ensure that PatchAsync throws a validation exception when no fields are provided for update
    [Fact]
    public async Task PatchAsync_WithNoFields_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<DeviceValidationException>(
            async () => await _service.PatchAsync(
                Guid.NewGuid(),
                new PatchDeviceRequest(),
                CancellationToken.None));
    }

    // Test to ensure that DeleteAsync throws a conflict exception when trying to delete a device that is in use
    [Fact]
    public async Task DeleteAsync_WhenDeviceIsInUse_ThrowsConflict()
    {
        var device = await AddDeviceAsync(
            "Device", "Brand", DeviceState.InUse);

        await Assert.ThrowsAsync<DeviceConflictException>(
            () => _service.DeleteAsync(
                device.Id, CancellationToken.None));

        var exists = await _dbContext.Devices
            .AsNoTracking()
            .AnyAsync(item => item.Id == device.Id);

        Assert.True(exists);
    }

    // Test to ensure that DeleteAsync successfully deletes a device when it is available
    [Fact]
    public async Task DeleteAsync_WhenDeviceIsAvailable_DeletesDevice()
    {
        var device = await AddDeviceAsync(
            "Device", "Brand", DeviceState.Available);

        await _service.DeleteAsync(
            device.Id, CancellationToken.None);

        var exists = await _dbContext.Devices
            .AsNoTracking()
            .AnyAsync(item => item.Id == device.Id);

        Assert.False(exists);
    }

    // Helper method to add a device to the in-memory database for testing purposes
    private async Task<Device> AddDeviceAsync(
        string name,
        string brand,
        DeviceState state = DeviceState.Available)
    {
        var device = new Device(name, brand, state);

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync();

        return device;
    }

    // Dispose method to clean up resources after tests are completed
    public void Dispose()
    {
        _dbContext.Dispose();
        _connection.Dispose();
    }
}
