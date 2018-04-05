using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ShipPosition : IComponentData {
    public float3 Position;
}
