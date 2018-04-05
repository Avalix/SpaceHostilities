using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ShipRenderMatrix : IComponentData {
	public float4x4 RenderMatrix;
}
