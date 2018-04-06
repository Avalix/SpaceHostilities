using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[UpdateAfter(typeof(ShipProcessRenderingBarrier))]
public class UpdateShipTransformsSystem : JobComponentSystem {
    
    struct ShipTransformGroup
    {
        [ReadOnly] public ComponentDataArray<ShipPosition> Positions;
        [ReadOnly] public ComponentDataArray<ShipHeading> Headings;
        [ReadOnly] public ComponentDataArray<ShipScale> ShipScale;
        public ComponentDataArray<ShipRenderMatrix> Transforms;
        public int Length;
    }
    [Inject] private ShipTransformGroup _shipTransformGroup;
    
    [ComputeJobOptimization]
    struct UpdateRenderMatrixFromPosHeadingScale : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<ShipPosition> Positions;
        [ReadOnly] public ComponentDataArray<ShipHeading> Headings;
        [ReadOnly] public ComponentDataArray<ShipScale> ShipScale;
        public ComponentDataArray<ShipRenderMatrix> Transforms;

        public void Execute(int index)
        {
            var TRMatrix = math.lookRotationToMatrix(Positions[index].Position, Headings[index].Heading, Headings[index].LocalUp);
            var SMatrix = math.scale(new float3(ShipScale[index].Scale));
            var finalMatrix = math.mul(TRMatrix, SMatrix);
            
            Transforms[index] = new ShipRenderMatrix { RenderMatrix = finalMatrix};
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if(_shipTransformGroup.Length == 0)
        {
            return inputDeps;
        }
        
        var updateHeadingTransTransformRootsJob = new UpdateRenderMatrixFromPosHeadingScale
        {
            Positions = _shipTransformGroup.Positions,
            Headings = _shipTransformGroup.Headings,
            ShipScale = _shipTransformGroup.ShipScale,
            Transforms = _shipTransformGroup.Transforms,
        };
        
        return updateHeadingTransTransformRootsJob.Schedule(_shipTransformGroup.Length, 64, inputDeps);
    }
}
