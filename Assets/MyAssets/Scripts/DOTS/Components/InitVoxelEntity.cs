using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;


public class InitVoxelEntity : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] private GameObject _voxelPrefab;
    [SerializeField] private GameObject _volumeGameObject;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var voxelArchetype = dstManager.CreateArchetype(
            typeof(LocalToWorld),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(WorldRenderBounds),
            typeof(NoiseValue),
            typeof(VoxelTagComponent));
        
        var voxelEntity = dstManager.CreateEntity(voxelArchetype);

        dstManager.SetSharedComponentData<RenderMesh>(
            voxelEntity,
            new RenderMesh
            {
                mesh = _voxelPrefab.GetComponent<MeshFilter>().sharedMesh,
                material = _voxelPrefab.GetComponent<Renderer>().sharedMaterial
            });

        //dstManager.SetComponentData<Scale>(voxelEntity, new Scale { Value = 1f });


        var volumeMesh = _volumeGameObject.GetComponent<MeshFilter>().sharedMesh;

        dstManager.AddSharedComponentData<VoxelsInitializerComponent>(entity, new VoxelsInitializerComponent
        {
            VolumeBounds = volumeMesh.bounds,
            VoxelEntityBase = voxelEntity,
            VolumeMesh = volumeMesh
        });
    }
}


public struct VoxelsInitializerComponent : ISharedComponentData, IEquatable<VoxelsInitializerComponent>
{
    public Bounds VolumeBounds;
    public Entity VoxelEntityBase;
    public Mesh VolumeMesh;

    public bool Equals(VoxelsInitializerComponent other)
    {
        return VoxelEntityBase == other.VoxelEntityBase;
    }

    public override int GetHashCode()
    {
        return VoxelEntityBase.GetHashCode();
    }
}
