using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct ShipDefinition : ISharedComponentData {
    public Mesh ShipMesh;
    public Material ShipMaterial;
    public float ShipScale;
}
