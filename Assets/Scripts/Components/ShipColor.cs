using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct ShipColor : IComponentData {
	public float4 Color; 
}
