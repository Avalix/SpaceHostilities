using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShipFaction", menuName = "SpaceH/Ship Faction")]
public class ShipFactionDefintionSO : ScriptableObject {
    public string FactionName;
    public List<Color> FactionColors;
    
    public int FactionId
    {
        get
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (FactionName != null ? FactionName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FactionColors != null ? FactionColors.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
