using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipDefinition", menuName = "SpaceH/Ship Defintion")]
public class ShipDefinitionDataSO : ScriptableObject {
    public Mesh ShipMesh;
    public Material ShipMaterial;
    public float Scale;
}
