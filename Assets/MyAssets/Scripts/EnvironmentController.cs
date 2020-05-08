using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [Space(10)]
    [SerializeField] private Transform _followCameraParent;
    [Header("Looping objects")]
    [SerializeField] private float _loopObjectDepth;
    [SerializeField] private Transform[] _loopingObjects;

    private Vector3 _followDelta;

    private float _currentLoopingDelta;
    private int _loopSearchIndex = 0;
    private void Awake()
    {
        _followDelta = _followCameraParent.position - _camera.position;
        _currentLoopingDelta = (_loopingObjects.Length - 1) * _loopObjectDepth;
        for(var i = 0; i < _loopingObjects.Length; i++)
        {
            _loopingObjects[i].position = Vector3.forward * i * _loopObjectDepth;
        }
    }

    public void LateUpdate()
    {
        
        _followCameraParent.transform.position = new Vector3(
            _followCameraParent.position.x,
            _followCameraParent.position.y,
            _camera.position.z + _followDelta.z);
        

        if(_loopingObjects[_loopSearchIndex].position.z < _camera.position.z)
        {
            _loopingObjects[_loopSearchIndex].position = Vector3.forward * _currentLoopingDelta;
            _currentLoopingDelta += _loopObjectDepth;
            _loopSearchIndex = (_loopSearchIndex + 1) % _loopingObjects.Length ;
        }
    }
}
