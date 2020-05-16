using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class VoxelsMusicUpdateData : MonoBehaviour
{
    [SerializeField] private float _carouselDepth;
    [SerializeField] private MusicSource _musicSource;

    private EntityManager _entityManager;
    private Entity _voxelMusicUpdateEntity;
    private CameraCarouselComponent _cameraCarouselComponent;

    void Awake()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _voxelMusicUpdateEntity = _entityManager.CreateEntity();

        _cameraCarouselComponent = new CameraCarouselComponent
        {
            CameraDepth = transform.position.z,
            CarouselLenght = _carouselDepth
        };

        _entityManager.AddComponentData<CameraCarouselComponent>(_voxelMusicUpdateEntity, _cameraCarouselComponent);
        _entityManager.AddComponentData<AudioSamplerComponentData>(_voxelMusicUpdateEntity, new AudioSamplerComponentData { });
    }

    void LateUpdate()
    {
        _cameraCarouselComponent.CameraDepth = transform.position.z;
        _entityManager.SetComponentData<CameraCarouselComponent>(_voxelMusicUpdateEntity, _cameraCarouselComponent);

        if (_musicSource.AudioSamples != null)
        {
            var samplesAverage = 0f;
            for (var i = 0; i < _musicSource.AudioSamples.Length; i++)
            {
                samplesAverage += Mathf.Abs(_musicSource.AudioSamples[i]);
            }
            samplesAverage /= (float)_musicSource.AudioSamples.Length;

            _entityManager.SetComponentData<AudioSamplerComponentData>(
                _voxelMusicUpdateEntity,
                new AudioSamplerComponentData { Value = samplesAverage });
        }

    }
}
