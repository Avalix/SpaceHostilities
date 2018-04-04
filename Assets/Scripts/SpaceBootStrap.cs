using UnityEngine;
using Unity.Entities;

public static class SpaceBootStrap {
    
    public static EntityArchetype ShipArchetype;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        CreateArchetypes();
    }
    
    private static void CreateArchetypes()
    {
        
    }
}
