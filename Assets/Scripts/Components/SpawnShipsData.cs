using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct SpawnShipsData : ISharedComponentData {
	public ShipFactionDefintionSO Faction;
	public ShipDefinitionDataSO ShipDefinition;
	public int SpawnCount;
}
