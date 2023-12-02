using SKCell;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Block : MonoBehaviour
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
    public Vector3 rotate =  new Vector3 (0,0,0);
    public bool rotated = false;

    //Link blocks
    public Block linkedblock;
    public bool canlink;
    [SerializeField] private GameObject blocka;
    [SerializeField] private GameObject blockb;
    [SerializeField] private Block B_blocka;
    [SerializeField] private Block B_blockb;
    [SerializeField] private float offset;
    [SerializeField] private bool linked = false;
    private Vector3 bcpos;
    private Vector3 bnpos;
    private bool instantiated = false;
    //mouse position
    private Vector3 mouseposition;


    //Get level loader
    private LevelLoader levelLoader;
    private void Awake()
    {

    }

    private void Start()
    {
        rotate = transform.rotation.eulerAngles;
        
        canlink = true;

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

    private void Update()
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
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotate), Time.deltaTime* rotatespeed);
        //check if the block is end
        if (type == BlockType.End)
        {

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
            LevelController.instance.curDraggedblock = this;
        }
        if (!mouse_drag && LevelController.instance.curDraggedblock == null && mouse_over && Input.GetMouseButtonDown(1))
        {
            
            
                B_blocka.rotate += new Vector3(0, 90, 0);
                B_blocka.rotated = true;
                B_blockb.rotate += new Vector3(0, 90, 0);
                B_blockb.rotated = true;

            

            //_OnEndDrag();
        }else
        {
            rotated = false;
        }
        if (mouse_drag)
        {



            _OnMouseDrag();
            if (Input.GetMouseButtonUp(0))
            {
                mouse_drag = false;
                if ((blockb!= null) && (B_blockb != null))
                {
                    B_blocka._OnEndDrag();
                    if (B_blockb != null)
                    {
                        B_blockb._OnEndDrag();
                    }
                }
                else
                {
                    _OnEndDrag();
                }
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
            cld_0.SetActive(false);
            cld_1.SetActive(true);
        }
        else
        {
            cld_0.SetActive(false);
            cld_1.SetActive(true);
        }
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
        if (instantiated)
        {
            if (this == B_blocka) // If this is the original block
            {
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                drag_start_pos = transform.position;
                is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(drag_start_pos);
                //is_drag_start_in_select_area = false;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                drag_offset = transform.position - moveposition;
                drag_offset.y = 0;


                bool isLeft = blockb.transform.position.x < LevelLoader.center.x;
                B_blockb.drag_start_pos = blockb.transform.position;
                B_blockb.is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(B_blockb.drag_start_pos);
                //B_blockb.is_drag_start_in_select_area = false;
                B_blockb.moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + (isLeft ? offset : -offset), Input.mousePosition.y, distance_to_screen));
                B_blockb.drag_offset = blockb.transform.position - B_blockb.moveposition;
                B_blockb.drag_offset.y = 0;
            }
            else if (this == B_blockb)
            {
                float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
                drag_start_pos = transform.position;
                is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(drag_start_pos);
                //is_drag_start_in_select_area = false;
                moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
                drag_offset = transform.position - moveposition;
                drag_offset.y = 0;


                bool isLeft = blocka.transform.position.x < LevelLoader.center.x;
                B_blocka.drag_start_pos = blocka.transform.position;
                B_blocka.is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(B_blocka.drag_start_pos);
                //B_blocka.is_drag_start_in_select_area = false;
                B_blocka.moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x + (isLeft ? offset : -offset), Input.mousePosition.y, distance_to_screen));
                B_blocka.drag_offset = blocka.transform.position - B_blocka.moveposition;
                B_blocka.drag_offset.y = 0;
            }


        }
        else
        {
            drag_start_pos = transform.position;
            is_drag_start_in_select_area = LevelLoader.IsPosInSelectionArea(drag_start_pos);
            float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
            moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
            drag_offset = transform.position - moveposition;
            drag_offset.y = 0;
        }






    }

    public void _LinkBlock(GameObject a1, GameObject b1)
    {
        
    }

    public void _OnMouseDrag()
    {
        mouseposition = CommonReference.mainCam.ScreenToWorldPoint(Input.mousePosition);

        if (transform.position.z < -50)
        {
            Destroy(blockb);
            blockb = null;
            B_blockb = null;
            linked = false;
            instantiated = false;
            Destroy(gameObject);
        }
        float distance_to_screen = CommonReference.mainCam.WorldToScreenPoint(gameObject.transform.position).z;
        moveposition = CommonReference.mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, distance_to_screen));
        Vector3 mpos = new Vector3(moveposition.x, transform.position.y, moveposition.z) + drag_offset;
        Vector3 npos = transform.position;
        bool isLeft = this.transform.position.x < LevelLoader.center.x;
        if (!instantiated)
        {
            transform.position = mpos;
        }
        if (linked)
        {
            if (this == B_blocka) // If this is the original block
            {

                blocka.transform.position = mpos;
                if (blockb != null) // Make sure blockb (the clone) is not null
                {

                    blockb.transform.position = mpos + (isLeft ? offset : -offset) * Vector3.right; // Assuming offset is the distance between original and clone
                }
            }
            else if (this == B_blockb) // If this is the clone
            {
 

                blockb.transform.position = mpos;
                if (blocka != null) // Make sure blocka (the original) is not null
                {

                    blocka.transform.position = mpos + (isLeft ? offset : -offset) * Vector3.right; // Assuming offset is the distance between original and clone
                }
            }
        }

    }
    public void _OnEndDrag()
    {
        Vector3 cpos = LevelLoader.WorldToCellPos(this.transform.position);
        Vector3 npos = this.transform.position;
        if (linked && blockb != null)
        {
            bcpos = LevelLoader.WorldToCellPos(blockb.transform.position);
            bnpos = blockb.transform.position;
        }

        /*
        if (transform.position.z < -40)
        {
            Destroy(blockb);
            blockb = null;
            B_blockb = null;
            linked = false;
            instantiated = false;
        }
        */

        // drag from map
        if (!is_drag_start_in_select_area)
        {

            //to map
            if (!LevelLoader.IsPosInSelectionArea(npos))
            {
                if ((cpos == Vector3.one * -1 || LevelLoader.HasBlockOnCellPos(cpos)) || (bcpos == Vector3.one * -1 || LevelLoader.HasBlockOnCellPos(bcpos)))
                {
                    Debug.Log(cpos);
                    CommonUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                    {
                        transform.position = Vector3.Lerp(npos, drag_start_pos, f);
                    }, null, gameObject.GetInstanceID() + "drag_fail");
                }
                else
                {

                    LevelLoader.instance.OnMoveBlock(this, LevelLoader.WorldToCellPos(drag_start_pos), cpos);
                    UpdateMapCollider();
                    CommonUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                    {
                        transform.position = Vector3.Lerp(npos, cpos, f);

                    }, null, gameObject.GetInstanceID() + "drag_success");
                    if (canlink && !instantiated)
                    {
                        instantiateBlocks();
                    }
                }

                Vector3 nscale = transform.localScale;
                CommonUtils.StopProcedure(gameObject.GetInstanceID() + "mouse_over");
                CommonUtils.StartProcedure(SKCurve.QuadraticOut, 0.1f, (f) =>
                {
                    transform.localScale = Vector3.Lerp(LevelLoader.BLOCK_SCALE_MAP, nscale, f);
                }, null, gameObject.GetInstanceID() + "mouse_over");
            }
            // to selection
            else
            {
                if (linked && blockb != null)
                {
                    Destroy(blockb);
                    blockb = null;
                    B_blockb = null;
                    linked = false;
                    instantiated = false;
                }
                LevelLoader.instance.OnMoveBlockToSelection(this, LevelLoader.WorldToCellPos(drag_start_pos), cpos);
                LevelLoader.instance.AlignBlockSelection();
            }
        }
        // drag from selection area
        else
        {
            if (cpos == Vector3.one * -1 || LevelLoader.HasBlockOnCellPos(cpos))
            {
                Debug.Log(cpos);
                CommonUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
                {
                    transform.position = Vector3.Lerp(npos, drag_start_pos, f);
                }, null, gameObject.GetInstanceID() + "drag_fail");
            }
            else
            {
                if (canlink && !instantiated)
                {
                    instantiateBlocks();
                }

                


            }
        }
    }

    public void instantiateBlocks()
    {
        Vector3 cpos = LevelLoader.WorldToCellPos(this.transform.position);
        Vector3 npos = this.transform.position;
        //Block a = this;
        //Block b = Instantiate(this.gameObject).GetComponent<Block>();

        instantiated = true;
        GameObject a = this.gameObject;
        GameObject b = Instantiate(this.gameObject);


        Vector3 l_pos = LevelLoader.CellToWorldPos(false, 0, 0);
        Vector3 r_pos = LevelLoader.CellToWorldPos(true, 0, 0);

        float dist = l_pos.x - r_pos.x;
        bool isLeft = transform.position.x < LevelLoader.center.x;

        //this moves the clone to its position.
        b.transform.position = transform.position + (isLeft ? dist : -dist) * Vector3.right;

        //float distanceBetweenBlocks = Vector3.Distance(l_pos, r_pos);
        _LinkBlock(a, b);

        Vector3 bcpos = LevelLoader.WorldToCellPos(b.transform.position);
        Vector3 bnpos = b.transform.position;
        //Debug.Log(bnpos);
        //Debug.Log(npos);

        offset = dist;
        blocka = this.gameObject;
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
        B_blockb.instantiated = true;
        {
            LevelLoader.instance.OnMoveBlockFromSelection(this, LevelLoader.WorldToCellPos(drag_start_pos), bcpos);
            UpdateMapCollider();
            CommonUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
            {
                transform.position = Vector3.Lerp(bnpos, bcpos, f);
            }, null, gameObject.GetInstanceID() + "drag_success");

            Vector3 nscale = transform.localScale;
            CommonUtils.StopProcedure(gameObject.GetInstanceID() + "mouse_over");
            CommonUtils.StartProcedure(SKCurve.QuadraticOut, 0.1f, (f) =>
            {
                transform.localScale = Vector3.Lerp(LevelLoader.BLOCK_SCALE_MAP, nscale, f);
            }, null, gameObject.GetInstanceID() + "mouse_over");
        }


        {
            LevelLoader.instance.OnMoveBlockFromSelection(b.GetComponent<Block>(), LevelLoader.WorldToCellPos(drag_start_pos), cpos);
            UpdateMapCollider();
            CommonUtils.StartProcedure(SKCurve.CubicIn, 0.2f, (f) =>
            {
                b.transform.position = Vector3.Lerp(npos, cpos, f);
            }, null, gameObject.GetInstanceID() + "drag_success");

            Vector3 nscale = b.transform.localScale;
            CommonUtils.StopProcedure(b.gameObject.GetInstanceID() + "mouse_over");
            CommonUtils.StartProcedure(SKCurve.QuadraticOut, 0.1f, (f) =>
            {
                b.transform.localScale = Vector3.Lerp(LevelLoader.BLOCK_SCALE_MAP, nscale, f);
            }, null, b.gameObject.GetInstanceID() + "mouse_over");
        }
    }

}

public enum BlockType
{
    Regular,
    Obstacle,
    Reward,
    Start,
    End
}
