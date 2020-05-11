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
    private int _raycastBatchSize;
    private int _raycastCurrentCount;
    private int _raycastTotalOperations;

    protected override void OnCreate()
    {
        _finish = false;
        _raycastCurrentCount = 0;
        _raycastBatchSize = 10;

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

        _raycastTotalOperations = _boundSideVoxelCount.x * _boundSideVoxelCount.y * 2;

        Debug.Log("Total of iterations " + _raycastTotalOperations);
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
        var MaxElementsScale = _voxelScale;
        var VolumePlanes = _volumePlanesArray;

        Entities
            .WithoutBurst()
            .ForEach((in VoxelsInitializerComponent voxelsInitializer) =>
            {
                

                var parallelInitEntityJob = new ParallelInitializationJob
                {
                    BaseEntity = voxelsInitializer.VoxelEntityBase,
                    BoundBoxCenter = voxelsInitializer.VolumeBounds.center,
                    BoundBoxCount = _boundSideVoxelCount,
                    BatchBeginIndex = _raycastCurrentCount,
                    MaxElementsScale = _voxelScale,
                    CommandBuffer = commandBuffer,
                    VolumePlanes = _volumePlanesArray
                };

                if (_raycastCurrentCount + _raycastBatchSize > _raycastTotalOperations)
                    _raycastBatchSize = _raycastTotalOperations - _raycastCurrentCount;
                
                var jobHandler = parallelInitEntityJob.Schedule(_raycastBatchSize, (int)(math.ceil(_raycastBatchSize * 0.25f)));
                jobHandler.Complete();

            }).Run();

        _raycastCurrentCount += _raycastBatchSize;
        _barrier.AddJobHandleForProducer(Dependency);
        _finish = _raycastCurrentCount > _raycastTotalOperations;
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
            var currentIndex = index + BatchBeginIndex;
            int x = currentIndex % BoundBoxCount.x;
            int y = currentIndex / BoundBoxCount.x;
            
            float3 pointA = float3.zero;
            if (y % 2 == 0)
                pointA = new float3(x + 0.5f, y * 0.25f, BoundBoxCount.z * 0.5f) * MaxElementsScale;
            else
                pointA = new float3(x, y * 0.25f, BoundBoxCount.z * 0.5f) * MaxElementsScale;

            pointA -= new float3(BoundBoxCount.x, BoundBoxCount.y * 0.5f, (BoundBoxCount.z - 2) * 0.5f) * 0.5f * MaxElementsScale;

            float3 pointB = pointA - new float3(0, 0, BoundBoxCount.z + 2) * 0.5f * MaxElementsScale;

            pointA += BoundBoxCenter;
            pointB += BoundBoxCenter;

            float3 hitPoint;

            var hitPoints = new NativeList<float>(Allocator.Temp);
            for (var i = 0; i < VolumePlanes.Length; i++)
            {
                var hitTest = PlaneCollisionUtility.ComputLineHit(
                    VolumePlanes[i],
                    pointA,
                    pointB,
                    out hitPoint);

                if (hitTest)
                {
                    var merge = false;
                    for (var j = 0; j < hitPoints.Length; j++)
                    {
                        if (math.abs(hitPoint.z - hitPoints[j]) < MaxElementsScale * 0.25f)
                        {
                            merge = true;
                            break;
                        }
                    }

                    if(!merge)
                        hitPoints.Add(hitPoint.z);
                }
            }

            hitPoints.Sort();

            if (hitPoints.Length != 0 && hitPoints.Length % 2 == 0)
            {
                for (var i = 0; i < hitPoints.Length; i += 2)
                {
                    var depth = hitPoints[i] - hitPoints[i + 1];
                    int depthInt = (int)(depth / MaxElementsScale);
                    depth = math.abs(depthInt * MaxElementsScale);

                    int parallelPointZint = (int)(hitPoints[i] / MaxElementsScale);
                    pointA.z = parallelPointZint * MaxElementsScale;

                    var flipSize = false;
                    for (var d = 0f; d <= depth * 2; d += MaxElementsScale)
                    {
                        var position = pointA + new float3(0, 0, d * 0.5f);
                        if (flipSize)
                            position -= new float3(0.5f, 0, 0f) * MaxElementsScale;
                        flipSize = !flipSize;

                        var entity = CommandBuffer.Instantiate(index, BaseEntity);
                        CommandBuffer.SetComponent<Translation>(index, entity, new Translation { Value = position });
                        CommandBuffer.SetComponent<Scale>(index, entity, new Scale { Value = MaxElementsScale });
                    }
                }
            }
            hitPoints.Dispose();
        }
    }
}
