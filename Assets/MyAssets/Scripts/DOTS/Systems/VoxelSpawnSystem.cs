using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class VoxelSpawnSystem : SystemBase
{
    EntityCommandBufferSystem _barrier;
    private NativeArray<PlaneStruct> _volumePlanesArray;
    private int3 _boundSideVoxelCount;
    private float _voxelScale;

    private bool _finish;
    private int _voxelBatchSize;
    private int _voxelCount;
    private int _voxelTotal;

    protected override void OnCreate()
    {
        _finish = false;
        _voxelCount = 0;
        _voxelBatchSize = 150;

        _barrier = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning()
    {
        var boundSizeLocal = float3.zero;
        _voxelScale = 0.5f;

        Entities
            .WithoutBurst()
            .ForEach((in VoxelsInitializerComponent voxelsInitializer) =>
            {
                InitializeVolumePlanes(voxelsInitializer.VolumeMesh);
                
                boundSizeLocal = voxelsInitializer.VolumeBounds.size / _voxelScale;

            }).Run();

        _boundSideVoxelCount = new int3((int)boundSizeLocal.x, (int)boundSizeLocal.y * 2, (int)boundSizeLocal.z * 2);

        _voxelTotal = _boundSideVoxelCount.x * _boundSideVoxelCount.y * _boundSideVoxelCount.z;

        Debug.Log("Total of iterations " + _voxelTotal);
    }

    protected override void OnStopRunning()
    {
        _volumePlanesArray.Dispose();
    }

    protected override void OnUpdate()
    {   
        if (_finish)
            return;

        var commandBuffer = _barrier.CreateCommandBuffer().ToConcurrent();
        
        Entities
            .WithoutBurst()
            .ForEach((in VoxelsInitializerComponent voxelsInitializer) =>
            {
                var parallelInitEntityJob = new ParallelInitializationJob
                {
                    BaseEntity = voxelsInitializer.VoxelEntityBase,
                    BoundBoxCenter = voxelsInitializer.VolumeBounds.center,
                    BoundBoxCount = _boundSideVoxelCount,
                    BatchBeginIndex = _voxelCount,
                    MaxElementsScale = _voxelScale, 
                    CommandBuffer = commandBuffer,
                    VolumePlanes = _volumePlanesArray
                };

                if(_voxelCount + _voxelBatchSize > _voxelTotal)
                {
                    _voxelBatchSize = _voxelTotal - _voxelCount;
                }

                var jobHandler = parallelInitEntityJob.Schedule(_voxelBatchSize, (int)(math.ceil(_voxelBatchSize * 0.25f)));
                jobHandler.Complete();
            }).Run();

        _voxelCount += _voxelBatchSize;

        _barrier.AddJobHandleForProducer(Dependency);
        _finish = _voxelCount > _voxelTotal;
    }

    private void InitializeVolumePlanes(Mesh volumeMesh)
    {
        int planeCount = volumeMesh.triangles.Length / 3;

        _volumePlanesArray = new NativeArray<PlaneStruct>(planeCount, Allocator.Persistent);

        for (var i = 0; i < volumeMesh.triangles.Length; i += 3)
        {
            var pointA = volumeMesh.vertices[volumeMesh.triangles[i + 0]];
            var pointB = volumeMesh.vertices[volumeMesh.triangles[i + 1]];
            var pointC = volumeMesh.vertices[volumeMesh.triangles[i + 2]];

            var collisionPlane = PlaneCollisionUtility.ComputePlaneParameters(pointA, pointB, pointC);

            _volumePlanesArray[i / 3] = collisionPlane;
        }
    }

    private struct ParallelInitializationJob : IJobParallelFor
    {
        public Entity BaseEntity;
        public float3 BoundBoxCenter;
        public int3 BoundBoxCount;
        public int BatchBeginIndex;
        public float MaxElementsScale;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        [ReadOnly] public NativeArray<PlaneStruct> VolumePlanes;

        public void Execute(int index)
        {
            var currentBatchIndex = BatchBeginIndex + index;

            int sliceVoxelCount = BoundBoxCount.x * BoundBoxCount.y;

            var z = currentBatchIndex / sliceVoxelCount;
            var sliceRest = currentBatchIndex % sliceVoxelCount;

            int x = sliceRest % BoundBoxCount.x;
            int y = sliceRest / BoundBoxCount.x;

            float3 position = float3.zero;

            if (y % 2 == 0)
                position = new float3(x + 0.5f, y * 0.5f, z * 0.5f) * MaxElementsScale;
            else
                position = new float3(x, y * 0.5f, z * 0.5f) * MaxElementsScale;

            if (z % 2 == 0)
                position += new float3(0.5f, 0, 0f) * MaxElementsScale;

            position -= new float3(BoundBoxCount.x, BoundBoxCount.y * 0.5f, BoundBoxCount.z * 0.5f) * 0.5f * MaxElementsScale;
            position += BoundBoxCenter;
            
            var isPointInsideMesh = IsPointInsideMesh(position);
            //isPointInsideMesh = true;
            if (isPointInsideMesh)
            {
                var entity = CommandBuffer.Instantiate(index, BaseEntity);
                CommandBuffer.SetComponent<Translation>(index, entity, new Translation { Value = position });
                CommandBuffer.SetComponent<Scale>(index, entity, new Scale { Value = MaxElementsScale });
            }
        }


        private bool IsPointInsideMesh(float3 origin)
        {
            //if (VolumePlanes == null)
            //    return false;

            var hitTest = false;

            var hitCounterA = 0;
            var endA = new float3(2.4f, -10f, 3.1f);

            var hitCounterB = 0;
            var endB = new float3(4.6f, -10f, -3.1f);

            float3 hitPoint = float3.zero;

            for (var i = 0; i < VolumePlanes.Length; i++)
            {
                
                hitTest = PlaneCollisionUtility.ComputLineHit(
                    VolumePlanes[i],
                    origin,
                    endA,
                    out hitPoint);

                if (hitTest)
                    hitCounterA++;

                hitTest = PlaneCollisionUtility.ComputLineHit(
                    VolumePlanes[i],
                    origin,
                    endB,
                    out hitPoint);

                if (hitTest)
                    hitCounterB++;
            }

            return hitCounterA % 2 == 1 && hitCounterB % 2 == 1;
        }
    }
}
