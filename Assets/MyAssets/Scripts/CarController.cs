using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private AnimationCurve _ascelerationCurve;
    [SerializeField] private float _ascelerationHorizontalDeltaSpeed;
    [SerializeField] private float _ascelerationScalar;
    [SerializeField] private float _maxSpeed;
    private float _ascelerationHorizontalValue = 0f;


    [Header("Turn")]
    [SerializeField] private float _turnAsceleration;
    [SerializeField] private float _maxRotationSpeed;
    [SerializeField] private Vector2 _rotationRange;
    [SerializeField] private float _roadWidth;

    [Header("Car rig")]
    [SerializeField] private Transform _chasis;
    [SerializeField] private Transform[] _frontWheels;
    [SerializeField] private Transform[] _rearWheels;
    [SerializeField] private Vector2 _chasisDampingMaxAngles;
    [SerializeField] private Material _lightTraceMaterial;

    private float _movementSpeed = 0f;
    private float _rotationSpeed = 0f;
    private float _currentAsceleration = 0f;

    public void Update()
    {
        CarRotation();
        CarMovement();
        UpdateCarRig();
    }

    private void CarRotation()
    {
        var ascelerationDir = Input.GetAxis("Horizontal") * _turnAsceleration;

        if (Mathf.Abs(ascelerationDir) > 0f)
            _rotationSpeed += ascelerationDir * Time.deltaTime;
        else
            _rotationSpeed = Mathf.Lerp(0, _rotationSpeed, Time.deltaTime * _turnAsceleration * 0.015f);

        if (Mathf.Abs(_rotationSpeed) > _maxRotationSpeed)
            _rotationSpeed = _maxRotationSpeed * Mathf.Sign(_rotationSpeed);

        var rotationYValue = transform.rotation.eulerAngles.y + _rotationSpeed * Time.deltaTime;

        var normalizedRoadXPosition = transform.position.x / _roadWidth;
        
        var roadDependentAngleRange = _rotationRange;

        if (normalizedRoadXPosition > 0)
            roadDependentAngleRange.y = Mathf.Lerp(_rotationRange.y, 270f, normalizedRoadXPosition);
        else
            roadDependentAngleRange.x = Mathf.Lerp(_rotationRange.x, 270f, -1 * normalizedRoadXPosition);

        rotationYValue = Mathf.Clamp(
            rotationYValue, 
            roadDependentAngleRange.x, 
            roadDependentAngleRange.y);

        transform.rotation = Quaternion.Euler(Vector3.up * rotationYValue);
    }

    private void CarMovement()
    {
        var verticalAxisInput = Input.GetAxis("Vertical");

        if (verticalAxisInput > 0)
        {
            _ascelerationHorizontalValue += verticalAxisInput * Time.deltaTime * _ascelerationHorizontalDeltaSpeed;
            _ascelerationHorizontalValue = Mathf.Clamp01(_ascelerationHorizontalValue);
            _currentAsceleration = _ascelerationCurve.Evaluate(_ascelerationHorizontalValue) * _ascelerationScalar;
            
        }
        else
        {
            _ascelerationHorizontalValue = 0;
            _currentAsceleration -= (2.5f - verticalAxisInput) * Time.deltaTime;
        }

        _movementSpeed += _currentAsceleration * Time.deltaTime;
        _movementSpeed = Mathf.Clamp(_movementSpeed, 0f, _maxSpeed);

        Debug.Log(_movementSpeed);

        transform.position += transform.right * _movementSpeed * Time.deltaTime;
    }

    private void UpdateCarRig()
    {
        for(var i = 0; i < _frontWheels.Length; i++)
        {
            _frontWheels[i].transform.Rotate(Vector3.right * _movementSpeed);
            _rearWheels[i].transform.Rotate(Vector3.right * _movementSpeed);
        }

        var materialAlpha = 0.25f * (_movementSpeed - _maxSpeed * 0.4f) / _maxSpeed;
        materialAlpha = Mathf.Clamp01(materialAlpha);
        _lightTraceMaterial.SetFloat("AlphaScale_01", materialAlpha);
    }
}
