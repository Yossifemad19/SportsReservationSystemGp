using System.Runtime.Serialization;

namespace backend.Core.Entities;

public enum UserRole
{
    [EnumMember(Value = "Customer")]
    Customer,
    [EnumMember(Value = "Owner")]
    Owner
}