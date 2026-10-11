using System.Text.Json.Serialization;

namespace Devices.Api.Models;

public enum DeviceState
{
    // Device states are represented as strings in JSON, so we use the JsonStringEnumMemberName attribute to specify the string representation of each enum value
    [JsonStringEnumMemberName("available")]
    Available,

    [JsonStringEnumMemberName("in-use")]
    InUse,

    [JsonStringEnumMemberName("inactive")]
    Inactive
}