using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager instance;

    [SerializeField] private AudioSource soundFXObject;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;    
        }
    }

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawnTransform, float volume)
    {
        //Spawn Gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position,Quaternion.identity);

        //Asign the audioclip
        audioSource.clip = audioClip;

        //Assign Volume
        audioSource.volume = volume;

        //Play Sound!
        audioSource.Play();

        //Get the length of the obj
        float clipLength = audioSource.clip.length;

        //Destory after Play
        Destroy(audioSource.gameObject, clipLength);


    }
    public void PlayRandomSoundFXClip(AudioClip[] audioClip, Transform spawnTransform, float volume)
    {
        //Random index
        int rand = Random.Range(0, audioClip.Length);

        //Spawn Gameobject
        AudioSource audioSource = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);

        //Asign the audioclip
        audioSource.clip = audioClip[rand];

        //Assign Volume
        audioSource.volume = volume;

        //Play Sound!
        audioSource.Play();

        //Get the length of the obj
        float clipLength = audioSource.clip.length;

        //Destory after Play
        Destroy(audioSource.gameObject, clipLength);


    }

}
