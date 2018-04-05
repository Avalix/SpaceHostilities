using Unity.Entities;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipDefinition", menuName = "SpaceH/Ship Defintion")]
public class ShipDefinitionDataSO : ScriptableObjectComponentDataBase {
    public ShipVisuals ShipVisuals;
    public ShipScale ShipScale;
    
    public override void UpdateComponentDataFromSO(EntityManager manager, Entity entity)
    {
        manager.SetSharedComponentData(entity, ShipVisuals);
        manager.SetComponentData(entity, ShipScale);
    }
}
