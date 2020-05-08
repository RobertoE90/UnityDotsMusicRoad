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
    public bool IsPointInsideMesh(float3 origin, float3 end, int displayIndex)
    {
        if (_planes == null)
            return false;

        var hitCounter = 0;

        var hit = false;

        for (var j = 0; j < _planes.Length; j++)
        {
            if (j != displayIndex)
                continue;

            float3 hitPoint = float3.zero;
            float3 lineInterception = float3.zero;

            hit = PlaneCollisionUtility.ComputLineHit(
                _planes[j],
                origin,
                end,
                out hitPoint,
                out lineInterception);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(hitPoint, 0.05f);

            Gizmos.color = new Color(0.5f, 0f, 0f);
            Gizmos.DrawWireCube(lineInterception, Vector3.one * 0.05f);

        }


        return hit;
    }
}
