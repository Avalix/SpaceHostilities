using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct ShipVisuals : ISharedComponentData {
    public Mesh ShipMesh;
    public Material ShipMaterial;
}
