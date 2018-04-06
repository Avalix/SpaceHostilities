using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

[UpdateAfter(typeof(UpdateShipTransformsSystem))]
public class RenderShipIndirectInstancingSystem : JobComponentSystem {
    
    private ComponentGroup _renderGroup;
    
    struct InstanceData {
        public float4x4 InstanceTransform;
        public float4 Color;
    }
    
    private Dictionary<ShipVisuals, ComputeBuffer> _instanceComputeBuffers = new Dictionary<ShipVisuals, ComputeBuffer>();
    private Dictionary<ShipVisuals, ComputeBuffer> _indirectArgsComputeBuffers = new Dictionary<ShipVisuals, ComputeBuffer>();
    private NativeArray<uint> _indirectArgs;
    private MaterialPropertyBlock _propBlock = new MaterialPropertyBlock();

    [ComputeJobOptimization]
    struct CopyInstanceDataToNativeArrayJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<ShipRenderMatrix> RenderMatricies;
        [ReadOnly] public ComponentDataArray<ShipColor> Colors;
        public NativeArray<InstanceData> TargetNativeArray;

        public void Execute(int index)
        {            
            TargetNativeArray[index] = new InstanceData()
            {
                InstanceTransform = RenderMatricies[index].RenderMatrix,
                Color = Colors[index].Color,
            };
        }
    }

    
    //Currently doesnt work
    /*[ComputeJobOptimization]
    struct FillInstanceComputeBuffersJob : IJobParallelForBatch
    {
        [ReadOnly] public NativeArray<InstanceData> InstanceData;
        public ComputeBuffer TargetComputeBuffer;

        public void Execute(int startIndex, int count)
        {
            TargetComputeBuffer.SetData(InstanceData, startIndex, startIndex, count);
        }
    }*/
    
    protected override void OnCreateManager(int capacity)
    {
        _renderGroup = GetComponentGroup(
            typeof(ShipVisuals), 
            typeof(ShipRenderMatrix), 
            typeof(ShipColor), 
            typeof(ShipFaction));
        
        _indirectArgs = new NativeArray<uint>(5, Allocator.Persistent);
        
        for(int i = 0; i < _indirectArgs.Length; ++i)
        {
            _indirectArgs[i] = 0;
        }
    }
    
    protected override void OnDestroyManager()
    {        
        _indirectArgs.Dispose();
        
        foreach(var computeBuffer in _instanceComputeBuffers.Values)
        {
            computeBuffer.Release();
        }
        
        foreach(var computeBuffer in _indirectArgsComputeBuffers.Values)
        {
            computeBuffer.Release();
        }
        
        _instanceComputeBuffers.Clear();
        _indirectArgsComputeBuffers.Clear(); 
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var uniqueShipVisualsData = new List<ShipVisuals>();
        EntityManager.GetAllUniqueSharedComponentDatas(uniqueShipVisualsData);
        
        if(uniqueShipVisualsData.Count == 0)
        {
            return inputDeps;
        }
        
        var instanceDataArrays = new List<NativeArray<InstanceData>>(uniqueShipVisualsData.Count);
        var runningRenderJobs = new NativeArray<JobHandle>(uniqueShipVisualsData.Count, Allocator.Temp);
        
        for (int renderBatchIndex = 0; renderBatchIndex != uniqueShipVisualsData.Count; renderBatchIndex++)
        {
            var shipVisuals = uniqueShipVisualsData[renderBatchIndex];
            _renderGroup.SetFilter(shipVisuals);
            
            var renderMatrices = _renderGroup.GetComponentDataArray<ShipRenderMatrix>();
            var colors = _renderGroup.GetComponentDataArray<ShipColor>();

            var newInstanceArray = new NativeArray<InstanceData>(renderMatrices.Length, Allocator.Temp);
            instanceDataArrays.Add(newInstanceArray);
            
            if(renderMatrices.Length == 0)
            {
                continue;
            }
            
            var copyToNativeArrayJob = new CopyInstanceDataToNativeArrayJob()
            {
                RenderMatricies = renderMatrices,
                Colors = colors,
                TargetNativeArray = newInstanceArray
            };
            
            var copyToArrayJob = copyToNativeArrayJob.Schedule(renderMatrices.Length, 64, inputDeps);
            
            //MANUAL COMPUTEBUFFER FILL
            runningRenderJobs[renderBatchIndex] = copyToArrayJob;
            
            /*var instanceDataComputeBuffer = GetInstanceDataComputeBuffertoUse(shipVisuals, renderMatrices.Length);

            var fillComputerBufferJob = new FillInstanceComputeBuffersJob()
            {
                InstanceData = newInstanceArray,
                TargetComputeBuffer = instanceDataComputeBuffer,
            };
            
            runningRenderJobs[renderBatchIndex] = fillComputerBufferJob.ScheduleBatch(newInstanceArray.Length, 64, copyToArrayJob);*/
            
            var indirectArgsBuffer = GetIndirectArgsBufferToUse(shipVisuals);
            
            _indirectArgs[0] = (uint)shipVisuals.ShipMesh.triangles.Length;
            _indirectArgs[1] = (uint)newInstanceArray.Length;
            
            indirectArgsBuffer.SetData(_indirectArgs);

        }
        
        var mainCamera = Camera.main;

        //make sure the compute buffers are filled
        JobHandle.CompleteAll(runningRenderJobs);

        //now fireoff the renders
        for (int renderBatchIndex = 0; renderBatchIndex != uniqueShipVisualsData.Count; renderBatchIndex++)
        {
            if(instanceDataArrays[renderBatchIndex].Length == 0)
            {
                continue;
            }
            
            var shipVisuals = uniqueShipVisualsData[renderBatchIndex];
            //var instanceComputeBuffer =  _instanceComputeBuffers[shipVisuals];
            var indirectArgsComputeBuffer =  _indirectArgsComputeBuffers[shipVisuals];
            
            //MANUAL COMPUTEBUFFER FILL
            var instanceComputeBuffer = GetInstanceDataComputeBuffertoUse(shipVisuals, instanceDataArrays[renderBatchIndex].Length);
            instanceComputeBuffer.SetData(instanceDataArrays[renderBatchIndex]);
            
            _propBlock.SetBuffer("_InstanceData", instanceComputeBuffer);
            
            Graphics.DrawMeshInstancedIndirect(
                shipVisuals.ShipMesh,
                0,
                shipVisuals.ShipMaterial,
                new Bounds(Vector3.zero, new Vector3(10000.0f, 10000.0f, 10000.0f)),
                indirectArgsComputeBuffer,
                0,
                _propBlock,
                ShadowCastingMode.On,
                true,
                0,
                mainCamera);
        }
        
        //Cleanup
        for(int i = 0; i < instanceDataArrays.Count; ++i)
        {
            instanceDataArrays[i].Dispose();
        }
        
        runningRenderJobs.Dispose();
        
        return default(JobHandle);
    }

    private ComputeBuffer GetInstanceDataComputeBuffertoUse(ShipVisuals key, int sizeNeeded)
    {
        ComputeBuffer instanceDataComputeBuffer = null;
        
        _instanceComputeBuffers.TryGetValue(key, out instanceDataComputeBuffer);

        if (instanceDataComputeBuffer == null || instanceDataComputeBuffer.count < sizeNeeded)
        {
            if (instanceDataComputeBuffer != null)
            {
                instanceDataComputeBuffer.Release();
            }
            
            unsafe
            {
                instanceDataComputeBuffer = new ComputeBuffer(sizeNeeded,
                    sizeof(InstanceData),
                    ComputeBufferType.Default);
            }
            
            _instanceComputeBuffers[key] = instanceDataComputeBuffer;
            
        }

        return instanceDataComputeBuffer;
    }

    private ComputeBuffer GetIndirectArgsBufferToUse(ShipVisuals key)
    {
        ComputeBuffer indirectArgsBuffer = null;
        
        _indirectArgsComputeBuffers.TryGetValue(key, out indirectArgsBuffer);

        if (indirectArgsBuffer == null)
        {
            indirectArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint),
                ComputeBufferType.IndirectArguments);
            
            _indirectArgsComputeBuffers[key] = indirectArgsBuffer;
        }

        return indirectArgsBuffer;
    }
}
