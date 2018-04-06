using System;
using UnityEngine;
using Unity.Entities;

public class SpaceHostilitiesBootStrap : MonoBehaviour {
    
    public static EntityArchetype ShipArchetype;
    public static EntityArchetype ShipSpawnerArchetype;
    public static EntityManager EntityManager;
    
    public BattleSetupSettingsSO BattleSettings;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        CreateArchetypes();
    }
    
    private static void CreateArchetypes()
    {
        EntityManager = World.Active.GetOrCreateManager<EntityManager>();
        
        ShipArchetype = EntityManager.CreateArchetype(
            typeof(ShipPosition),
            typeof(ShipHeading),
            typeof(ShipScale),
            typeof(ShipColor),
            typeof(ShipRenderMatrix),
            typeof(ShipVisuals),
            typeof(ShipFaction));
        
        ShipSpawnerArchetype = EntityManager.CreateArchetype(
            typeof(SpawnShipsData)
            );
    }
    
    public void Awake()
    {
        SetupBattle();
    }

    private void SetupBattle()
    {
        if(BattleSettings == null)
        {
            throw new InvalidOperationException("No Battle Settings provided");
        }
        
        CreateSpawner();
    }

    private void CreateSpawner()
    {
        for (int i = 0; i < BattleSettings.SpawnData.Count; ++i)
        {
            var newSpawner = EntityManager.CreateEntity(ShipSpawnerArchetype);
            EntityManager.SetSharedComponentData(newSpawner, BattleSettings.SpawnData[i]);
        }
    }
}
