using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioType { 
    Music,
    Sound,
}

[RequireComponent(typeof(AudioSource))]
public class AudioControl : MonoBehaviour
{
    [SerializeField] private AudioType type;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        switch (type) {
            case AudioType.Music:
                GameManager.OnMusicVolumeChanged += SetVolume;
                SetVolume(GameManager.musicVolume);
                break;
            case AudioType.Sound:
                GameManager.OnSoundVolumeChanged += SetVolume;
                SetVolume(GameManager.soundVolume);
                break;
        }
    }

    private void SetVolume(float volume) {
        audioSource.volume = volume;
    }

    private void OnDestroy() {
        switch (type) {
            case AudioType.Music:
                GameManager.OnMusicVolumeChanged -= SetVolume;
                break;
            case AudioType.Sound:
                GameManager.OnSoundVolumeChanged -= SetVolume;
                break;
        }
    }

    public void PlayOneShoot(AudioClip audioClip) {
        audioSource.PlayOneShot(audioClip);
    }

    public void Play() {
        audioSource.Play();
    }
    public void Stop()
    {
        audioSource.Stop();
    }
}
