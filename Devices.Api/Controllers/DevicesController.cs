using Devices.Api.DTOs;
using Devices.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Devices.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    // Constructor for the DevicesController class 
    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //Method to create a new device
    public async Task<ActionResult<DeviceResponse>> Create(
        [FromBody] CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtRoute(
            "GetDeviceById",
            new { id = device.Id },
            device);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DeviceResponse>), StatusCodes.Status200OK)]
    //Method to get all devices
    public async Task<ActionResult<IReadOnlyList<DeviceResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var devices = await _deviceService.GetAllAsync(
            cancellationToken);

        return Ok(devices);
    }

    [HttpGet("{id:guid}", Name = "GetDeviceById")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //Method to get a device by its ID
    public async Task<ActionResult<DeviceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetByIdAsync(
            id,
            cancellationToken);

        if (device is null)
        {
            return NotFound();
        }

        return Ok(device);
    }
}
