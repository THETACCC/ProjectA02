using SKCell;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;



public class Block : MonoBehaviour
{
    // ===== Debug (visible in build) =====
    [Header("Debug")]
    public bool showMirrorGhostInBuild = true;   // toggle in Inspector (or set true by default)
    public KeyCode toggleMirrorGhostKey = KeyCode.F9;

    private GameObject _mirrorGhost;             // the intended mirror position
    private GameObject _mirrorActual;            // where the clone actually ended up (after instantiate)
    private LineRenderer _mirrorLine;            // line from this -> intended mirror
    private TextMesh _mirrorLabel;

    static float MOUSE_OVER_SCALE = 1.15f;
    //This is the type of the block
    public BlockType type;
    //This determines whether the block is in the inventory area
    public bool isInventory = false;
    public bool draggable = true;

    public GameObject cld_0, cld_1,cld_2;

    private Vector3 oScale;
    private Coroutine CR_mouseOver;

    public bool mouse_drag, is_drag_start_in_select_area;
    [HideInInspector]
    public bool mouse_over, prev_mouseOver;

    private Vector3 drag_offset, drag_start_pos, rotate_start_pos;

    public Vector3 moveposition = Vector3.zero;


    private Transform subBlock;
    private Transform effectContainer;
    private GameObject FX_HOVER;

    private LevelController controller;

    public float rotatespeed = 10f;
    public Vector3 rotate = new Vector3(0, 0, 0);
    private bool _isRotating = false;
    public bool rotated = false;

    //Link blocks
    public Block linkedblock;
    public bool canlink;
    [SerializeField] public GameObject blocka;
    [SerializeField] public GameObject blockb;
    [SerializeField] private Block B_blocka;
    [SerializeField] private Block B_blockb;
    //60f in editor
    //78.1 in Build
    private float offset = 60f;
    private float offsetz =  -60f;
    [SerializeField] private bool linked = false;
    private Vector3 bcpos;
    private Vector3 bnpos;
    private Vector3 cpos;
    private Vector3 npos;
    private Vector3 BlockA_Drag_Start_Pos;
    private Vector3 BlockB_Drag_Start_Pos;
    private bool isBlockALeft = false;
    private bool instantiated = false;
    public bool DragsuccessA;
    public bool DragsuccessB;


    //mouse position
    private Vector3 mouseposition;


    //Get level loader
    private LevelLoader levelLoader;


    //Get the inventory
    public GameObject inventoryManagerOBJ;
    public InventoryManager inventoryManager;

    //Get the map OBJ

    [SerializeField]
    private GameObject LevelLeft;
    [SerializeField]
    private GameObject LevelRight;
    [SerializeField]
    private LevelRotation levelrotation;
    private bool rotationFound = false;

    //Get the
    //
    //effect
    public Outline outlineEffect;
    public GameObject outlineEffectOBJ;
    public bool allowOutline = true;

    //Visual Effects
    public int levelDimensions = 3;
    public GameObject onDestroyParticle;
    public MMFeedbacks init;
    public MMFeedbacks init4x4;
    private float lerpSpeed = 10f;
    //player related
    public bool isDragging = false;

    //method for checking if the player is on the block
    [HideInInspector] public bool isPlayerOnBlock = false;

    //New Method for Block Alignment
    public BlockAlignment myAlignedBrick;
    private bool isOutofBound_Mirror = false;

    //Regular Block offest Calculations
    private GameObject gameLevelLeft;
    private GameObject gameLevelRight;

    //local scale
    private Vector3 myLocalScale;

    //BreakableGround Mechanic
    public BreakableGround BreakableGroundScript;
    public BreakableGround BreakableGroundScript2;
    public BreakableGround BreakableGroundScript3;
    //SpawnGroundMechanic
    public SpawnBlock SpawnBlockScript;
    public SpawnBlock SpawnBlockScript2;
    public SpawnBlock SpawnBlockScript3;

    private void Awake()
    {
        // Robust outline assignment (works for originals & clones; Editor & Player)
        if (outlineEffectOBJ == null)
        {
            var child = transform.Find("OutlineEffect");
            if (child != null) outlineEffectOBJ = child.gameObject;
        }

        // Optional: cache Outline too, but guard it.
        if (outlineEffect == null)
            outlineEffect = GetComponentInChildren<Outline>();
    }

    private void Start()
    {
        StartCoroutine(ExecuteAfterDelay(0.1f));
        myLocalScale = this.gameObject.transform.localScale;
        cld_0 = transform.Find("Base")?.gameObject;
        cld_1 = transform.Find("Lower")?.gameObject;
        cld_2 = transform.Find("Upper")?.gameObject;
        if (cld_0 != null)
        {
            cld_0.tag = "Wall";
        }

        if(cld_1 != null)
        {
            cld_1.tag = "Wall";
        }

        subBlock = transform.Find("SubBlock");
        if (subBlock != null)
        {
            effectContainer = subBlock.Find("Effects");
            if (effectContainer != null)
            {
                var fx = effectContainer.Find("FX_HOVER");
                if (fx != null) FX_HOVER = fx.gameObject;
            }
        }

        //instantiate outline effect
        if (outlineEffectOBJ != null)
        {
            if(allowOutline)
            {
                Transform childTransform = transform.Find("OutlineEffect");
                outlineEffectOBJ = childTransform.gameObject;
                outlineEffectOBJ.SetActive(false);
            }

        }

        //get the inventory manager at start
        //inventoryManagerOBJ = GameObject.FindGameObjectWithTag("Inventory");
        //inventoryManager = inventoryManagerOBJ.GetComponent<InventoryManager>();

        rotate = transform.rotation.eulerAngles;

        canlink = true;

        //GameObject level_loader = GameObject.Find("LevelLoader");
        //levelLoader = level_loader.GetComponent<LevelLoader>();

        GameObject level_controller = GameObject.Find("LevelController");
        controller = level_controller.GetComponent<LevelController>();



        //UpdateMapCollider();
        oScale = transform.localScale;

        //Align Blocks
        if(isInventory)
        {
            GameObject[] inventoryBricks = GameObject.FindGameObjectsWithTag("LevelBrickInventory");
            GameObject nearestBlock = null;
            float nearestDistance = Mathf.Infinity;
            foreach (GameObject levelBrick in inventoryBricks)
            {
                float distance = Vector3.Distance(transform.position, levelBrick.transform.position);
                BlockAlignment alignment = levelBrick.GetComponent<BlockAlignment>();
                // Check if this block is closer than the previously found ones and within the threshold
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestBlock = levelBrick;
                }
            }

            // Align with the nearest block if it's within range
            if (nearestBlock != null)
            {

                BlockAlignment alignment = nearestBlock.GetComponent<BlockAlignment>();
                myAlignedBrick = alignment;
                alignment.isBlocked = true;

            }
        }


    }
    IEnumerator ExecuteAfterDelay(float delay)
    {
        // Wait for the specified amount of time (0.1 seconds)
        yield return new WaitForSeconds(delay);

        // Call the function you want to execute after the delay
        CalculateOffset();
    }

    private void CalculateOffset()
    {
        if (type == BlockType.Regular)
        {
            gameLevelLeft = GameObject.FindGameObjectWithTag("LevelLeft");
            gameLevelRight = GameObject.FindGameObjectWithTag("LevelRight");

            offset = Mathf.Abs(gameLevelLeft.transform.position.x - gameLevelRight.transform.position.x);
            offsetz = -Mathf.Abs(gameLevelLeft.transform.position.z - gameLevelRight.transform.position.z);
            //offset = Vector3.Distance(gameLevelLeft.transform.position, gameLevelRight.transform.position) - 24.7521f;
            //offsetz = -Vector3.Distance(gameLevelLeft.transform.position, gameLevelRight.transform.position) + 24.7521f;
            Debug.Log(offset);
            //THis is for build use

            //offset += 18f;
            //offsetz -= 18f;
        }
    }
    private void Update()
    {
        //If speaking then the player cannot move the blocks
        if(controller.phase != LevelPhase.Speaking && controller.phase != LevelPhase.Loading && controller.phase != LevelPhase.Sprinting)
        {
            if (!rotationFound)
            {
                FindLevelRot();
            }



            //Controlls the Block Rotation When Right Clicked
            if (type != BlockType.Obstacle)
            {

                if (type == BlockType.Regular)
                {
                    if(instantiated && !isDragging)
                    {
                        if (B_blocka.isPlayerOnBlock == false && B_blockb.isPlayerOnBlock == false)
                        {
                            UpdateMouseBehavior();
                            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);

                            //Needs to be addressed in future updates as it is confliting with the intro
                            transform.localScale = Vector3.Lerp(transform.localScale, myLocalScale, Time.deltaTime * rotatespeed * 0.75f);
                        }
                        else if ((B_blocka.isPlayerOnBlock == true && B_blockb.isPlayerOnBlock == true) && (B_blocka.isDragging == true && B_blockb.isPlayerOnBlock == isDragging))
                        {
                            UpdateMouseBehavior();
                            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);
                            transform.localScale = Vector3.Lerp(transform.localScale, myLocalScale, Time.deltaTime * rotatespeed * 0.75f);
                        }
                    }
                    else
                    {
                        if (!isPlayerOnBlock)
                        {
                            UpdateMouseBehavior();
                            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);

                            //Needs to be addressed in future updates as it is confliting with the intro
                            transform.localScale = Vector3.Lerp(transform.localScale, myLocalScale, Time.deltaTime * rotatespeed * 0.75f);
                        }
                        else if (isPlayerOnBlock && isDragging)
                        {
                            UpdateMouseBehavior();
                            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);
                            transform.localScale = Vector3.Lerp(transform.localScale, myLocalScale, Time.deltaTime * rotatespeed * 0.75f);
                        }
                    }

                }
                else if (type == BlockType.Free)
                {
                    if(!isPlayerOnBlock)
                    {
                        UpdateMouseBehavior();
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);

                        //Needs to be addressed in future updates as it is confliting with the intro
                        transform.localScale = Vector3.Lerp(transform.localScale, myLocalScale, Time.deltaTime * rotatespeed * 0.75f);
                    }
                    else if (isPlayerOnBlock && isDragging)
                    {
                        UpdateMouseBehavior();
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);
                        transform.localScale = Vector3.Lerp(transform.localScale, myLocalScale, Time.deltaTime * rotatespeed * 0.75f);
                    }
                }




            }



        }

        if (type == BlockType.Regular)
        {
            if(!isDragging && instantiated)
            {
                if (BreakableGroundScript != null)
                {
                    if (BreakableGroundScript.isBreak == true)
                    {
                        Destroy(B_blocka.BreakableGroundScript.gameObject);
                        Destroy(B_blockb.BreakableGroundScript.gameObject);
                    }
                }

                if (BreakableGroundScript2 != null)
                {
                    if (BreakableGroundScript2.isBreak == true)
                    {
                        Destroy(B_blocka.BreakableGroundScript2.gameObject);
                        Destroy(B_blockb.BreakableGroundScript2.gameObject);
                    }
                }

                if (BreakableGroundScript3 != null)
                {
                    if (BreakableGroundScript3.isBreak == true)
                    {
                        Destroy(B_blocka.BreakableGroundScript3.gameObject);
                        Destroy(B_blockb.BreakableGroundScript3.gameObject);
                    }
                }

                if (SpawnBlockScript != null)
                {
                    if (SpawnBlockScript.isSpawn == true)
                    {

                        B_blocka.SpawnBlockScript.StartSpawn();

                        B_blockb.SpawnBlockScript.StartSpawn();


                    }
                }

                if (SpawnBlockScript2 != null)
                {
                    if (SpawnBlockScript2.isSpawn == true)
                    {

                        B_blocka.SpawnBlockScript2.StartSpawn();

                        B_blockb.SpawnBlockScript2.StartSpawn();


                    }
                }

                if (SpawnBlockScript3 != null)
                {
                    if (SpawnBlockScript3.isSpawn == true)
                    {

                        B_blocka.SpawnBlockScript3.StartSpawn();

                        B_blockb.SpawnBlockScript3.StartSpawn();


                    }
                }
            }


        }

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player1") || other.CompareTag("Player2"))
        {
            isPlayerOnBlock = true;

            if (type == BlockType.Free)
            {
                if (outlineEffectOBJ != null) outlineEffectOBJ.SetActive(false);
            }
            else if (type == BlockType.Regular)
            {
                if (B_blocka != null && B_blocka.outlineEffectOBJ != null)
                    B_blocka.outlineEffectOBJ.SetActive(false);
                if (B_blockb != null && B_blockb.outlineEffectOBJ != null)
                    B_blockb.outlineEffectOBJ.SetActive(false);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.tag == "Player1") || (other.gameObject.tag == "Player2"))
        {
            isPlayerOnBlock = false;
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.tag == "Player1") && (type == BlockType.End))
        {
            collision.gameObject.GetComponent<CharacterMovement>().enabled = false;
        }
    }

    private void UpdateMouseBehavior()
    {

        Ray ray = CommonReference.mainCam.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, 100000, 1 << CommonReference.LAYER_GROUND);
        mouse_over = hit.collider == null ? false : hit.collider.gameObject == this.gameObject;
        if (!prev_mouseOver && mouse_over)
        {
            if(draggable)
            {
                LevelController.instance.curOverBlock = this;
                _OnMouseEnter();
            }

        }
        if(mouse_over)
        {
            if (draggable)
            {
                LevelController.instance.curOverBlock = this;
            }
        }


        if (prev_mouseOver && !mouse_over)
        {
            if(draggable)
            {
                LevelController.instance.curOverBlock = null;
                _OnMouseExit();
            }
            isDragging = false;
        }
        if (!mouse_drag && LevelController.instance.curDraggedblock == null && mouse_over && Input.GetMouseButtonDown(0))
        {
            mouse_drag = true;
            controller.DisablePlayerColliders();
            _OnStartDrag();
            LevelController.instance.curDraggedblock = this;
        }
        if (!mouse_drag && LevelController.instance.curDraggedblock == null && mouse_over && Input.GetMouseButtonDown(1))
        {

            if (type == BlockType.Regular)
            {
                Debug.Log("Rotating");
                B_blocka.rotate += new Vector3(0, 90, 0);
                B_blocka.rotated = true;
                B_blockb.rotate += new Vector3(0, 90, 0);
                B_blockb.rotated = true;
                B_blocka.transform.localScale = new Vector3(.5f, .5f, .5f);
                B_blockb.transform.localScale = new Vector3(.5f, .5f, .5f);
            }
            else if (type == BlockType.Free)
            {

                rotate += new Vector3(0, 90, 0);
                rotated = true;
                transform.localScale = new Vector3(.5f, .5f, .5f);


            }
        }
        else
        {
            rotated = false;
        }

        if (mouse_drag)
        {
            isDragging = true;


            _OnMouseDrag();
            if (Input.GetMouseButtonUp(0))
            {
                mouse_drag = false;
                Debug.Log("On  End Draingg");
                controller.EnablePlayerColliders();
                _OnEndDrag();
                LevelController.instance.curDraggedblock = null;
            }
        }
        prev_mouseOver = mouse_over;
    }

    /// <summary>
    /// Toggle left/right map colliders
    /// </summary>
    public void UpdateMapCollider()
    {
        int map = LevelLoader.PosToMapID(transform.position);
        if (map == 0)
        {
            cld_0.SetActive(true);
            cld_1.SetActive(true);
        }
        else
        {
            cld_0.SetActive(true);
            cld_1.SetActive(true);
        }
    }

    public void SyncLocalScale(Vector3 scale)
    {
        oScale = scale;
    }

    private void _OnMouseEnter()
    {

        if (!isInventory)
        {
            if ((type == BlockType.Regular))
            {
                if (instantiated)
                {

                    B_blocka.outlineEffectOBJ.SetActive(true);
                    B_blockb.outlineEffectOBJ.SetActive(true);

                }
            }
            else
            {

                outlineEffectOBJ.SetActive(true);


            }
        }
        else
        {
            outlineEffectOBJ.SetActive(true);
        }

        //FX_HOVER.SetActive(true);
    }

    private void _OnMouseExit()
    {
        if (!isInventory)
        {
            if ((type == BlockType.Regular))
            {
                if (instantiated)
                {

                    B_blocka.outlineEffectOBJ.SetActive(false);
                    B_blockb.outlineEffectOBJ.SetActive(false);
                    //outlineEffect.enabled = false;


                }
            }
            else
            {

                outlineEffectOBJ.SetActive(false);

            }
        }
        else
        {
            outlineEffectOBJ.SetActive(false);
        }
        //FX_HOVER.SetActive(false);
    }

    public void _OnStartDrag()
    {
        if (!isInventory)
        {
            if (type == BlockType.Regular)
            {
                //These controlls the information that is instantiated for both blocks when dragging
                if (instantiated)
                {
                    //disable the collision box
                    B_blocka.cld_0.gameObject.SetActive(false);
                    B_blocka.cld_1.gameObject.SetActive(false);
                    B_blocka.cld_2.gameObject.SetActive(false);
                    B_blockb.cld_0.gameObject.SetActive(false);
                    B_blockb.cld_1.gameObject.SetActive(false);
                    B_blockb.cld_2.gameObject.SetActive(false);
                    //disable Collision box for the mechanics





                    if (myAlignedBrick != null)
                    {
                        B_blocka.myAlignedBrick.isBlocked = false;
                        B_blockb.myAlignedBrick.isBlocked = false;
                    }
                    if (this == B_blocka) // If this is the original block
                    {
                        float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                        drag_start_pos = transform.position;
                        BlockA_Drag_Start_Pos = blocka.transform.position;
                        BlockB_Drag_Start_Pos = blockb.transform.position;
                        B_blockb.BlockA_Drag_Start_Pos = blocka.transform.position;
                        B_blockb.BlockB_Drag_Start_Pos = blockb.transform.position;
                        is_drag_start_in_select_area = false;
                        moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                        drag_offset = transform.position - moveposition;
                        drag_offset.y = 0;


                        bool isLeft = blockb.transform.position.x < LevelLoader.center.x;
                        B_blockb.drag_start_pos = blockb.transform.position;
                        B_blockb.is_drag_start_in_select_area = false;
                        B_blockb.moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + (isLeft ? offset : -offset), Input.mousePosition.y, distance_to_screen));
                        B_blockb.drag_offset = blockb.transform.position - B_blockb.moveposition;
                        B_blockb.drag_offset.y = 0;

                    }
                    else if (this == B_blockb)
                    {
                        float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                        drag_start_pos = transform.position;
                        BlockA_Drag_Start_Pos = blocka.transform.position;
                        BlockB_Drag_Start_Pos = blockb.transform.position;
                        B_blocka.BlockA_Drag_Start_Pos = blocka.transform.position;
                        B_blocka.BlockB_Drag_Start_Pos = blockb.transform.position;
                        is_drag_start_in_select_area = false;
                        moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                        drag_offset = transform.position - moveposition;
                        drag_offset.y = 0;


                        bool isLeft = blocka.transform.position.x < LevelLoader.center.x;
                        B_blocka.drag_start_pos = blocka.transform.position;
                        B_blocka.is_drag_start_in_select_area = false;
                        B_blocka.moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + (isLeft ? offset : -offset), Input.mousePosition.y, distance_to_screen));
                        B_blocka.drag_offset = blocka.transform.position - B_blocka.moveposition;
                        B_blocka.drag_offset.y = 0;

                    }

                    if (this == B_blocka)
                    {
                        Debug.Log("Dragged Block A");
                    }
                    else
                    {
                        Debug.Log("Dragged Block B");
                    }
                    if (this.gameObject == blocka)
                    {
                        Debug.Log("Dragged Block A OBJ");
                    }
                    else
                    {
                        Debug.Log("Dragged Block B OBJ");
                    }
                }
                else
                {



                    drag_start_pos = transform.position;
                    //is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(drag_start_pos);
                    float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                    moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                    drag_offset = transform.position - moveposition;
                    drag_offset.y = 0;
                    //disable collision
                    cld_0.gameObject.SetActive(false);
                    cld_1.gameObject.SetActive(false);
                    cld_2.gameObject.SetActive(false);
                }


            }
            else if (type == BlockType.Free)
            {
                if (myAlignedBrick != null)
                {
                    myAlignedBrick.isBlocked = false;
                }
                //drag_start_pos = LevelLoader.WorldToCellPos(this.transform.position);
                is_drag_start_in_select_area = false;
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                drag_offset = transform.position - moveposition;
                drag_offset.y = 0f;
                //disable collision
                cld_0.gameObject.SetActive(false);
                cld_1.gameObject.SetActive(false);
                cld_2.gameObject.SetActive(false);

            }

        }
        else if (isInventory)
        {
            if (type == BlockType.Free)
            {
                //Make the aligned block Free
                if (myAlignedBrick != null)
                {
                    myAlignedBrick.isBlocked = false;
                }

                //Dragging Operations
                //drag_start_pos = LevelLoader.WorldToCellPos(this.transform.position);
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                drag_offset = transform.position - moveposition;
                drag_offset.y = 0;
                //disable collision
                cld_0.gameObject.SetActive(false);
                cld_1.gameObject.SetActive(false);
                cld_2.gameObject.SetActive(false);
            }
            else if (type == BlockType.Regular)
            {
                //Make the aligned block Free
                if (myAlignedBrick != null)
                {
                    myAlignedBrick.isBlocked = false;
                }

                //Dragging Operations
                drag_start_pos = transform.position;
                //is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(drag_start_pos);
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                drag_offset = transform.position - moveposition;
                drag_offset.y = 0;

                //disable collision
                cld_0.gameObject.SetActive(false);
                cld_1.gameObject.SetActive(false);
                cld_2.gameObject.SetActive(false);
            }

        }

    }

    public void _OnMouseDrag()
    {
        mouseposition = CommonReference.mainCam.ScreenToWorldPoint(Input.mousePosition);

        float testPos = 0;
        testPos = transform.position.z + transform.position.x;

        if(!isInventory)
        {
            if (type == BlockType.Regular)
            {

                //Makes the block a little bit higher compare to other blocks
                float yOffset = drag_start_pos.y + 4f;

                // Calculate the distance to the screen to maintain the block's depth in the scene
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;

                // Convert mouse position to world position using the same depth as the block
                Vector3 moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));

                // Create a modified position vector that maintains the block's original Y position but updates X and Z
                Vector3 mpos = new Vector3(moveposition.x, yOffset, moveposition.z) + drag_offset;

                // Determine if the block is on the left side of the screen
                //bool isLeft = this.transform.position.x < LevelLoader.center.x;
                bool isLeft = IsBlockOnLeftSideOfScreen();
                // Convert the block's position to screen space
                Vector3 screenPos = CommonReference.mainCam.WorldToScreenPoint(mpos);
                Vector3 npos = transform.position;
                if (!instantiated)
                {
                    //disable collision
                    cld_0.gameObject.SetActive(false);
                    cld_1.gameObject.SetActive(false);
                    cld_2.gameObject.SetActive(false);
                    transform.position = mpos;
                }
                if(instantiated)
                {
                    //disable the collision box
                    B_blocka.cld_0.gameObject.SetActive(false);
                    B_blocka.cld_1.gameObject.SetActive(false);
                    B_blocka.cld_2.gameObject.SetActive(false);
                    B_blockb.cld_0.gameObject.SetActive(false);
                    B_blockb.cld_1.gameObject.SetActive(false);
                    B_blockb.cld_2.gameObject.SetActive(false);
                }

                if (linked)
                {
                    if (this == B_blocka) // If this is the original block
                    {


                        ClampMousePosition(ref screenPos, isLeft);
                        // Set the position of blocka
                        blocka.transform.position = mpos;

                        if (blockb != null) // Ensure blockb (the clone) is not null
                        {
                            // Position blockb with an offset, keeping in mind the isometric axis adjustments
                            //blockb.transform.position = new Vector3(mpos.x + (isLeft ? offset : -offset), mpos.y, mpos.z + (isLeft ? offsetz : -offsetz));
                            Vector3 targetPosition = new Vector3(mpos.x + (isLeft ? offset : -offset), mpos.y, mpos.z + (isLeft ? offsetz : -offsetz));

                            // Smoothly interpolate the current position to the target position
                            blockb.transform.position = Vector3.Lerp(blockb.transform.position, targetPosition, Time.deltaTime * lerpSpeed);
                        }
                    }
                    else if (this == B_blockb) // If this is the clone
                    {

                        ClampMousePosition(ref screenPos, isLeft);
                        // Set the position of blockb
                        blockb.transform.position = mpos;

                        if (blocka != null) // Ensure blocka (the original) is not null
                        {
                            // Position blocka with an offset, keeping in mind the isometric axis adjustments
                            //blocka.transform.position = new Vector3(mpos.x + (isLeft ? offset : -offset), mpos.y, mpos.z + (isLeft ? offsetz : -offsetz));
                            Vector3 targetPosition = new Vector3(mpos.x + (isLeft ? offset : -offset), mpos.y, mpos.z + (isLeft ? offsetz : -offsetz));

                            // Smoothly interpolate the current position to the target position
                            blocka.transform.position = Vector3.Lerp(blocka.transform.position, targetPosition, Time.deltaTime * lerpSpeed);
                        }
                    }
                }





            }
            else if (type == BlockType.Free)
            {
                //disable collision
                cld_0.gameObject.SetActive(false);
                cld_1.gameObject.SetActive(false);
                cld_2.gameObject.SetActive(false);
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                //Makes the block a little bit higher compare to other blocks
                float yOffset = drag_start_pos.y + 4f;

                Vector3 mpos = new Vector3(moveposition.x, yOffset, moveposition.z) + drag_offset;
                Vector3 npos = transform.position;
                bool isLeft = this.transform.position.x < LevelLoader.center.x;
                transform.position = mpos;





            }
        }
        else if (isInventory)
        {
            if (type == BlockType.Free)
            {
                //disable collision
                cld_0.gameObject.SetActive(false);
                cld_1.gameObject.SetActive(false);
                cld_2.gameObject.SetActive(false);
                //Dragging
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                //Makes the block a little bit higher compare to other blocks
                float yOffset = drag_start_pos.y + 4f;

                Vector3 mpos = new Vector3(moveposition.x, yOffset, moveposition.z) + drag_offset;
                Vector3 npos = transform.position;
                bool isLeft = this.transform.position.x < LevelLoader.center.x;
                transform.position = mpos;
            }
            else if(type == BlockType.Regular)
            {
                //disable collision
                cld_0.gameObject.SetActive(false);
                cld_1.gameObject.SetActive(false);
                cld_2.gameObject.SetActive(false);
                //Dragging
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                //Makes the block a little bit higher compare to other blocks
                float yOffset = drag_start_pos.y + 4f;

                Vector3 mpos = new Vector3(moveposition.x, yOffset, moveposition.z) + drag_offset;
                Vector3 npos = transform.position;
                bool isLeft = this.transform.position.x < LevelLoader.center.x;
                transform.position = mpos;
            }
        }





    }

    void ClampMousePosition(ref Vector3 screenPos, bool isLeft)
    {
        if (isLeft)
        {
            // Clamp the X position of the mouse to the left side of the screen
            screenPos.x = Mathf.Clamp(screenPos.x, 0, Screen.width / 2);
        }
        else
        {
            // Clamp the X position of the mouse to the right side of the screen
            screenPos.x = Mathf.Clamp(screenPos.x, Screen.width / 2, Screen.width);
        }

        // Optionally, you can also clamp the Y position if needed, for example:
        // screenPos.y = Mathf.Clamp(screenPos.y, 0, Screen.height); 
    }

    public void _OnEndDrag()
    {
        if(!isInventory)
        {
            if (type == BlockType.Regular)
            {
                if (blockb == null)
                {
                    Debug.Log("initial place");
                    cld_0.gameObject.SetActive(true);
                    cld_1.gameObject.SetActive(true);
                    cld_2.gameObject.SetActive(true);
                    cpos = LevelLoader.WorldToCellPos(this.transform.position);
                    npos = this.transform.position;

                }
                else
                {
                    Debug.Log("run COde");
                    //enable the collision box
                    B_blocka.cld_0.gameObject.SetActive(true);
                    B_blocka.cld_1.gameObject.SetActive(true);
                    B_blocka.cld_2.gameObject.SetActive(true);
                    B_blockb.cld_0.gameObject.SetActive(true);
                    B_blockb.cld_1.gameObject.SetActive(true);
                    B_blockb.cld_2.gameObject.SetActive(true);

                }


                //Map to Map Block moving

                    if (blockb != null)
                    {
                        B_blocka.BlockAlignmentMirrorInMap();
                        B_blockb.BlockAlignmentMirrorInMap();
                    }
                    else
                    {

                        if (canlink && !instantiated)
                        {
                            GameObject[] levelBricks = GameObject.FindGameObjectsWithTag("LevelBrick");
                            GameObject nearestBlock = null;
                            float nearestDistance = Mathf.Infinity;
                            foreach (GameObject levelBrick in levelBricks)
                            {
                                float distance = Vector3.Distance(transform.position, levelBrick.transform.position);
                                BlockAlignment alignment = levelBrick.GetComponent<BlockAlignment>();
                                // Check if this block is closer than the previously found ones and within the threshold
                                if (distance < nearestDistance && (!alignment.isBlocked) && distance <= 7.5f)
                                {
                                    nearestDistance = distance;
                                    nearestBlock = levelBrick;
                                }
                            }

                            // Align with the nearest block if it's within range
                            if (nearestBlock != null)
                            {
                                BlockAlignment alignment = nearestBlock.GetComponent<BlockAlignment>();
                                if (!alignment.isBlocked)
                                {

                                }
                                else
                                {
                                    inventoryManager.ItemPicked(this.gameObject);
                                    Destroy(gameObject);
                                    return;
                                }


                            }
                            else
                            {
                                inventoryManager.ItemPicked(this.gameObject);
                                Destroy(gameObject);
                                return;
                            }


                            //instantiateBlocks();
                            Debug.Log("Start Instantiation");
                        }

                        if (!instantiated)
                        {
                            instantiated = true;
                        }

                    

                        //Add Connection to the map here, make parents
                        if (instantiated)
                        {
                        float DistanceToLevelLeftBlockA = Vector3.Distance(blocka.transform.position, LevelLeft.transform.position);
                        float DistanceToLevelRightBlockA = Vector3.Distance(blocka.transform.position, LevelRight.transform.position);
                        if (DistanceToLevelLeftBlockA < DistanceToLevelRightBlockA)
                        {
                            blocka.transform.SetParent(LevelLeft.transform);
                            isBlockALeft = true;
                        }
                        else
                        {
                            blocka.transform.SetParent(LevelRight.transform);
                            isBlockALeft = false;
                        }

                        float DistanceToLevelLeftBlockB = Vector3.Distance(blockb.transform.position, LevelLeft.transform.position);
                        float DistanceToLevelRightBlockB = Vector3.Distance(blockb.transform.position, LevelRight.transform.position);
                        if (DistanceToLevelLeftBlockB < DistanceToLevelRightBlockB)
                        {
                            blockb.transform.SetParent(LevelLeft.transform);

                        }
                        else
                        {
                            blockb.transform.SetParent(LevelRight.transform);

                        }

                    }

                }

            }
            else if (type == BlockType.Free)
            {
                //This is a new methodology for free blocks to align
                //enable collision
                cld_0.gameObject.SetActive(true);
                cld_1.gameObject.SetActive(true);
                cld_2.gameObject.SetActive(true);
                Debug.Log("Dropped Free Block");

                GameObject[] levelBricks = GameObject.FindGameObjectsWithTag("LevelBrick");
                GameObject[] inventoryBricks = GameObject.FindGameObjectsWithTag("LevelBrickInventory");
                List<GameObject> allBricks = new List<GameObject>();
                allBricks.AddRange(levelBricks);
                allBricks.AddRange(inventoryBricks);
                GameObject[] combinedBricksArray = allBricks.ToArray();

                GameObject nearestBlock = null;
                float nearestDistance = Mathf.Infinity;
                foreach (GameObject levelBrick in combinedBricksArray)
                {
                    float distance = Vector3.Distance(transform.position, levelBrick.transform.position);
                    BlockAlignment alignment = levelBrick.GetComponent<BlockAlignment>();
                    // Check if this block is closer than the previously found ones and within the threshold
                    if (distance < nearestDistance && (!alignment.isBlocked))
                    {
                        nearestDistance = distance;
                        nearestBlock = levelBrick;
                    }
                }

                // Align with the nearest block if it's within range
                if (nearestBlock != null)
                {




                    BlockAlignment alignment = nearestBlock.GetComponent<BlockAlignment>();
                    if (!alignment.isBlocked)
                    {
                        myAlignedBrick = alignment;
                        if (myAlignedBrick.gameObject.tag == "LevelBrickInventory")
                        {
                            Debug.Log("InventoryBlock");
                            //This is the code that gives the transform a smooth effect
                            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                            {
                                transform.position = Vector3.Lerp(transform.position, nearestBlock.transform.position, f);
                            }, null, gameObject.GetInstanceID() + "drag_success");

                            //transform.position = nearestBlock.transform.position;
                            alignment.isBlocked = true;
                            isInventory = true;

                            //Make the inventory the parent again
                            GameObject levelInventory = GameObject.FindGameObjectWithTag("LevelInventory");
                            if (levelInventory != null)
                            {
                                transform.SetParent(levelInventory.transform);
                            }

                        }
                        else if (myAlignedBrick.gameObject.tag == "LevelBrick")
                        {
                            //This is the code that gives the transform a smooth effect
                            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                            {
                                transform.position = Vector3.Lerp(transform.position, nearestBlock.transform.position, f);
                            }, null, gameObject.GetInstanceID() + "drag_success");

                            //transform.position = nearestBlock.transform.position;
                            alignment.isBlocked = true;
                            isInventory = false;




                        }
                    }
                    else
                    {
                        SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                        {
                            transform.position = Vector3.Lerp(transform.position, myAlignedBrick.transform.position, f);
                        }, null, gameObject.GetInstanceID() + "drag_success");

                        myAlignedBrick.isBlocked = true;
                    }


                }
                else
                {
                    SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                    {
                        transform.position = Vector3.Lerp(transform.position, myAlignedBrick.transform.position, f);
                    }, null, gameObject.GetInstanceID() + "drag_success");

                    myAlignedBrick.isBlocked = true;
                }

                if (!instantiated)
                {
                    instantiated = true;
                }

                if(!isInventory)
                {
                    float DistanceToLevelLeft = Vector3.Distance(transform.position, LevelLeft.transform.position);
                    float DistanceToLevelRight = Vector3.Distance(transform.position, LevelRight.transform.position);
                    if (DistanceToLevelLeft < DistanceToLevelRight)
                    {
                        transform.SetParent(LevelLeft.transform);

                    }
                    else
                    {
                        transform.SetParent(LevelRight.transform);

                    }
                }


            }

        }
        else if (isInventory)
        {
            if (type == BlockType.Free)
            {
                //This is a new methodology for free blocks to align
                //enable collision
                cld_0.gameObject.SetActive(true);
                cld_1.gameObject.SetActive(true);
                cld_2.gameObject.SetActive(true);
                Debug.Log("Dropped Free Block");

                GameObject[] levelBricks = GameObject.FindGameObjectsWithTag("LevelBrick");
                GameObject[] inventoryBricks = GameObject.FindGameObjectsWithTag("LevelBrickInventory");
                List<GameObject> allBricks = new List<GameObject>();
                allBricks.AddRange(levelBricks);
                allBricks.AddRange(inventoryBricks);
                GameObject[] combinedBricksArray = allBricks.ToArray();

                GameObject nearestBlock = null;
                float nearestDistance = Mathf.Infinity;
                foreach (GameObject levelBrick in combinedBricksArray)
                {
                    float distance = Vector3.Distance(transform.position, levelBrick.transform.position);
                    BlockAlignment alignment = levelBrick.GetComponent<BlockAlignment>();
                    // Check if this block is closer than the previously found ones and within the threshold
                    if (distance < nearestDistance && (!alignment.isBlocked))
                    {
                        nearestDistance = distance;
                        nearestBlock = levelBrick;
                    }
                }

                // Align with the nearest block if it's within range
                if (nearestBlock != null)
                {
                    BlockAlignment alignment = nearestBlock.GetComponent<BlockAlignment>();
                    if (!alignment.isBlocked)
                    {

                        myAlignedBrick = alignment;
                        if(myAlignedBrick.gameObject.tag == "LevelBrickInventory")
                        {
                            Debug.Log("InventoryBlock");
                            //This is the code that gives the transform a smooth effect
                            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                            {
                                transform.position = Vector3.Lerp(transform.position, nearestBlock.transform.position, f);
                            }, null, gameObject.GetInstanceID() + "drag_success");

                            //transform.position = nearestBlock.transform.position;
                            alignment.isBlocked = true;
                            isInventory = true;

                            //Make the inventory the parent again
                            GameObject levelInventory = GameObject.FindGameObjectWithTag("LevelInventory");
                            if (levelInventory != null)
                            {
                                transform.SetParent(levelInventory.transform);
                            }


                        }
                        else if (myAlignedBrick.gameObject.tag == "LevelBrick")
                        {
                            //This is the code that gives the transform a smooth effect
                            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                            {
                                transform.position = Vector3.Lerp(transform.position, nearestBlock.transform.position, f);
                            }, null, gameObject.GetInstanceID() + "drag_success");

                            //transform.position = nearestBlock.transform.position;
                            alignment.isBlocked = true;
                            isInventory = false;
                        }


                    }
                    else
                    {

                        SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                        {
                            transform.position = Vector3.Lerp(transform.position, myAlignedBrick.transform.position, f);
                        }, null, gameObject.GetInstanceID() + "drag_success");
                        Debug.Log("Hello2");
                        myAlignedBrick.isBlocked = true;

                    }


                }
                else
                {
                    SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                    {
                        transform.position = Vector3.Lerp(transform.position, myAlignedBrick.transform.position, f);
                    }, null, gameObject.GetInstanceID() + "drag_success");
                    Debug.Log("Hello1");
                    myAlignedBrick.isBlocked = true;

                }

                if (!instantiated)
                {
                instantiated = true;
                }

                //Add Connection to the map here, make parents
                if(!isInventory)
                {
                    float DistanceToLevelLeft = Vector3.Distance(transform.position, LevelLeft.transform.position);
                    float DistanceToLevelRight = Vector3.Distance(transform.position, LevelRight.transform.position);
                    if (DistanceToLevelLeft < DistanceToLevelRight)
                    {
                        transform.SetParent(LevelLeft.transform);

                    }
                    else
                    {
                        transform.SetParent(LevelRight.transform);

                    }
                }

            }
            else if (type == BlockType.Regular)
            {
                //enable collision
                Debug.Log("dropped Regular Block");
                cld_0.gameObject.SetActive(true);
                cld_1.gameObject.SetActive(true);
                cld_2.gameObject.SetActive(true);

                if (canlink && !instantiated)
                {
                    GameObject[] levelBricks = GameObject.FindGameObjectsWithTag("LevelBrick");
                    GameObject[] inventoryBricks = GameObject.FindGameObjectsWithTag("LevelBrickInventory");
                    List<GameObject> allBricks = new List<GameObject>();
                    allBricks.AddRange(levelBricks);
                    allBricks.AddRange(inventoryBricks);
                    GameObject[] combinedBricksArray = allBricks.ToArray();

                    GameObject nearestBlock = null;
                    float nearestDistance = Mathf.Infinity;
                    foreach (GameObject levelBrick in combinedBricksArray)
                    {
                        float distance = Vector3.Distance(transform.position, levelBrick.transform.position);
                        BlockAlignment alignment = levelBrick.GetComponent<BlockAlignment>();
                        // Check if this block is closer than the previously found ones and within the threshold
                        if (distance < nearestDistance && (!alignment.isBlocked))
                        {
                            nearestDistance = distance;
                            nearestBlock = levelBrick;
                        }
                    }

                    // Align with the nearest block if it's within range
                    if (nearestBlock != null)
                    {
                        BlockAlignment alignment = nearestBlock.GetComponent<BlockAlignment>();
                        if (!alignment.isBlocked)
                        {


                            if (alignment.gameObject.tag == "LevelBrickInventory")
                            {
                                Debug.Log("InventoryBlock123 + Changed Aligned Block");
                                myAlignedBrick = alignment;
                                //This is the code that gives the transform a smooth effect
                                SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                                {
                                    transform.position = Vector3.Lerp(transform.position, nearestBlock.transform.position, f);
                                }, null, gameObject.GetInstanceID() + "drag_success");

                                //transform.position = nearestBlock.transform.position;
                                alignment.isBlocked = true;
                                isInventory = true;

                                //Make the inventory the parent again
                                GameObject levelInventory = GameObject.FindGameObjectWithTag("LevelInventory");
                                if (levelInventory != null)
                                {
                                    transform.SetParent(levelInventory.transform);
                                }


                            }
                            else if (alignment.gameObject.tag == "LevelBrick")
                            {

                                Debug.Log("Start Instantiation");
                                instantiateBlocks();


                            }


                        }
                        else
                        {

                            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                            {
                                transform.position = Vector3.Lerp(transform.position, myAlignedBrick.transform.position, f);
                            }, null, gameObject.GetInstanceID() + "drag_success");

                            myAlignedBrick.isBlocked = true;

                        }


                    }
                    else
                    {
                        SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                        {
                            transform.position = Vector3.Lerp(transform.position, myAlignedBrick.transform.position, f);
                        }, null, gameObject.GetInstanceID() + "drag_success");

                        myAlignedBrick.isBlocked = true;

                    }

                }

            }

        }


}

public void EndMapRotation()
{
    Vector3 Rcpos = LevelLoader.WorldToCellPos(this.transform.position);
    Vector3 Rnpos = this.transform.position;

    LevelLoader.instance.OnMoveBlock(this, LevelLoader.WorldToCellPos(rotate_start_pos), Rcpos);
    //UpdateMapCollider();
    SKUtils.StartProcedure(SKCurve.CubicIn, 0.05f, (f) =>
    {
        transform.position = Vector3.Lerp(Rnpos, Rcpos, f);


    }, null, gameObject.GetInstanceID() + "drag_success");
    Debug.Log("Rotation Started");
}

public void StartMapRotation()
{
    rotate_start_pos = this.transform.position;
    LevelLoader.instance.OnBlockRotation(LevelLoader.WorldToCellPos(rotate_start_pos));
}

public void instantiateBlocks()
{

    Debug.Log("Before test");

    GameObject a = this.gameObject;
    GameObject b = Instantiate(this.gameObject);


    bool isLeft = transform.position.x < LevelLoader.center.x;
    Vector3 targetPosition = new Vector3(transform.position.x + (isLeft ? offset : -offset), transform.position.y, transform.position.z + (isLeft ? offsetz : -offsetz));
    //this moves the clone to its position.
    b.transform.position = new Vector3(transform.position.x + (isLeft ? offset : -offset), 0, transform.position.z + (isLeft ? offsetz : -offsetz));

    blocka = a.gameObject;
    blockb = b.gameObject;
    B_blocka = this.GetComponent<Block>();
    B_blockb = b.GetComponent<Block>();
    linked = true;
    B_blockb.linked = true;
    B_blockb.B_blockb = blockb.GetComponent<Block>();
    B_blockb.B_blocka = blocka.GetComponent<Block>();
    B_blockb.blocka = blocka;
    B_blockb.blockb = blockb;
    B_blockb.offset = offset;
    B_blockb.offsetz = offsetz;
    B_blockb.instantiated = true;
    instantiated = true;
    B_blockb.cld_0 = b.transform.Find("Base")?.gameObject;
    B_blockb.cld_1 = b.transform.Find("Lower")?.gameObject;
    B_blockb.cld_2 = b.transform.Find("Upper")?.gameObject;

        //Instatiation of the new mechanics

        // Assign BreakableGround scripts to B_blocka and B_blockb
        AssignBreakableGroundScripts(B_blocka, B_blockb);

        // Assign SpawnBlock scripts to B_blocka and B_blockb
        AssignSpawnBlockScripts(B_blocka, B_blockb);
        //    B_blocka.cld_0.SetActive(true);
        //B_blockb.cld_0.SetActive(true);

        //Get the outline stuff
        //ShowMirrorGhost(b);

        B_blockb.outlineEffect = blockb.GetComponentInChildren<Outline>();
    B_blocka.BlockAlignment();
    //B_blockb.BlockAlignment();
}

    // Function to assign multiple BreakableGround scripts
    private void AssignBreakableGroundScripts(Block blockA, Block blockB)
    {
        BreakableGround[] breakableGroundScriptsA = blockA.GetComponentsInChildren<BreakableGround>();
        BreakableGround[] breakableGroundScriptsB = blockB.GetComponentsInChildren<BreakableGround>();

        if (breakableGroundScriptsA.Length > 0)
            blockA.BreakableGroundScript = breakableGroundScriptsA[0];

        if (breakableGroundScriptsA.Length > 1)
            blockA.BreakableGroundScript2 = breakableGroundScriptsA[1];

        if (breakableGroundScriptsA.Length > 2)
            blockA.BreakableGroundScript3 = breakableGroundScriptsA[2];

        if (breakableGroundScriptsB.Length > 0)
            blockB.BreakableGroundScript = breakableGroundScriptsB[0];

        if (breakableGroundScriptsB.Length > 1)
            blockB.BreakableGroundScript2 = breakableGroundScriptsB[1];

        if (breakableGroundScriptsB.Length > 2)
            blockB.BreakableGroundScript3 = breakableGroundScriptsB[2];
    }

    // Function to assign multiple SpawnBlock scripts
    private void AssignSpawnBlockScripts(Block blockA, Block blockB)
    {
        SpawnBlock[] spawnBlockScriptsA = blockA.GetComponentsInChildren<SpawnBlock>();
        SpawnBlock[] spawnBlockScriptsB = blockB.GetComponentsInChildren<SpawnBlock>();

        if (spawnBlockScriptsA.Length > 0)
            blockA.SpawnBlockScript = spawnBlockScriptsA[0];

        if (spawnBlockScriptsA.Length > 1)
            blockA.SpawnBlockScript2 = spawnBlockScriptsA[1];

        if (spawnBlockScriptsA.Length > 2)
            blockA.SpawnBlockScript3 = spawnBlockScriptsA[2];

        if (spawnBlockScriptsB.Length > 0)
            blockB.SpawnBlockScript = spawnBlockScriptsB[0];

        if (spawnBlockScriptsB.Length > 1)
            blockB.SpawnBlockScript2 = spawnBlockScriptsB[1];

        if (spawnBlockScriptsB.Length > 2)
            blockB.SpawnBlockScript3 = spawnBlockScriptsB[2];
    }







    bool IsBlockOnLeftSideOfScreen()
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        return screenPos.x < Screen.width / 2;
    }


    void CheckForDropOnUIImage()
    {
        // Create a pointer event data using the current event system
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        // Create a list to store the raycast results
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        // Perform the raycast
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        // Check if any of the raycast results is the UI Image you're interested in
        foreach (var result in raycastResults)
        {
            if(type == BlockType.Regular)
            {
                if (result.gameObject.CompareTag("BlockInventory")) // Ensure your UI Image has this tag
                {
                    Debug.Log("Dropped on the specific UI Image!");

                    LevelLoader.instance.OnBlockToInventory(LevelLoader.WorldToCellPos(BlockA_Drag_Start_Pos));
                    LevelLoader.instance.OnBlockToInventory(LevelLoader.WorldToCellPos(BlockB_Drag_Start_Pos));
                    inventoryManager.ItemPicked(blocka);
                    Instantiate(onDestroyParticle, blocka.transform.position, Quaternion.identity);
                    Instantiate(onDestroyParticle, blockb.transform.position, Quaternion.identity);
                    Destroy(blockb);
                    Destroy(blocka);
                    Destroy(this.gameObject);
                    return;
                }
            }
            else if(type == BlockType.Free)
            {
                if (result.gameObject.CompareTag("BlockInventory")) // Ensure your UI Image has this tag
                {
                    Instantiate(onDestroyParticle, this.transform.position, Quaternion.identity);
                    LevelLoader.instance.OnBlockToInventory(LevelLoader.WorldToCellPos(drag_start_pos));
                inventoryManager.ItemPicked(this.gameObject);
                return;
                }
            }


        }

        Debug.Log("Not dropped on a UI Image.");
    }

    public void PlayInit()
    {


        if (init != null && init4x4 != null)
        {
            if(levelDimensions == 3)
            {
                init?.PlayFeedbacks();
            }
            else if(levelDimensions == 4)
            {
                init4x4?.PlayFeedbacks();
            }

        }
    }

    public void FindLevelRot()
    {
        if(LevelLeft== null)
        {
            LevelLeft = GameObject.FindGameObjectWithTag("LevelLeft");
            levelrotation = LevelLeft.GetComponent<LevelRotation>();
            LevelRight = GameObject.FindGameObjectWithTag("LevelRight");
        }
        else
        {
            rotationFound = true;
            return;
        }
    }

    public void BlockAlignment()
    {

        GameObject[] levelBricksA = GameObject.FindGameObjectsWithTag("LevelBrick");
        GameObject[] levelBricksB = GameObject.FindGameObjectsWithTag("LevelBrick");
        GameObject nearestBlockA = null;
        GameObject nearestBlockB = null;
        float nearestDistanceA = Mathf.Infinity;
        float nearestDistanceB = Mathf.Infinity;


        foreach (GameObject levelBrickA in levelBricksA)
        {
            float distanceA = DistanceXZ(blocka.transform.position, levelBrickA.transform.position);

            BlockAlignment alignmentA = levelBrickA.GetComponent<BlockAlignment>();
            // Check if this block is closer than the previously found ones and within the threshold
            if (distanceA < nearestDistanceA && !alignmentA.isBlocked && distanceA <= 15f)
            {
                nearestDistanceA = distanceA;
                nearestBlockA = levelBrickA;
                Debug.Log("DistanceA is" + nearestDistanceA);
            }
        }

        foreach (GameObject levelBrickB in levelBricksB)
        {
            float distanceB = DistanceXZ(blockb.transform.position, levelBrickB.transform.position);

            BlockAlignment alignmentB = levelBrickB.GetComponent<BlockAlignment>();
            // Check if this block is closer than the previously found ones and within the threshold
            if (distanceB < nearestDistanceB && (!alignmentB.isBlocked) && distanceB <= 15f)
            {
                nearestDistanceB = distanceB;
                nearestBlockB = levelBrickB;
                Debug.Log("DistanceB is" + nearestDistanceB);
            }
        }

        if (nearestBlockA != null && nearestBlockB != null)
        {
            BlockAlignment alignmentA = nearestBlockA.GetComponent<BlockAlignment>();
            BlockAlignment alignmentB = nearestBlockB.GetComponent<BlockAlignment>();


            if (!alignmentA.isBlocked && !alignmentB.isBlocked)
            {
                //Make the inventory clear
                B_blocka.myAlignedBrick.isBlocked = false;

                B_blocka.myAlignedBrick = alignmentA;
                B_blockb.myAlignedBrick = alignmentB;
                Debug.Log("Changed Alignment");

                SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                {
                    blocka.transform.position = Vector3.Lerp(blocka.transform.position, nearestBlockA.transform.position, f);
                }, null, gameObject.GetInstanceID() + "drag_success");

                SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                {
                    blockb.transform.position = Vector3.Lerp(blockb.transform.position, nearestBlockB.transform.position, f);
                }, null, gameObject.GetInstanceID() + "drag_success");
                alignmentA.isBlocked = true;
                alignmentB.isBlocked = true;

                //Blocks are Put into the Map
                B_blocka.isInventory = false;
                B_blockb.isInventory = false;
                RegularBlockMapAlign();
                Debug.Log("Put Block");
            }
            else
            {
                Debug.Log("Testfailed");
                SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                {
                    blocka.transform.position = Vector3.Lerp(blocka.transform.position, B_blocka.myAlignedBrick.transform.position, f);
                }, null, gameObject.GetInstanceID() + "drag_success");

                B_blocka.myAlignedBrick.isBlocked = true;
                B_blocka.isInventory= true;
                B_blocka.instantiated = false;

                if (blockb != null)
                {
                    Destroy(blockb);    
                }

            }
        }
        else
        {
            Debug.Log("Testfailed2");
            Debug.Log(myAlignedBrick);
            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
            {
                blocka.transform.position = Vector3.Lerp(blocka.transform.position, B_blocka.myAlignedBrick.transform.position, f);
            }, null, gameObject.GetInstanceID() + "drag_success");

            B_blocka.myAlignedBrick.isBlocked = true;
            B_blocka.isInventory = true;
            B_blocka.instantiated = false;
            if (blockb != null)
            {
                Destroy(blockb);
            }

        }


    }

    public void BlockAlignmentMirrorInMap()
    {
        // Find all the Level and Inventory Bricks
        GameObject[] levelBricks = GameObject.FindGameObjectsWithTag("LevelBrick");
        GameObject[] inventoryBricks = GameObject.FindGameObjectsWithTag("LevelBrickInventory");

        List<GameObject> allBricks = new List<GameObject>(levelBricks);
        allBricks.AddRange(inventoryBricks);

        GameObject nearestBlockA = FindNearestBlock(blocka.transform.position, allBricks);
        GameObject nearestBlockB = FindNearestBlock(blockb.transform.position, allBricks);

        BlockAlignment alignmentA = nearestBlockA != null ? nearestBlockA.GetComponent<BlockAlignment>() : null;
        BlockAlignment alignmentB = nearestBlockB != null ? nearestBlockB.GetComponent<BlockAlignment>() : null;




        // If both blocks are found and unblocked, align them
        if (alignmentA != null && alignmentB != null && !alignmentA.isBlocked && !alignmentB.isBlocked)
        {
            // Unblock previously assigned bricks
            UnblockPreviousAlignment();

            // Assign new alignment
            B_blocka.myAlignedBrick = alignmentA;
            B_blockb.myAlignedBrick = alignmentB;
            alignmentA.isBlocked = true;
            alignmentB.isBlocked = true;

            MoveBlockToAlignment(blocka, alignmentA.transform.position);
            MoveBlockToAlignment(blockb, alignmentB.transform.position);

            B_blocka.isInventory = false;
            B_blockb.isInventory = false;
            RegularBlockMapAlign();

            Debug.Log("Blocks aligned to the map");
        }
        else
        {
            HandleSingleOrNoAlignment(alignmentA, alignmentB);
        }


    }

    private GameObject FindNearestBlock(Vector3 position, List<GameObject> blocks)
    {
        GameObject nearestBlock = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject block in blocks)
        {
            BlockAlignment alignment = block.GetComponent<BlockAlignment>();
            if (alignment == null || alignment.isBlocked) continue; // Skip blocked or invalid blocks

            float distance = Vector3.Distance(position, block.transform.position);
            if (distance < nearestDistance && distance <= 7.5f)
            {
                nearestDistance = distance;
                nearestBlock = block;
            }
        }
        return nearestBlock;
    }
    private void MoveBlockToAlignment(GameObject block, Vector3 targetPosition)
    {
        SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
        {
            block.transform.position = Vector3.Lerp(block.transform.position, targetPosition, f);
        }, null, block.GetInstanceID() + "drag_success");
    }

    private void HandleSingleOrNoAlignment(BlockAlignment alignmentA, BlockAlignment alignmentB)
    {
        if (alignmentA != null && !alignmentA.isBlocked && alignmentA.gameObject.CompareTag("LevelBrickInventory"))
        {
            MoveBlockBackToInventory(blocka, alignmentA);
            if (blockb != null) Destroy(blockb);
        }
        else if (alignmentB != null && !alignmentB.isBlocked && alignmentB.gameObject.CompareTag("LevelBrickInventory"))
        {
            MoveBlockBackToInventory(blockb, alignmentB);
            if (blocka != null) Destroy(blocka);
        }
        else
        {
            Debug.Log("No valid alignment found, keeping blocks in map");
            MoveBlockToAlignment(blocka, B_blocka.myAlignedBrick.transform.position);
            MoveBlockToAlignment(blockb, B_blockb.myAlignedBrick.transform.position);
        }
    }
    private void MoveBlockBackToInventory(GameObject block, BlockAlignment alignment)
    {
        UnblockPreviousAlignment();

        if (block == blocka)
        {


            //B_blocka.myAlignedBrick = alignment;
            //B_blocka.isInventory = true;
            //B_blocka.instantiated = false;
            //B_blockb.outlineEffect.RemakeMaterial();


            //Make the inventory clear
            B_blocka.myAlignedBrick.isBlocked = false;
            B_blockb.myAlignedBrick.isBlocked = false;
            B_blocka.myAlignedBrick = alignment;
            Debug.Log("Block A Back to Inventory");
            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
            {
                blocka.transform.position = Vector3.Lerp(blocka.transform.position, B_blocka.myAlignedBrick.transform.position, f);
            }, null, gameObject.GetInstanceID() + "drag_success");

            B_blocka.myAlignedBrick.isBlocked = true;
            B_blocka.isInventory = true;
            B_blocka.instantiated = false;
            //Make the inventory the parent again
            GameObject levelInventory = GameObject.FindGameObjectWithTag("LevelInventory");
            if (levelInventory != null)
            {
                B_blocka.transform.SetParent(levelInventory.transform);
            }
            if (blockb != null)
            {
                Destroy(blockb);
            }

        }
        else if (block == blockb)
        {
            //B_blockb.myAlignedBrick = alignment;
            //B_blockb.isInventory = true;
            //B_blockb.instantiated = false;
            //B_blockb.outlineEffect.RemakeMaterial();


            //Make the inventory clear
            B_blocka.myAlignedBrick.isBlocked = false;
            B_blockb.myAlignedBrick.isBlocked = false;
            B_blockb.myAlignedBrick = alignment;
            Debug.Log("Block B Back to Inventory");
            SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
            {
                blockb.transform.position = Vector3.Lerp(blockb.transform.position, B_blockb.myAlignedBrick.transform.position, f);
            }, null, gameObject.GetInstanceID() + "drag_success");

            B_blockb.myAlignedBrick.isBlocked = true;
            B_blockb.isInventory = true;
            B_blockb.instantiated = false;
            //Make the inventory the parent again
            GameObject levelInventory = GameObject.FindGameObjectWithTag("LevelInventory");
            if (levelInventory != null)
            {
                B_blockb.transform.SetParent(levelInventory.transform);
            }

            if (blocka != null)
            {
                Destroy(blocka);
            }
            Outline blockB_Outline = B_blockb.outlineEffect;
            blockB_Outline.RemakeMaterial();
        }
        /*
        alignment.isBlocked = true;
        MoveBlockToAlignment(block, alignment.transform.position);

        GameObject levelInventory = GameObject.FindGameObjectWithTag("LevelInventory");
        if (levelInventory != null) block.transform.SetParent(levelInventory.transform);

        Debug.Log(block.name + " moved back to inventory");
        */
        //B_blocka = null;
       // B_blockb = null;
        //blocka = null;
        //blockb= null;
        //linked = false;
    }

    private void UnblockPreviousAlignment()
    {
        if (B_blocka.myAlignedBrick != null) B_blocka.myAlignedBrick.isBlocked = false;
        if (B_blockb.myAlignedBrick != null) B_blockb.myAlignedBrick.isBlocked = false;
    }

    private void RegularBlockMapAlign()
    {
        //Add Connection to the map here, make parents
            float DistanceToLevelLeftBlockA = Vector3.Distance(blocka.transform.position, LevelLeft.transform.position);
            float DistanceToLevelRightBlockA = Vector3.Distance(blocka.transform.position, LevelRight.transform.position);
            if (DistanceToLevelLeftBlockA < DistanceToLevelRightBlockA)
            {
                blocka.transform.SetParent(LevelLeft.transform);
                isBlockALeft = true;
            }
            else
            {
                blocka.transform.SetParent(LevelRight.transform);
                isBlockALeft = false;
            }

            float DistanceToLevelLeftBlockB = Vector3.Distance(blockb.transform.position, LevelLeft.transform.position);
            float DistanceToLevelRightBlockB = Vector3.Distance(blockb.transform.position, LevelRight.transform.position);
            if (DistanceToLevelLeftBlockB < DistanceToLevelRightBlockB)
            {
                blockb.transform.SetParent(LevelLeft.transform);

            }
            else
            {
                blockb.transform.SetParent(LevelRight.transform);

            }

        
    }
    static float DistanceXZ(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x;
        float dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }


private void ShowMirrorGhost(GameObject actualCloneIfAny = null)
{
    if (!showMirrorGhostInBuild) return;

    // Ensure containers exist
    if (_mirrorGhost == null)
    {
        _mirrorGhost = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _mirrorGhost.name = $"MirrorGhost_{name}";
        _mirrorGhost.transform.localScale = Vector3.one * 2.0f; // small cube
        _mirrorGhost.GetComponent<Collider>().enabled = false;

        // make it semi-transparent
        var mr = _mirrorGhost.GetComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Standard"));
        mr.material.SetFloat("_Mode", 3f);           // Transparent
        mr.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mr.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mr.material.SetInt("_ZWrite", 0);
        mr.material.DisableKeyword("_ALPHATEST_ON");
        mr.material.EnableKeyword("_ALPHABLEND_ON");
        mr.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mr.material.renderQueue = 3000;
        mr.material.color = new Color(0f, 1f, 1f, 0.35f); // cyan, translucent
    }

    if (_mirrorLine == null)
    {
        var go = new GameObject($"MirrorLine_{name}");
        _mirrorLine = go.AddComponent<LineRenderer>();
        _mirrorLine.positionCount = 2;
        _mirrorLine.widthMultiplier = 0.03f;
        _mirrorLine.material = new Material(Shader.Find("Sprites/Default"));
        _mirrorLine.numCapVertices = 4;
        _mirrorLine.numCornerVertices = 4;
    }

    if (_mirrorLabel == null)
    {
        var labelGO = new GameObject($"MirrorLabel_{name}");
        _mirrorLabel = labelGO.AddComponent<TextMesh>();
        _mirrorLabel.fontSize = 32;
        _mirrorLabel.characterSize = 0.05f;
        _mirrorLabel.anchor = TextAnchor.LowerCenter;
        _mirrorLabel.alignment = TextAlignment.Center;
        _mirrorLabel.color = Color.white;
    }

    // Compute intended mirror position based on your current logic
    bool isLeft = transform.position.x < LevelLoader.center.x;
    Vector3 intended = new Vector3(
        transform.position.x + (isLeft ? offset : -offset),
        transform.position.y,
        transform.position.z + (isLeft ? offsetz : -offsetz)
    );

    // Place/update visuals
    _mirrorGhost.transform.position = intended;

    _mirrorLine.SetPosition(0, transform.position + Vector3.up * 0.1f);
    _mirrorLine.SetPosition(1, intended + Vector3.up * 0.1f);

    // Update label
    float distXZ = DistanceXZ(intended, transform.position);
    string text = $"INTENDED MIRROR\n" +
                  $"offset=({offset:0.00},{offsetz:0.00})\n" +
                  $"distXZ={distXZ:0.00}";

    // If we also know the actual clone pos, show delta
    if (actualCloneIfAny != null)
    {
        if (_mirrorActual == null)
        {
            _mirrorActual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _mirrorActual.name = $"MirrorActual_{name}";
            _mirrorActual.transform.localScale = Vector3.one * 1.6f;
            _mirrorActual.GetComponent<Collider>().enabled = false;

            var mrA = _mirrorActual.GetComponent<MeshRenderer>();
            mrA.material = new Material(Shader.Find("Standard"));
            mrA.material.color = new Color(1f, 0.3f, 0.3f, 0.8f); // reddish
        }
        _mirrorActual.transform.position = actualCloneIfAny.transform.position;

        float delta = Vector3.Distance(intended, actualCloneIfAny.transform.position);
        text += $"\nactual¦¤3D={delta:0.00}";
    }

    // keep label above the ghost and facing camera
    _mirrorLabel.transform.position = intended + Vector3.up * 1.6f;
    var cam = Camera.main;
    if (cam != null)
    {
        _mirrorLabel.transform.rotation = Quaternion.LookRotation(_mirrorLabel.transform.position - cam.transform.position);
    }
    _mirrorLabel.text = text;
}

// Hide/remove the ghost (optional)
private void HideMirrorGhost()
{
    if (_mirrorGhost != null) Destroy(_mirrorGhost);
    if (_mirrorActual != null) Destroy(_mirrorActual);
    if (_mirrorLine != null) Destroy(_mirrorLine.gameObject);
    if (_mirrorLabel != null) Destroy(_mirrorLabel.gameObject);

    _mirrorGhost = null;
    _mirrorActual = null;
    _mirrorLine = null;
    _mirrorLabel = null;
}

// Let you toggle in builds with a key (optional)
private void LateUpdate()
{
        /*
    if (Input.GetKeyDown(toggleMirrorGhostKey))
    {
        showMirrorGhostInBuild = !showMirrorGhostInBuild;
        if (!showMirrorGhostInBuild) HideMirrorGhost();
        else ShowMirrorGhost(blockb); // try to refresh immediately
    }

    // Keep the ghost tracking while dragging or moving
    if (showMirrorGhostInBuild && type == BlockType.Regular)
    {
        ShowMirrorGhost(blockb);
    }
        */
}

}

public enum BlockType
{
    Regular,
    Free,
    Obstacle,
    Reward,
    Start,
    End
}
