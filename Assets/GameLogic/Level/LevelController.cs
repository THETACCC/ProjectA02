using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using SKCell;


public class LevelController : SKMonoSingleton<LevelController>
{
    [Header("Chapter Settings")]
    public int chapterIndex = 0;

    public int Level { get; private set; }
    [SerializeField]
    private LevelPhase _phase;
    public LevelPhase phase
    {
        get => _phase;
        set
        {
            if (_phase != value)
            {
              //  Debug.Log($"[PHASE CHANGE] {Time.time:F2}s: {name} changed phase from {_phase} ¡ú {value}.\n" +
              //            $"Call stack:\n{new System.Diagnostics.StackTrace()}");

                _phase = value;
            }
        }
    }


    public Block curDraggedblock = null;
    public Block curOverBlock = null;
    //private void Start()
    //{
    //   InitLevel();
    // }

    //Get level loader
    private LevelLoader levelLoader;


    //GetBlock Logic
    public GameObject[] mblocks;
    public Block[] mblockCode;

    public GameObject[] mLevelbrick;
    public BlockAlignment[] malignment;
    public RegularBlockPlacement[] mRegularPlacement;


    public bool isAnyBlockBeingDragged = false;
    public bool isAnyBlockDragging = false;

    public AnimationCurve IndicatoreffectCurve;   // The curve controlling the transition
    public float transitionDuration = 1f; // Total time to complete the effect (forward or backward)
    private float transitionTimer = 0f;
    private float transitionTimerReverse = 0f;
    public MeshRenderer myRenderer;
    public Material myMaterial;

    //Left Right Player Controll Related
    public bool isPlayingRight = true;
    public PlayerController playerLeft;
    public PlayerController playerRight;
    public Rigidbody playerLeftRB;
    public Rigidbody playerRightRB;
    public Collider playerLeftCollider;
    public Collider playerRightCollider;

    private bool isPlayerFound = false;

    private void Start()
    {

        //Controlls the indicator material
        //GameObject IndicatorMat = GameObject.FindGameObjectWithTag("IndicatorMaterial");
        //myRenderer = IndicatorMat.GetComponent<MeshRenderer>();
        //myMaterial = myRenderer.material;
        // Set initial Effect_Time value
        myMaterial.SetFloat("_Effect_Time", 2f);


        mblocks = GameObject.FindGameObjectsWithTag("Block");
        mLevelbrick = GameObject.FindGameObjectsWithTag("LevelBrick");
        // Initialize the mblockCode array with the same size as mblocks
        mblockCode = new Block[mblocks.Length];
        malignment = new BlockAlignment[mLevelbrick.Length];
        mRegularPlacement = new RegularBlockPlacement[mLevelbrick.Length];

        //The Logic to get all the alignment 
        for (int i = 0; i < mLevelbrick.Length; i++)
        {

            malignment[i] = mLevelbrick[i].GetComponent<BlockAlignment>();
            mRegularPlacement[i] = mLevelbrick[i].GetComponent<RegularBlockPlacement>();
            if (malignment[i] == null)
            {
                Debug.LogWarning("Block component missing on GameObject: " + malignment[i].name);
            }
        }


        // Iterate over each GameObject in mblocks
        for (int i = 0; i < mblocks.Length; i++)
        {
            // Get the Block component from the GameObject and store it in mblockCode
            mblockCode[i] = mblocks[i].GetComponent<Block>();

            // Check if the Block component exists to avoid potential null reference exceptions
            if (mblockCode[i] == null)
            {
                Debug.LogWarning("Block component missing on GameObject: " + mblocks[i].name);
            }
        }

        foreach (BlockAlignment mAlign in malignment)
        {

            mAlign.PlaceIndicator.SetActive(false);

        }
        GameObject level_loader = GameObject.Find("LevelLoader");
        levelLoader = level_loader.GetComponent<LevelLoader>();
    }

    private void Update()
    {
        if(!isPlayerFound)
        {
            FindPlayers();
        }

        if (phase == LevelPhase.Running)
        {
            /*
            if (Input.GetKeyDown(KeyCode.F))
            {
                playerRight.SwitchPlayer();

                if (isPlayingRight)
                {
                    isPlayingRight = false;
                }
                else if (!isPlayingRight)
                {
                    isPlayingRight = true;
                }
            }
            */
        }

        //Commented out for debug
        //New method
        if(curOverBlock!= null)
        {
            if (curOverBlock.type == BlockType.Regular)
            {
                foreach (RegularBlockPlacement mRegularBlockPlace in mRegularPlacement)
                {
                    if (mRegularBlockPlace != null && mRegularBlockPlace.isRegularCanPlace == true)
                    {
                        mRegularBlockPlace.PlaceIndicator.SetActive(true);
                    }
                }
            }
            else if (curOverBlock.type == BlockType.Free)
            {
                foreach (BlockAlignment mAlign in malignment)
                {
                    if (mAlign != null && mAlign.isBlocked == false)
                    {
                        mAlign.PlaceIndicator.SetActive(true);
                    }
                }
            }
        }



        if (curDraggedblock != null)
        {
            isAnyBlockBeingDragged = true;
            isAnyBlockDragging = true;

        }
        else
        {
            isAnyBlockBeingDragged = false;
            isAnyBlockDragging = false;
        }
        // Ensure the transition timer remains within [0, 1] bounds for Evaluate() usage
        transitionTimer = Mathf.Clamp01(transitionTimer);
        transitionTimerReverse = Mathf.Clamp01(transitionTimerReverse);
        /*
        foreach (Block block in mblockCode)
        {
            // Check if the block is dragging
            if (block != null && block.mouse_over)
            {

                if (block.type == BlockType.Regular)
                {
                    // Send a debug message when isDragging is true
                    Debug.Log("Open indicator");

                    isAnyBlockBeingDragged = true;
                    foreach (RegularBlockPlacement mRegularBlockPlace in mRegularPlacement)
                    {
                        if (mRegularBlockPlace != null && mRegularBlockPlace.isRegularCanPlace == true)
                        {
                            mRegularBlockPlace.PlaceIndicator.SetActive(true);
                        }
                    }

                    if(block.isDragging)
                    {
                        isAnyBlockDragging = true;
                    }


                }
                else if(block.type == BlockType.Free)
                {
                    isAnyBlockBeingDragged = true;
                    foreach (BlockAlignment mAlign in malignment)
                    {
                        if (mAlign != null && mAlign.isBlocked == false)
                        {
                            Debug.Log("Block is being dragged: " + block.gameObject.name);
                            Debug.Log(isAnyBlockBeingDragged);
                            mAlign.PlaceIndicator.SetActive(true);
                        }
                    }

                    if (block.isDragging)
                    {
                        isAnyBlockDragging = true;
                    }
                    // Send a debug message when isDragging is true

                }



            }

        }
        */

        if (curOverBlock != null)
        {
            transitionTimerReverse = 0;
            //Debug.Log("At least one block is being dragged.");
            transitionTimer += Time.deltaTime / transitionDuration;
            float effectTime = Mathf.Lerp(1f, -1f, IndicatoreffectCurve.Evaluate(transitionTimer));
            myMaterial.SetFloat("_Effect_Time", effectTime);
        }
        else
        {
            foreach (BlockAlignment mAlign in malignment)
            {

                StartCoroutine(HidePlaceIndicatorsWithDelay(transitionDuration));

            }

            transitionTimerReverse += Time.deltaTime / transitionDuration;
            //Debug.Log("No blocks are being dragged.");
            float effectTime = Mathf.Lerp(-1f, 1f, IndicatoreffectCurve.Evaluate(transitionTimerReverse));
            myMaterial.SetFloat("_Effect_Time", effectTime);
        }

        



        // Update transition timer within the given duration



        // Evaluate the curve based on the normalized timer



        //CHEAT CODE




        if ( (phase != LevelPhase.Speaking ) && (Input.GetKeyDown(KeyCode.Space)))
        {
            //levelLoader.AlignBlockSelection();

            bool wasRunning = (phase == LevelPhase.Running);

            phase = LevelPhase.Running;

            if (!wasRunning && LevelTimer.Instance != null)
                LevelTimer.Instance.StartTimer();
        }
        
    }

    public void FindPlayers()
    {
        if (playerLeft == null)
        {
            GameObject playerleft = GameObject.FindGameObjectWithTag("Player1");
            if (playerleft != null)
            {
                playerLeftCollider = playerleft.GetComponent<Collider>();
                playerLeftRB = playerleft.GetComponent<Rigidbody>();
                playerLeft = playerleft.GetComponent<PlayerController>();
            }

            GameObject playerright = GameObject.FindGameObjectWithTag("Player2");
            if (playerright != null)
            {
                playerRight = playerright.GetComponent<PlayerController>();
                playerRightCollider = playerright.GetComponent<Collider>();
                playerRightRB = playerright.GetComponent<Rigidbody>();
                isPlayerFound = true;
            }

        }
    }

    public void InitLevel()
    {
        phase = LevelPhase.Loading;
        LevelLoader.instance.Load();
        //there is a bug here where the phase will not change to place
    }

    public void DisablePlayerColliders()
    {
        playerLeftRB.useGravity= false;
        playerRightRB.useGravity= false;
        playerLeftCollider.enabled= false;
        playerRightCollider.enabled= false;
        playerLeft.isLevelDragging = true;
        playerRight.isLevelDragging = true;
    }
    public void EnablePlayerColliders()
    {
        playerLeftCollider.enabled = true;
        playerRightCollider.enabled = true;
        playerLeftRB.useGravity = true;
        playerRightRB.useGravity = true;
        playerLeft.isLevelDragging = false;
        playerRight.isLevelDragging = false;

    }

    private IEnumerator HidePlaceIndicatorsWithDelay(float delay)
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(delay);

        // Hide the PlaceIndicator after the delay
        foreach (BlockAlignment mAlign in malignment)
        {
            if (mAlign != null)
            {
                transitionTimer = 0;
                mAlign.PlaceIndicator.SetActive(false);
            }
        }
    }

}

public enum LevelPhase
{
    Loading,
    Placing,
    Draging,
    Speaking,
    Sprinting,
    Running
}
