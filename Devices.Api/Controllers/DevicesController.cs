using Devices.Api.DTOs;
using Devices.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Devices.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    // Endpoint to create a new device
    public async Task<ActionResult<DeviceResponse>> Create(
        [FromBody] CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.CreateAsync(
            request, cancellationToken);

        return CreatedAtRoute(
            "GetDeviceById",
            new { id = device.Id },
            device);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DeviceResponse>), StatusCodes.Status200OK)]
    // Endpoint to get all devices
    public async Task<ActionResult<IReadOnlyList<DeviceResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var devices = await _deviceService.GetAllAsync(cancellationToken);
        return Ok(devices);
    }

    [HttpGet("{id:guid}", Name = "GetDeviceById")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // Endpoint to get a device by its ID
    public async Task<ActionResult<DeviceResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var device = await _deviceService.GetByIdAsync(
            id, cancellationToken);

        if (device is null)
        {
            return NotFound();
        }

        return Ok(device);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    // Endpoint to update a device by its ID
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var device = await _deviceService.UpdateAsync(
                id, request, cancellationToken);

            return Ok(device);
        }
        catch (DeviceNotFoundException ex)
        {
            return CreateProblem(404, "Device not found", ex.Message);
        }
        catch (DeviceConflictException ex)
        {
            return CreateProblem(409, "Device conflict", ex.Message);
        }
        catch (DeviceValidationException ex)
        {
            return CreateProblem(400, "Invalid device update", ex.Message);
        }
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    // Endpoint to partially update a device by its ID
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchDeviceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var device = await _deviceService.PatchAsync(
                id, request, cancellationToken);

            return Ok(device);
        }
        catch (DeviceNotFoundException ex)
        {
            return CreateProblem(404, "Device not found", ex.Message);
        }
        catch (DeviceConflictException ex)
        {
            return CreateProblem(409, "Device conflict", ex.Message);
        }
        catch (DeviceValidationException ex)
        {
            return CreateProblem(400, "Invalid device update", ex.Message);
        }
    }

    // Method to create a ProblemDetails response for error handling
    private static ObjectResult CreateProblem(
        int status,
        string title,
        string detail)
    {
        return new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        })
        {
            StatusCode = status
        };
    }
}
