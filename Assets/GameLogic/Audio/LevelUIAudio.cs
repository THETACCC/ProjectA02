using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUIAudio : MonoBehaviour
{

    //Audio
    [SerializeField] private AudioClip[] clickSoundClip;
    [SerializeField] private AudioClip[] closeSoundClip;

    public void playUIAudio()
    {
        //Audio
        SoundFXManager.instance.PlayRandomSoundFXClip(clickSoundClip, transform, 1f);
    }


    public void closeUIAudio()
    {
        //Audio
        SoundFXManager.instance.PlayRandomSoundFXClip(closeSoundClip, transform, 1f);
    }

}
