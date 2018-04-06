using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Video;

//TODO: Rewrite this with jobs?
public class SpawnShipsSystem : ComponentSystem {
    
    struct SpawnZones {
        public float3 Position;
        public float SpawnRadius;
    }
    
    struct SpawnEntityData {
        public Entity Entity;
        public int SharedDataIndex;
    }
    
    private ComponentGroup _spawnGroup;
    private Dictionary<int, NativeArray<SpawnZones>> _factionSpawnZones;
    [Inject] private EndFrameBarrier _endFrameBarrier;
    
    protected override void OnCreateManager(int capacity)
    {
        _spawnGroup = GetComponentGroup(typeof(SpawnShipsData));
    }
    
    protected override void OnUpdate()
    {
        if(_factionSpawnZones == null)
        {
            GatherSpawnZones();
        }
        
        var uniqueSpawnShipSharedData = new List<SpawnShipsData>();
        var spawnShipEntityData = new List<SpawnEntityData>();
        
        EntityManager.GetAllUniqueSharedComponentDatas(uniqueSpawnShipSharedData);
        
        for (int sharedIndex = 0; sharedIndex != uniqueSpawnShipSharedData.Count; sharedIndex++)
        {
            var spawner = uniqueSpawnShipSharedData[sharedIndex];
            _spawnGroup.SetFilter(spawner);
            var entities = _spawnGroup.GetEntityArray();
            
            for (int entityIndex = 0; entityIndex < entities.Length; entityIndex++)
            {
                spawnShipEntityData.Add(new SpawnEntityData()
                {
                    Entity = entities[entityIndex],
                    SharedDataIndex = sharedIndex,
                });
            }
        }

        if (spawnShipEntityData.Count == 0)
        {
            return;
        }
           
        for (int spawnShipEntityIndex = 0; spawnShipEntityIndex < spawnShipEntityData.Count; spawnShipEntityIndex++)
        {
            var spawnEntity = spawnShipEntityData[spawnShipEntityIndex].Entity;
            var sharedDataIndex = spawnShipEntityData[spawnShipEntityIndex].SharedDataIndex;
            var sharedData = uniqueSpawnShipSharedData[sharedDataIndex];
            
            NativeArray<SpawnZones> factionSpawnZones;
            
            if(_factionSpawnZones.TryGetValue(sharedData.Faction.FactionId, out factionSpawnZones) == false)
            {
                Debug.LogWarning("No Spawn Zone for Faction '" + sharedData.Faction.FactionName + "', skipping...");
                continue;
            }
            
            var newShips = new NativeArray<Entity>(sharedData.SpawnCount, Allocator.Temp);
            var spawnPositions = new NativeArray<float3>(sharedData.SpawnCount, Allocator.Temp);
            
            EntityManager.CreateEntity(SpaceHostilitiesBootStrap.ShipArchetype, newShips);
            GenerateRandomPointsInSpawnZones(ref factionSpawnZones, ref spawnPositions);
            
            for(int i = 0; i < newShips.Length; ++i)
            {
                SetNewShipData(newShips[i], spawnPositions[i], sharedData);
            }
            
            spawnPositions.Dispose();
            newShips.Dispose();
            EntityManager.DestroyEntity(spawnEntity);
        }
    }

    private void SetNewShipData(Entity newShip, float3 spawnPosition, SpawnShipsData sharedData)
    {
        //Compoennt Data
        EntityManager.SetComponentData(newShip, new ShipPosition()
        {
            Position = spawnPosition,
        });

        EntityManager.SetComponentData(newShip, new ShipHeading()
        {
            Heading = new float3(0.0f, 0.0f, 1.0f),
            LocalUp = new float3(0.0f, 1.0f, 0.0f)
        });
        
        var shipColor = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            //sharedData.Faction.FactionColors[Random.Range(0, sharedData.Faction.FactionColors.Count)];

        EntityManager.SetComponentData(newShip, new ShipColor()
        {
            Color = new float4(
                shipColor.r,
                shipColor.g,
                shipColor.b,
                shipColor.a),
        });

        EntityManager.SetComponentData(newShip, new ShipScale()
        {
            Scale = sharedData.ShipDefinition.Scale,
        });

        //Shared Data
        EntityManager.SetSharedComponentData(newShip, new ShipFaction()
        {
            FactionId = sharedData.Faction.FactionId,
        });

        EntityManager.SetSharedComponentData(newShip, new ShipVisuals()
        {
            ShipMesh = sharedData.ShipDefinition.ShipMesh,
            ShipMaterial = sharedData.ShipDefinition.ShipMaterial,
        });

        EntityManager.SetSharedComponentData(newShip, new ShipFaction()
        {
            FactionId = sharedData.Faction.FactionId,
        });
    }

    protected override void OnDestroyManager()
    {
        foreach(var spawnZone in _factionSpawnZones)
        {
            spawnZone.Value.Dispose();
        }
        
        _factionSpawnZones.Clear();
    }
    
    private void GatherSpawnZones()
    {
        _factionSpawnZones = new Dictionary<int, NativeArray<SpawnZones>>();
        
        var allSpawnPointInScene = Object.FindObjectsOfType<FactionSpawnPoint>();
        
        //Gather Spawn poits by faction
        
        Dictionary<int, List<SpawnZones>> spawnPointsByFaction = new Dictionary<int, List<SpawnZones>>();
        
        foreach(var factionSpawnPoint in allSpawnPointInScene)
        {
            List<SpawnZones> spawnZones;
            var factionId = factionSpawnPoint.Faction.FactionId;
            
            if(spawnPointsByFaction.TryGetValue(factionId, out spawnZones) == false)
            {
                spawnZones = new List<SpawnZones>();
                spawnPointsByFaction.Add(factionId, spawnZones);
            }
            
            spawnZones.Add(new SpawnZones()
            {
                Position = factionSpawnPoint.transform.position,
                SpawnRadius = factionSpawnPoint.SpawnRadius,
            });
        }
        
        //Now Convert To Native Data
        foreach(var spawnPoints in spawnPointsByFaction)
        {
            NativeArray<SpawnZones> spawnZoneArray = new NativeArray<SpawnZones>(spawnPoints.Value.Count, Allocator.Persistent);

            for (var spawnPointIndex = 0; spawnPointIndex < spawnPoints.Value.Count; spawnPointIndex++)
            {
                spawnZoneArray[spawnPointIndex] = spawnPoints.Value[spawnPointIndex];
            }

            _factionSpawnZones.Add(spawnPoints.Key, spawnZoneArray);
            
        }

    }
    
    private void GenerateRandomPointsInSpawnZones(ref NativeArray<SpawnZones> factionSpawnZones, ref NativeArray<float3> points)
    {
        var pointsFound = 0;
        var count = points.Length;
        while (pointsFound < count)
        {
            var spawnZones = factionSpawnZones[Random.Range(0, factionSpawnZones.Length)];
            
            var p = new float3
            {
                x = Random.Range(-spawnZones.SpawnRadius, spawnZones.SpawnRadius),
                y = Random.Range(-spawnZones.SpawnRadius, spawnZones.SpawnRadius),
                z = Random.Range(-spawnZones.SpawnRadius, spawnZones.SpawnRadius)
            };
            if (math.lengthSquared(p) < (spawnZones.SpawnRadius * spawnZones.SpawnRadius))
            {
                points[pointsFound] = spawnZones.Position + p;
                pointsFound++;
            }
        }
    }
}
