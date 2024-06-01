using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayAudioManager : Singleton<GameplayAudioManager>
{
    [SerializeField] private AudioControl soundAudioControl;
    [SerializeField] private AudioControl musicAudioControl;

    public static void SoundPlayOneShot(AudioClip audioclip) {
        if (Instance.soundAudioControl != null)
        {
            Instance.soundAudioControl.PlayOneShoot(audioclip);
        }
    }
}
