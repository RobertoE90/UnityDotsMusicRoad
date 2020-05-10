using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


[InternalBufferCapacity(8)]
public struct CollisionMeshBufferData : IBufferElementData
{
    public PlaneStruct Plane;
}

public struct PlaneStruct: IComponentData
{
    public float3 PointA;
    public float3 PointB;
    public float3 PointC;

    public float3 DeltaB;
    public float3 DeltaC;

    public float3 Normal;

    public float _displacement;
}

public struct VoxelTagComponent : IComponentData { }

public struct NoiseValue : IComponentData
{
    public float Value;
}
public struct CollisionDetected : IComponentData
{
    public byte Hit;
}

