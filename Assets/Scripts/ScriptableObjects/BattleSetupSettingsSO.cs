using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBattleSetupSettings", menuName = "SpaceH/Battle Setup Settings")]
public class BattleSetupSettingsSO : ScriptableObject {
    public List<SpawnShipsData> SpawnData;
}
