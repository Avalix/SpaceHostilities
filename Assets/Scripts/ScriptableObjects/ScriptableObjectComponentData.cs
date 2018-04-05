using Unity.Entities;
using UnityEngine;

public abstract class ScriptableObjectComponentData<T> : ScriptableObject where T : struct, IComponentData {
    public T Data;
    
    public ComponentType GetComponentType()
    {
        return ComponentType.Create<T>();
    }
    
    public void UpdateComponentData(EntityManager manager, Entity entity)
    {
        manager.SetComponentData(entity, Data);
    }
}

public abstract class ScriptableObjectSharedComponentData<T> : ScriptableObject where T : struct, ISharedComponentData {
    public T Data;
    
    public ComponentType GetComponentType()
    {
        return ComponentType.Create<T>();
    }
    
    public void UpdateComponentData(EntityManager manager, Entity entity)
    {
        manager.SetSharedComponentData(entity, Data);
    }
}
