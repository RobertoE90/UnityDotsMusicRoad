using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class VoxelSpawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, in VoxelsInitializerComponent vi) => { });
        /*
        Entities
            .ForEach((in VoxelsInitializerComponent voxelsInitializer) =>
            {
                var bounds = voxelsInitializer.VolumeBoundingBoxSize;
                for(var z = 0; z < bounds.z; z++)
                {
                    for(var y = 0; y < bounds.y; y++)
                    {
                        for(var x = 0; x < bounds.x; x++)
                        {
                            var voxel = EntityManager.Instantiate(voxelsInitializer.VoxelsEntity);
                            EntityManager.SetComponentData<Translation>(voxel, new Translation
                            {
                                Value = new float3(x, y, z)
                            });
                        }
                    }
                }
            });/*
    }
    
}
