using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using UnityEngine;

public class UpdatePositionSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var cameraZPosition = 0f;
        var squareDepth = 0f;
        Entities
            .ForEach((in CameraCarouselComponent cameraCarousel) => {
                cameraZPosition = cameraCarousel.CameraDepth;
                squareDepth = cameraCarousel.CarouselLenght;
            }).Run();

        var jobHandler = Entities
            .WithAll<VoxelTagComponent>()
            .ForEach((ref Translation translation) =>
            {
                if(translation.Value.z < cameraZPosition)
                {
                    translation.Value.z += squareDepth; 
                }

            }).Schedule(inputDeps);

        return jobHandler;
    }
}
