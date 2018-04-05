using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct ShipFaction : ISharedComponentData {
    public string FactionName;
    public Color FactionColor; 
}
