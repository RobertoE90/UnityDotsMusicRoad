using Unity.Entities;
using Unity.Mathematics;

public struct ConfigComponentData : IComponentData{
    public int3 SpawnCubeCount;
    public float ZPosition;
    public float MaxScale;
}

public struct AudioSamplerComponentData : IComponentData
{
    public float Value;
}