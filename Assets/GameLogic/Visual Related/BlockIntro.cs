using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockIntro : MonoBehaviour
{
    public GameObject[] mblocks;
    public GameObject Player1;
    public GameObject Player2;

    public Enterfinishleft LevelSuccessLeft;
    public EnterFinishRight LevelSuccessRight;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PlayEffectsSequentially());
        StartCoroutine(EnablePlayer());
    }

    IEnumerator PlayEffectsSequentially()
    {
        yield return new WaitForSeconds(1.5f);
        // Loop through each GameObject in mblocks
        foreach (GameObject block in mblocks)
        {
            // Trigger the visual effect for the current block.
            // This assumes you have a method to play the effect. Replace "PlayVisualEffect" with your actual method.
            PlayVisualEffect(block);

            // Wait for 0.2 seconds before continuing to the next iteration of the loop
            yield return new WaitForSeconds(0.1f);
        }
    }


    IEnumerator EnablePlayer()
    {
        yield return new WaitForSeconds(3f);
        Player1.SetActive(true);
        Player2.SetActive(true);
        LevelSuccessLeft.SerachPlayer();    
        LevelSuccessRight.SerachPlayer();
    }

    void PlayVisualEffect(GameObject block)
    {
        // Assuming the visual effect is attached to the GameObject and can be enabled or triggered in some way.
        // For example, if your effect is a particle system, you might call:
        // block.GetComponent<ParticleSystem>().Play();

        // Add your code here to play the effect on the given GameObject.
        // This is just a placeholder function body.
        Block blockCode = block.GetComponent<Block>();
        if (blockCode != null)
        {
            blockCode.PlayInit();
        }

    }
}

