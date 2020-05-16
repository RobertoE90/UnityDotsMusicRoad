using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PerlinNoiseValueCompute : JobComponentSystem
{
    private float _time = 0f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        _time += Time.DeltaTime * 0.02f;
        var valueTime = _time;
        
        Entities
            .ForEach((in AudioSamplerComponentData audioSource) => {
                valueTime += (audioSource.Value * 0.04f);
            }).Run();

        _time = valueTime;

        var jobHandler = Entities
            .WithAll<VoxelTagComponent>()
            .ForEach((ref NoiseValue noiseOut, in Translation translation) =>
            {
                var noisePass = ComputeNoise(translation.Value, 0.3f, valueTime, 0.75f);

                noiseOut.Value = math.clamp(noisePass, 0f, 1f);

                float ComputeNoise(float3 position, float scale, float time, float threshold)
                {
                    //normalized distance from the center
                    var noiseValue = noise.cnoise(new float4(
                                position.x * scale,
                                position.y * scale,
                                position.z * scale,
                                time));

                    //ranges noise from 0 to 1
                    noiseValue = noiseValue * 0.5f + 0.5f;
                    
                    var normalizedBorderDistance = (threshold - noiseValue) / threshold;

                    if(normalizedBorderDistance > 0)
                        return math.clamp(normalizedBorderDistance /= 0.25f, 0f, 1f) * 1.3f;
                    else
                        return 0f;
                }

            }).Schedule(inputDeps);

        return jobHandler;
    }
}
