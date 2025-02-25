using System.Runtime.Serialization;

namespace backend.Core.Entities;

public enum BookingStatus
{
    [EnumMember(Value = "Pending")]
    Pending,
    [EnumMember(Value = "Confirmed")]
    Confirmed,
    [EnumMember(Value = "Cancelled")]
    Cancelled,
    [EnumMember(Value = "Completed")]
    Completed
}