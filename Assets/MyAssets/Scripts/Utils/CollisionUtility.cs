using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CollisionUtility
{
    private PlaneStruct[] _planes;

    public PlaneStruct[] Planes => _planes;

    public void InitializeMesh(Mesh collisionMesh)
    {
        _planes = new PlaneStruct[collisionMesh.triangles.Length / 3];

        for (var i = 0; i < collisionMesh.triangles.Length; i += 3)
        {
            var pointA = collisionMesh.vertices[collisionMesh.triangles[i + 0]];
            var pointB = collisionMesh.vertices[collisionMesh.triangles[i + 1]];
            var pointC = collisionMesh.vertices[collisionMesh.triangles[i + 2]];

            var collisionPlane = PlaneCollisionUtility.ComputePlaneParameters(pointA, pointB, pointC);

            _planes[i / 3] = collisionPlane;
        }
    }

    /// <summary>
    /// Returns true if the point is inside the initialized mesh
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsPointInsideMesh(float3 origin)
    {
        if (_planes == null)
            return false;

        var hitCounter = 0;
        var hit = false;

        var end = new float3(origin.x,-10f, origin.z);

        for (var i = 0; i < _planes.Length; i++)
        {
            float3 hitPoint = float3.zero;
            
            hit = PlaneCollisionUtility.ComputLineHit(
                _planes[i],
                origin,
                end,
                out hitPoint);

            if (hit)
            {
                hitCounter++;
                DrawPlaneGizmos(_planes[i], hitPoint, _planes[i].Normal);
            }
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(origin, end);
        }

        return hitCounter % 2 == 0;
    }

    private void DrawPlaneGizmos(PlaneStruct plane, float3 hitPoint, float3 hitNormal)
    {

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(plane.PointA, Vector3.one * 0.05f);
        Gizmos.DrawWireCube(plane.PointB, Vector3.one * 0.05f);
        Gizmos.DrawWireCube(plane.PointC, Vector3.one * 0.05f);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(plane.PointA, plane.PointB);
        Gizmos.DrawLine(plane.PointA, plane.PointC);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(plane.PointB, plane.PointC);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(plane.PointA, hitPoint);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(hitPoint, 0.05f);
        Gizmos.DrawLine(hitPoint, hitPoint + hitNormal);
    }
}
