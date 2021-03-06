﻿using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform _followTarget;
    [SerializeField] private float _followDelta;

    private float _firtsCameraXPos;



    public void Awake()
    {
        _firtsCameraXPos = transform.position.x;
    }

    void LateUpdate()
    {
        transform.position = new Vector3(
            _firtsCameraXPos + _followTarget.position.x * 0.5f,
            transform.position.y,
            _followTarget.position.z + _followDelta);
    }
}
