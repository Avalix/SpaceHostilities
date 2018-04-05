using Unity.Entities;
using UnityEngine;

public abstract class ScriptableObjectComponentDataBase : ScriptableObject {
    public abstract void UpdateComponentDataFromSO(EntityManager manager, Entity entity);
}

public abstract class ScriptableObjectComponentData<T> : ScriptableObjectComponentDataBase 
    where T : struct, IComponentData {
    
    public T Data;

    public override void UpdateComponentDataFromSO(EntityManager manager, Entity entity)
    {
        manager.SetComponentData(entity, Data);
    }
}

public abstract class ScriptableObjectComponentData<T, U> : ScriptableObjectComponentDataBase
    where T : struct, IComponentData 
    where U : struct, IComponentData {
    
    public T DataA;
    public U DataB;

    public override void UpdateComponentDataFromSO(EntityManager manager, Entity entity)
    {
        manager.SetComponentData(entity, DataA);
        manager.SetComponentData(entity, DataB);
    }
}

public abstract class ScriptableObjectSharedComponentData<T> : ScriptableObjectComponentDataBase 
    where T : struct, ISharedComponentData {
    
    public T Data;
    
    public override void UpdateComponentDataFromSO(EntityManager manager, Entity entity)
    {
        manager.SetSharedComponentData(entity, Data);
    }
}

public abstract class ScriptableObjectSharedComponentData<T, U> : ScriptableObjectComponentDataBase
    where T : struct, ISharedComponentData
    where U : struct, ISharedComponentData {
    
    public T DataA;
    public U DataB;
    
    public override void UpdateComponentDataFromSO(EntityManager manager, Entity entity)
    {
        manager.SetSharedComponentData(entity, DataA);
        manager.SetSharedComponentData(entity, DataB);
    }
}
