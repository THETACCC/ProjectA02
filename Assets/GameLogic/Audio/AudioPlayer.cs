using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer instance;


    //Block Related Audio
    [SerializeField] private AudioClip[] blockSelectSound;
    [SerializeField] private AudioClip[] blockPlaceSound;
    [SerializeField] private AudioClip[] blockRotateSound;
    [SerializeField] private AudioClip[] blockHoverSound;

    //Menu related Audio
    [SerializeField] private AudioClip[] hoverSound;
    [SerializeField] private AudioClip[] levelEndSound;
    [SerializeField] private AudioClip[] levelSuccessSound;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void playBlockSelectSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(blockSelectSound, transform, 1f);
    }

    public void playBlockPlaceSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(blockPlaceSound, transform, 1f);
    }
  
    public void playBlockRotateSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(blockRotateSound, transform, 1f);
    }

    public void playBlockHoverSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(blockHoverSound, transform, 0.3f);
    }

    public void playUIHoverSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(hoverSound, transform, 0.3f);
    }

    public void playlevelEndSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(levelEndSound, transform, 1f);
    }
    public void playlevelSuccessSound()
    {
        SoundFXManager.instance.PlayRandomSoundFXClip(levelSuccessSound, transform, 1f);
    }
}
