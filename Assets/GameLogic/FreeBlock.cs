using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKCell;
using MoreMountains.Feel;

public class FreeBlock : MonoBehaviour
{
    static float MOUSE_OVER_SCALE = 1.15f;
    public BlockType type;
    public bool draggable = true;

    public GameObject cld_0, cld_1;

    private Vector3 oScale;
    private Coroutine CR_mouseOver;

    public bool mouse_drag, is_drag_start_in_select_area;
    private bool mouse_over, prev_mouseOver;
    private Vector3 drag_offset, drag_start_pos;

    public Vector3 moveposition = Vector3.zero;


    private Transform subBlock;
    private Transform effectContainer;
    private GameObject FX_HOVER;

    private LevelController controller;

    public float rotatespeed = 10f;
    public Vector3 rotate = new Vector3(0, 0, 0);
    private bool _isRotating = false;
    public bool rotated = false;
    private Vector3 cpos;
    private Vector3 npos;
    //mouse position
    private Vector3 mouseposition;


    //Get level loader
    private LevelLoader levelLoader;


    //Get the inventory
    public GameObject inventoryManagerOBJ;
    public InventoryManager inventoryManager;

    //Get the map OBJ
    private GameObject LevelLeft;
    private GameObject LevelRight;
    private LevelRotation levelrotation;

    // Start is called before the first frame update
    void Start()
    {
        LevelLeft = GameObject.FindGameObjectWithTag("LevelLeft");
        levelrotation = LevelLeft.GetComponent<LevelRotation>();
        LevelRight = GameObject.FindGameObjectWithTag("LevelRight");


        //get the inventory manager at start
        inventoryManagerOBJ = GameObject.FindGameObjectWithTag("Inventory");
        inventoryManager = inventoryManagerOBJ.GetComponent<InventoryManager>();

        rotate = transform.rotation.eulerAngles;

        GameObject level_loader = GameObject.Find("LevelLoader");
        levelLoader = level_loader.GetComponent<LevelLoader>();


        GameObject level_controller = GameObject.Find("LevelController");
        controller = level_controller.GetComponent<LevelController>();

        cld_0 = transform.Find("CLD_0")?.gameObject;
        cld_1 = transform.Find("CLD_1")?.gameObject;
        cld_0.tag = "Wall";
        cld_1.tag = "Wall";
        subBlock = transform.Find("SubBlock");
        effectContainer = subBlock.Find("Effects");
        FX_HOVER = effectContainer.Find("FX_HOVER").gameObject;

        UpdateMapCollider();
        oScale = transform.localScale;
    }
    public void UpdateMapCollider()
    {
        int map = LevelLoader.PosToMapID(transform.position);
        if (map == 0)
        {
            cld_0.SetActive(false);
            cld_1.SetActive(true);
        }
        else
        {
            cld_0.SetActive(false);
            cld_1.SetActive(true);
        }
    }




    // Update is called once per frame
    void Update()
    {
        if (!draggable)
            return;

        if (controller.phase == LevelPhase.Placing)
        {

            UpdateMouseBehavior();
        }
        else
        {
            FX_HOVER.SetActive(false);
        }


        //This is the code for controlling the block rotation with the level rotation, to make sure that the block is rotating after the level rotated, and do not rotate while the level is rotating
        if (!levelrotation.isRotating)
        {

            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(rotate), Time.deltaTime * rotatespeed);
        }
        else
        {
        }
    }

    private void UpdateMouseBehavior()
    {

        Ray ray = CommonReference.mainCam.ScreenPointToRay(Input.mousePosition);

        Physics.Raycast(ray, out RaycastHit hit, 100000, 1 << CommonReference.LAYER_GROUND);
        mouse_over = hit.collider == null ? false : hit.collider.gameObject == this.gameObject;
        if (!prev_mouseOver && mouse_over)
        {
            _OnMouseEnter();
        }
        if (prev_mouseOver && !mouse_over)
        {
            _OnMouseExit();
        }
        if (!mouse_drag && LevelController.instance.curDraggedblock == null && mouse_over && Input.GetMouseButtonDown(0))
        {
            mouse_drag = true;
            _OnStartDrag();
            //LevelController.instance.curDraggedblock = this;
        }
        if (!mouse_drag && LevelController.instance.curDraggedblock == null && mouse_over && Input.GetMouseButtonDown(1))
        {


            rotate += new Vector3(0, 90, 0);
            rotated = true;




            //_OnEndDrag();
        }
        else
        {
            rotated = false;
        }

        if (mouse_drag)
        {



            _OnMouseDrag();
            if (Input.GetMouseButtonUp(0))
            {
                mouse_drag = false;
                _OnEndDrag();
                //LevelController.instance.curDraggedblock = null;
            }
        }
        prev_mouseOver = mouse_over;
    }


    public void SyncLocalScale(Vector3 scale)
    {
        oScale = scale;
    }

    private void _OnMouseEnter()
    {
        FX_HOVER.SetActive(true);
    }

    private void _OnMouseExit()
    {
        FX_HOVER.SetActive(false);
    }

    public void _OnStartDrag()
    {

        //These controlls the information that is instantiated for both blocks when dragging
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                drag_start_pos = transform.position;
                is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(drag_start_pos);
                //is_drag_start_in_select_area = true;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                drag_offset = transform.position - moveposition;
                drag_offset.y = 0;
    }

    public void _OnMouseDrag()
    {
        mouseposition = CommonReference.mainCam.ScreenToWorldPoint(Input.mousePosition);

        float testPos = 0;
        testPos = transform.position.z + transform.position.x;

        float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
        moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        Vector3 mpos = new Vector3(moveposition.x, transform.position.y, moveposition.z) + drag_offset;
        Vector3 npos = transform.position;
        bool isLeft = this.transform.position.x < LevelLoader.center.x;
        transform.position = mpos;

 

    }

    public void _OnEndDrag()
    {
            cpos = LevelLoader.WorldToCellPos(this.transform.position);
            npos = this.transform.position;


        if (!is_drag_start_in_select_area)
        {



            //to map
            if (!LevelLoader.IsPosInSelectionArea(npos))
            {

                if (!LevelLoader.HasBlockOnCellPos(cpos) || cpos != Vector3.one * -1)
                {

                    SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                    {
                        transform.position = Vector3.Lerp(npos, drag_start_pos, f);
       
                    }, null, gameObject.GetInstanceID() + "drag_fail");



                    Debug.Log("Drag fail");

                }
                else
                {
                    Debug.Log("Drag success");
                    UpdateMapCollider();
                    SKUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                    {
                        transform.position = Vector3.Lerp(npos, cpos, f);


                    }, null, gameObject.GetInstanceID() + "drag_success");

                }

                Vector3 nscale = transform.localScale;
                SKUtils.StopProcedure(gameObject.GetInstanceID() + "mouse_over");
                SKUtils.StartProcedure(SKCurve.QuadraticOut, 0.1f, (f) =>
                {
                    transform.localScale = Vector3.Lerp(LevelLoader.BLOCK_SCALE_MAP, nscale, f);
                }, null, gameObject.GetInstanceID() + "mouse_over");


                //Add Connection to the map here, make parents
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


}
