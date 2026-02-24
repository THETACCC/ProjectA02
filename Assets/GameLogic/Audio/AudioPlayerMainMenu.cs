using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayerMainMenu : MonoBehaviour
{
    //Audio
    [SerializeField] private AudioClip[] otherSelectSoundClip;
    [SerializeField] private AudioClip[] selectSoundClip;

    public void otherSelectUIAudio()
    {
        //Audio
        SoundFXManager.instance.PlayRandomSoundFXClip(otherSelectSoundClip, transform, 1f);
    }


    public void selectUIAudio()
    {
        //Audio
        SoundFXManager.instance.PlayRandomSoundFXClip(selectSoundClip, transform, 1f);
    }
}
