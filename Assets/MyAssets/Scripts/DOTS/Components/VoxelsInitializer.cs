using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public class VoxelsInitializerComponent : IComponentData
{
    public Entity VoxelsEntity;
    public float3 VolumeBoundingBoxSize;
}
