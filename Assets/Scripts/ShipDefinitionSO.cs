using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipDefinition", menuName = "SpaceH/Ship Defintion")]
public class ShipDefinitionSO : ScriptableObject {
    public Mesh ShipMesh;
    public Color ShipColor = Color.white;
    public float ShipScale = 1.0f;
}
