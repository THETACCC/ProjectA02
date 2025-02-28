using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static UnityEditor.Experimental.GraphView.GraphView;

public class BlockIntro : MonoBehaviour
{
    public GameObject[] mblocks;
    public GameObject Player1;
    public GameObject Player2;


    public GameObject LevelSuccessOBJ_Left;
    public GameObject LevelSuccessOBJ_Right;

    public Enterfinishleft LevelSuccessLeft;
    public EnterFinishRight LevelSuccessRight;

    public LevelLoader LevelLoader;

    public LevelController controller;
    // Start is called before the first frame update
    void Start()
    {
        GameObject level_controller = GameObject.Find("LevelController");
        controller = level_controller.GetComponent<LevelController>();

        controller.phase = LevelPhase.Loading;

        mblocks = GameObject.FindGameObjectsWithTag("Block");

        //get player and disable player for visual effects
        Player1 = GameObject.FindGameObjectWithTag("Player1");
        Player2 = GameObject.FindGameObjectWithTag("Player2");
        Player1.SetActive(false);
        Player2.SetActive(false);
        LevelSuccessOBJ_Left = GameObject.FindGameObjectWithTag("FinishLeft");
        LevelSuccessOBJ_Right = GameObject.FindGameObjectWithTag("FinishRight");
        LevelSuccessLeft = LevelSuccessOBJ_Left.GetComponent<Enterfinishleft>();
        LevelSuccessRight = LevelSuccessOBJ_Right.GetComponent<EnterFinishRight>();

        GameObject LevelLoaderOBJ = GameObject.FindGameObjectWithTag("LevelLoader");
        if (LevelLoaderOBJ != null )
        {
            LevelLoader = LevelLoaderOBJ.GetComponent<LevelLoader>();   
        }    
        StartCoroutine(PlayEffectsSequentially());
        StartCoroutine(EnablePlayer());
        foreach (GameObject block in mblocks)
        {
            block.transform.localScale = Vector3.zero;
        }
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
        yield return new WaitForSeconds(4f);

        Player1.SetActive(true);

        Player2.SetActive(true);
        LevelSuccessLeft.SerachPlayer();    
        LevelSuccessRight.SerachPlayer();
        LevelLoader.LoadCharacter();
        yield return new WaitForSeconds(1f);
        controller.phase = LevelPhase.Placing;
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

