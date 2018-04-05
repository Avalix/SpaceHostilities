using UnityEngine;

[CreateAssetMenu(fileName = "NewShipFaction", menuName = "SpaceH/Ship Faction")]
public class ShipFactionDataSO : ScriptableObjectSharedComponentData<ShipFaction> {
    public int FactionId
    {
        get
        {
            unchecked
            {
                return ((Data.FactionName != null ? Data.FactionName.GetHashCode() : 0) * 397) ^ Data.FactionColor.GetHashCode();
            }
        }
    }
}
