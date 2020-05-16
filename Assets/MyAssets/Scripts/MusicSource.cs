using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicSource : MonoBehaviour
{
    [SerializeField] private AudioClip _currentClip;
    
    private AudioSource _audioSource;
    private float[] _audioSamples;
    public float[] AudioSamples => _audioSamples;

    public int Frequency
    {
        get
        {
            return _currentClip.frequency;
        }
    }
    
    public int Channels
    {
        get
        {
            return _currentClip.channels;
        }
    }


    public void Initialize()
    {
        _audioSource = GetComponent<AudioSource>();
        _audioSamples = new float[30];

        _audioSource.clip = _currentClip;
        _audioSource.Play();
    }

    private void Awake()
    {
        Initialize();
    }

    private void Update()
    {
        if (_audioSamples == null)
            return;
        try
        {
            _currentClip.GetData(_audioSamples, _audioSource.timeSamples);
        }
        catch (Exception e)
        {

        }

    }
}
