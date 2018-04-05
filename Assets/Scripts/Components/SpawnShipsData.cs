using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct SpawnShipsData : ISharedComponentData {
	public ShipFactionDataSO Faction;
	public ShipDefinitionDataSO ShipDefinition;
	public int SpawnCount;
}
