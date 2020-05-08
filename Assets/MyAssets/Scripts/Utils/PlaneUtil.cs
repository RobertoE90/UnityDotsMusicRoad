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


public struct LineSegment3D : IComponentData
{
    public float3 origin;
    public float3 end;
}

public static class PlaneCollisionUtility
{
    /// <summary>
    /// Use when create a plane for collision detection
    /// </summary>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="pointC"></param>
    /// <returns></returns>
    public static PlaneStruct ComputePlaneParameters(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var collisionPlane = new PlaneStruct();

        collisionPlane.PointA = pointA;
        collisionPlane.PointB = pointB;
        collisionPlane.PointC = pointC;

        collisionPlane.DeltaB = collisionPlane.PointA - collisionPlane.PointB;
        collisionPlane.DeltaC = collisionPlane.PointA - collisionPlane.PointC;

        collisionPlane.Normal = ComputePlaneNormal(collisionPlane);

        collisionPlane._displacement = ComputePlaneDisplacement(collisionPlane);

        return collisionPlane;
    }

    /// <summary>
    /// Computes the plane normal vector
    /// </summary>
    /// <param name="plane"></param>
    /// <returns></returns>
    private static float3 ComputePlaneNormal(PlaneStruct plane)
    {

        var normal = new float3(
            (plane.DeltaB.y * plane.DeltaC.z) - (plane.DeltaB.z * plane.DeltaC.y),
            ((plane.DeltaB.x * plane.DeltaC.z) - (plane.DeltaB.z * plane.DeltaC.x)) * -1,
            (plane.DeltaB.x * plane.DeltaC.y) - (plane.DeltaB.y * plane.DeltaC.x)
            );

        return math.normalize(normal);
    }

    /// <summary>
    /// Computes the D Component of the plane formula
    /// </summary>
    /// <param name="plane"></param>
    /// <returns></returns>
    private static float ComputePlaneDisplacement(PlaneStruct plane)
    {
        return  plane.Normal.x * plane.PointA.x + plane.Normal.y * plane.PointA.y + plane.Normal.z * plane.PointA.z;
    }

    /// <summary>
    /// returns true if a line collides with the plane
    /// points are float4 homogeneus coordenates
    /// </summary>
    /// <param name="linePointA"></param>
    /// <param name="linePointB"></param>
    /// <param name="hitPoint">stores the hit point if exists</param>
    /// <returns></returns>
    public static bool ComputLineHit(PlaneStruct plane, float3 linePointA, float3 linePointB, out float3 hitPoint, out float3 interceptionPoint)
    {
        hitPoint = float3.zero;
        interceptionPoint = float3.zero;

        float3 delta = linePointB - linePointA;

        var nominator = plane._displacement - (plane.Normal.x * linePointA.x) - (plane.Normal.y * linePointA.y) - (plane.Normal.z * linePointA.z);

        float denominator = (plane.Normal.x * delta.x) + (plane.Normal.y * delta.y) + (plane.Normal.z * delta.z);

        if (denominator == 0f)
            return false;

        float t = nominator / denominator;

        var deltaScaled = delta * t;

        if (t > 1f || t < 0)
            return false;

        hitPoint = new float3(
            linePointA.x + deltaScaled.x,
            linePointA.y + deltaScaled.y,
            linePointA.z + deltaScaled.z);

        var iPoint = float3.zero;
        IsInPlaneLimits(plane, hitPoint, out iPoint);

        interceptionPoint = iPoint;

        return true;
    }

    private static bool IsInPlaneLimits(PlaneStruct plane, float3 hitPoint, out float3 interceptionPoint)
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(plane.PointA, Vector3.one * 0.05f);
        Gizmos.DrawWireCube(plane.PointB, Vector3.one * 0.05f);
        Gizmos.DrawWireCube(plane.PointC, Vector3.one * 0.05f);

        Debug.DrawLine(plane.PointA, plane.PointB, Color.gray);
        Debug.DrawLine(plane.PointA, plane.PointC, Color.gray);

        Debug.DrawLine(plane.PointA, hitPoint, Color.magenta);
        Debug.DrawLine(plane.PointB, plane.PointC, Color.yellow);

        //Compute J component of the collision

        var d = plane.PointA - hitPoint;
        var g = plane.PointB - plane.PointC;

        var j = -1f;
        var divisor = 0f;

        var converge = false;

        if(d.x != 0f && d.y != 0f)
        {
            divisor = (g.x / d.x) - (g.y / d.y);
            if(divisor != 0f)
            {
                var nom = (plane.PointB.y / d.y) - (plane.PointA.y / d.y) - (plane.PointB.x / d.x) + (plane.PointA.x / d.x);
                if (nom != 0f)
                {
                    converge = true;
                    j = nom / divisor;
                }
            }
        }

        if (!converge)
        {
            if (d.z != 0f && d.x != 0f)
            {
                divisor = (g.x / d.x) - (g.z / d.z);
                if (divisor != 0f)
                {
                    var nom = (plane.PointB.z / d.z) - (plane.PointA.z / d.z) - (plane.PointB.x / d.x) + (plane.PointA.x / d.x);
                    if (nom != 0f)
                    {
                        converge = true;
                        j = nom / divisor;
                    }
                }
            }
        }
        
        if (!converge)
        {
            if (d.z != 0f && d.y != 0f)
            {
                divisor = (g.y / d.y) - (g.z / d.z);
                if (divisor != 0f)
                {
                    var nom = (plane.PointB.z / d.z) - (plane.PointA.z / d.z) - (plane.PointB.y / d.y) + (plane.PointA.y / d.y);
                    if (nom != 0f)
                    {
                        converge = true;
                        j = nom / divisor;
                    }
                }
            }
        }

        interceptionPoint = plane.PointB + g * j;

        if (!converge)
            return false;

        //Compute i component of the collision
        var i = -1f;
        if(d.x != 0f)
        {
            i = (plane.PointB.x + g.x * j - plane.PointA.x) / d.x;
        }
        else if (d.y != 0)
        {
            i = (plane.PointB.y + g.y * j - plane.PointA.y) / d.y;
        }
        else if(d.z != 0)
        {
            i = (plane.PointB.z + g.z * j - plane.PointA.z) / d.z;
        }

        return false;
        //return (i > 0 && i < d);
    }

    /// <summary>
    /// return a positive number if the point is in the same side as the normal from the plane
    /// return a negative number if the point is in the contrary side of the normal from the plane
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private static float GetPointSide(PlaneStruct plane, float3 point)
    {
        var deltaPoint = point - plane.PointA;
        return deltaPoint.x * (plane.DeltaB.y * plane.DeltaC.z - plane.DeltaB.z * plane.DeltaC.y) -
            deltaPoint.y * (plane.DeltaB.x * plane.DeltaC.z - plane.DeltaB.z * plane.DeltaC.x) +
            deltaPoint.z * (plane.DeltaB.x * plane.DeltaC.y - plane.DeltaB.y * plane.DeltaC.x);
    }

    /// <summary>
    /// Returns true if the point is on the same side of the plane as the normal
    /// Uses a threshold for displacing the plane by the normal
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="point"></param>
    /// <param name="collisionThreshold"></param>
    /// <returns></returns>
    public static bool IsPointInNormalSideOfPlane(PlaneStruct plane, float3 point, float collisionThreshold = 0.001f)
    {
        var sideDeterminant = GetPointSide(plane, point);
        return sideDeterminant > collisionThreshold;
    }
} 
