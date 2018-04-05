using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ShipHeading : IComponentData {
	public float3 Heading;
	public float3 LocalUp;
}