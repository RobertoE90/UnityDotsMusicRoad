using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;

public class ModifierSystem : JobComponentSystem
{
    protected override void OnStartRunning()
    {
        base.OnStartRunning();
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float maxScale = 0f;

        Entities
            .WithoutBurst()
            .ForEach((in VoxelsInitializerComponent voxelInitializer) => {
            maxScale = voxelInitializer.VoxelScale;
        }).Run();


        var jobHandler = Entities
            .WithAll<VoxelTagComponent>()
            .ForEach((ref Scale scale, in NoiseValue noise) =>
            {
                //scale.Value = noise.Value * maxScale;
                scale.Value = maxScale;
            }).Schedule(inputDeps);

        return jobHandler;
    }
}
