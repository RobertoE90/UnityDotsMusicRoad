using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InsideTest : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField] private int _displayIndexValue;
    [SerializeField] private Mesh _mesh;
    private CollisionUtility _collisionUtility;

    [SerializeField] private Transform _originPoint;
    [SerializeField] private Transform _endPoint;

    void Start()
    {
        if (_collisionUtility == null)
            _collisionUtility = new CollisionUtility();

        _collisionUtility.InitializeMesh(_mesh);
    }

    public void OnDrawGizmos()
    {
        /*
        if (!Application.isPlaying)
            return;
        */
        if (_collisionUtility == null)
            _collisionUtility = new CollisionUtility();

        _displayIndexValue = Mathf.Clamp(_displayIndexValue, 0, _collisionUtility.Planes.Length);
        var hitArray = _collisionUtility.IsPointInsideMesh(_originPoint.position, _endPoint.position, _displayIndexValue);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_originPoint.position, 0.25f);
        Gizmos.DrawWireSphere(_endPoint.position, 0.25f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(_originPoint.position, _endPoint.position);
    }
}
