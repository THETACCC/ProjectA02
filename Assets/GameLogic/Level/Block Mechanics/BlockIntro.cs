using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

    [Header("Intro Timing")]
    public float introStartDelay = 1.5f;
    public float blockPopInterval = 0.1f;
    public float playerEnableDelayAfterBlocks = 0.5f;

    [Header("Landing Check")]
    public float maxWaitForLanding = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] popSoundClip;
    [SerializeField] private AudioClip[] enablePlayerSoundClip;

    private PlayerController player1Controller;
    private PlayerController player2Controller;

    void Start()
    {
        StartCoroutine(LevelIntroRoutine());
    }

    private IEnumerator LevelIntroRoutine()
    {
        yield return null;

        GameObject level_controller = GameObject.Find("LevelController");
        if (level_controller != null)
            controller = level_controller.GetComponent<LevelController>();

        if (controller != null)
            controller.phase = LevelPhase.Loading;

        mblocks = GameObject.FindGameObjectsWithTag("Block");

        Player1 = GameObject.FindGameObjectWithTag("Player1");
        Player2 = GameObject.FindGameObjectWithTag("Player2");

        if (Player1 != null) Player1.SetActive(false);
        if (Player2 != null) Player2.SetActive(false);

        LevelSuccessOBJ_Left = GameObject.FindGameObjectWithTag("FinishLeft");
        LevelSuccessOBJ_Right = GameObject.FindGameObjectWithTag("FinishRight");

        if (LevelSuccessOBJ_Left != null)
            LevelSuccessLeft = LevelSuccessOBJ_Left.GetComponent<Enterfinishleft>();

        if (LevelSuccessOBJ_Right != null)
            LevelSuccessRight = LevelSuccessOBJ_Right.GetComponent<EnterFinishRight>();

        GameObject LevelLoaderOBJ = GameObject.FindGameObjectWithTag("LevelLoader");
        if (LevelLoaderOBJ != null)
            LevelLoader = LevelLoaderOBJ.GetComponent<LevelLoader>();

        foreach (GameObject block in mblocks)
        {
            if (block != null)
                block.transform.localScale = Vector3.zero;
        }

        yield return StartCoroutine(PlayEffectsSequentially());

        yield return new WaitForSeconds(playerEnableDelayAfterBlocks);

        yield return StartCoroutine(EnablePlayersAndWaitForLanding());
    }

    private IEnumerator PlayEffectsSequentially()
    {
        yield return new WaitForSeconds(introStartDelay);

        foreach (GameObject block in mblocks)
        {
            if (block == null) continue;

            PlayVisualEffect(block);

            if (SoundFXManager.instance != null)
                SoundFXManager.instance.PlayRandomSoundFXClip(popSoundClip, transform, 1f);

            yield return new WaitForSeconds(blockPopInterval);
        }
    }

    private IEnumerator EnablePlayersAndWaitForLanding()
    {
        if (SoundFXManager.instance != null)
            SoundFXManager.instance.PlayRandomSoundFXClip(enablePlayerSoundClip, transform, 1f);

        if (Player1 != null)
            Player1.SetActive(true);

        if (Player2 != null)
            Player2.SetActive(true);

        yield return null;

        if (Player1 != null)
            player1Controller = Player1.GetComponent<PlayerController>();

        if (Player2 != null)
            player2Controller = Player2.GetComponent<PlayerController>();

        if (LevelSuccessLeft != null)
            LevelSuccessLeft.SerachPlayer();

        if (LevelSuccessRight != null)
            LevelSuccessRight.SerachPlayer();

        if (LevelLoader != null)
            LevelLoader.LoadCharacter();

        float timer = 0f;

        while (timer < maxWaitForLanding)
        {
            bool p1Landed = player1Controller != null && player1Controller.hasLanded;
            bool p2Landed = player2Controller != null && player2Controller.hasLanded;

            if (p1Landed && p2Landed)
                break;

            timer += Time.deltaTime;
            yield return null;
        }

        if (controller != null)
        {
            controller.RegisterPlayers(player1Controller, player2Controller);

            controller.playersReadyForInteraction = true;
            controller.phase = LevelPhase.Placing;
        }
    }

    private void PlayVisualEffect(GameObject block)
    {
        Block blockCode = block.GetComponent<Block>();

        if (blockCode != null)
            blockCode.PlayInit();
    }
}