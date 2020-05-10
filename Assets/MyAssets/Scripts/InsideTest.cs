using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class InsideTest : MonoBehaviour
{
    [SerializeField] private GameObject _volumeMesh;
    private Renderer _volumeRenderer;
    private CollisionUtility _collisionUtility;

    [SerializeField] private Transform _originPoint;
    
    void Start()
    {
    //    if (_collisionUtility == null)
    //        _collisionUtility = new CollisionUtility();

        _volumeRenderer = _volumeMesh.GetComponent<Renderer>();
    //    _collisionUtility.InitializeMesh(_volumeMesh.GetComponent<MeshFilter>().mesh);
    }
    

    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        //var insideMesh = _collisionUtility.IsPointInsideMesh(_originPoint.position);

        //Gizmos.color = insideMesh? Color.green : Color.red;
        //Gizmos.DrawWireSphere(_originPoint.position, 0.25f);

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_volumeRenderer.bounds.center, _volumeRenderer.bounds.size);
    }
}
