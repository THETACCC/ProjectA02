using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SKCell;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;

public class LevelLoader : SKMonoSingleton<LevelLoader>
{
    public static MapData mapData;
    public static Vector3 BLOCK_SCALE_MAP, BLOCK_SCALE_SELECT_AREA;
    public LevelController levelController;
    public GameObject levelControllerObj;


    GameObject[] blocks;
    public Vector2Int levelDimensions = new Vector2Int(3,3);
    [SerializeField] Transform mapLeft, mapRight;
    [SerializeField] Transform LeftSide, RightSide;
    [SerializeField] Transform blockContainerLeft, blockContainerRight;
    [SerializeField] Transform BS_Center, BS_Left, BS_Right, BS_Top;

    public Bounds boundsLeft, boundsRight;
    public Vector3[,] blockposLeft, blockposRight;
    public static Vector3 center;
    private Dictionary<Vector3, Block> blockPos = new Dictionary<Vector3, Block>(); 

    private Block endLeft, endRight, startLeft, startRight;
    private PlayerCharacter characterLeft, characterRight;

    const float BLOCK_SCALE_RATIO = 3.36f;
    const float DRAGGABLE_BLOCK_SCALE = 0.7f;
    const float DRAGGABLE_BLOCK_GAP = 12.0f;

    //This is the matrix code created for left and right map
    public Block[,] blocksLeftMap, blocksRightMap;

    //check how manay blocks are in the alignment
    private int blockindex = 0;
    public int checkindex = 0;
    private void Start()
    {
        //levelControllerObj = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        //levelController = levelControllerObj.GetComponent<LevelController>();
        levelController.InitLevel();
    }

    public void Load()
    {
        boundsLeft = mapLeft.GetComponent<BoxCollider>().bounds;
        boundsRight = mapRight.GetComponent<BoxCollider>().bounds;
        center = (LeftSide.position + RightSide.position) / 2.0f;
        Debug.Log(center);
        //center.x = (mapLeft.position.x + mapRight.position.x) / 2.0f;
        LoadMap();
        //LoadCharacter();
    }
    public void LoadMap()
    {
        //init map data
        mapData = new MapData();
        mapData.map = new int[levelDimensions.x, levelDimensions.y];

        // Initialize the matrices with the dimensions of the level
        blocksLeftMap = new Block[levelDimensions.x, levelDimensions.y];
        blocksRightMap = new Block[levelDimensions.x, levelDimensions.y];

        //calculate block position
        blockposLeft = new Vector3[levelDimensions.x,levelDimensions.y];
        blockposRight = new Vector3[levelDimensions.x,levelDimensions.y];
        //left
        float cellGapX = (boundsLeft.max.x - boundsLeft.min.x) /(2*levelDimensions.x);
        float cellGapZ = (boundsLeft.max.z - boundsLeft.min.z) /(2*levelDimensions.y);

        float startX = boundsLeft.min.x + cellGapX;
        float startZ = boundsLeft.min.z + cellGapZ;
        for (int i = 0; i < levelDimensions.y; i++)
        {
            for (int j = 0; j < levelDimensions.x; j++)
            {
                Vector3 pos = new Vector3(startX + 2 * cellGapX * j, 0, startZ + 2 * cellGapZ * i);
                blockposLeft[i, j] = pos;
            }
        }

        //right
        cellGapX = (boundsRight.max.x - boundsRight.min.x) / (2 * levelDimensions.x);
        cellGapZ = (boundsRight.max.z - boundsRight.min.z) / (2 * levelDimensions.y);

        startX = boundsRight.min.x + cellGapX;
        startZ = boundsRight.min.z + cellGapZ;
        for (int i = 0; i < levelDimensions.y; i++)
        {
            for (int j = 0; j < levelDimensions.x; j++)
            {
                Vector3 pos = new Vector3(startX + 2 * cellGapX * j, 0, startZ + 2 * cellGapZ * i);
                blockposRight[i, j] = pos;
            }
        }

        //align blocks
        blocks = GameObject.FindGameObjectsWithTag(CommonReference.TAG_BLOCK);
        BLOCK_SCALE_MAP = Vector3.one * BLOCK_SCALE_RATIO / levelDimensions.x;
        print(BLOCK_SCALE_MAP);
        BLOCK_SCALE_SELECT_AREA = Vector3.one * DRAGGABLE_BLOCK_SCALE;
        foreach (GameObject block in blocks)
        {
            Vector3 pos = block.transform.position;


            //This is disabled for now due to the bug that the block would not align
            //if(pos.z < boundsLeft.min.z || pos.z>boundsLeft.max.z || pos.x<boundsLeft.min.x || pos.x>boundsRight.max.x)
            //{
            //    continue; // out of bounds, do not align
            //}

            float min_delta = float.MaxValue;
            Vector3 best_pos = Vector3.zero;
            Vector3[,] to_comp = pos.x < center.x?blockposLeft: blockposRight;

            //to_comp = pos.z < center.z ? blockposLeft: blockposRight;
            foreach (Vector3 comp in to_comp)
            {
                float delta = Mathf.Abs(comp.x - pos.x) + Mathf.Abs(comp.z - pos.z);
                if (delta < min_delta)
                {
                    min_delta = delta;
                    best_pos = comp;
                }
            }
            block.transform.position = best_pos;
            SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, best_pos, block.GetComponent<Block>());

            //changed to zero to test
            block.transform.localScale = Vector3.zero;
            block.GetComponent<Block>().SyncLocalScale(Vector3.zero);
            //load start and end blocks
            bool isLeft = pos.x < center.x;
            Block b = block.GetComponent<Block>();
            if (b.type == BlockType.Start)
            {
                if (isLeft) startLeft = b;
                else startRight = b;
                b.draggable = false;
            }
            else if(b.type == BlockType.End)
            {
                if (isLeft) endLeft = b;
                else endRight = b;
                b.draggable = false;
            }
        }
        Debug.Log(blockposLeft[0,1]);
        //align block selection area
        AlignBlockSelection();
    }
    public void AlignBlockSelection()
    {
        Vector3[] bs_locs = GetBSLocations();
        blockindex = 0;
        foreach (GameObject block in blocks)
        {
            Vector3 pos = block.transform.position;

            if (pos.z > boundsLeft.min.z || pos.x < BS_Left.position.x || pos.x > BS_Right.position.x)
            {
                continue; // out of bounds, do not align
            }
            Vector3 fpos = block.transform.position;
            Vector3 tpos = bs_locs[blockindex];
            SKUtils.StartProcedure(SKCurve.CubicDoubleIn, 0.2f, (f) =>
            {
                block.transform.position = Vector3.Lerp(fpos, tpos, f);
            });

            block.transform.localScale = BLOCK_SCALE_SELECT_AREA;
            block.GetComponent<Block>().SyncLocalScale(BLOCK_SCALE_SELECT_AREA);
            blockindex++;
        }
        checkindex = blockindex;
    }
    public void LoadCharacter()
    {
        PlayerCharacter[] chars = GameObject.FindObjectsOfType<PlayerCharacter>();

        CommonReference.playerCharacters = chars;
        print(CommonReference.playerCharacters[0]);
        print(CommonReference.playerCharacters[1]);
        foreach (PlayerCharacter character in chars)
        {
            bool isLeft = character.transform.position.x< center.x; 
            if (isLeft)
            {
                characterLeft = character;
                character.transform.SetPositionX(startLeft.transform.position.x);
                character.transform.SetPositionZ(startLeft.transform.position.z);
                character.gameObject.layer = CommonReference.LAYER_MAP_0;
            }
            else
            {
                characterRight = character;
                character.transform.SetPositionX(startRight.transform.position.x);
                character.transform.SetPositionZ(startRight.transform.position.z);
                character.gameObject.layer = CommonReference.LAYER_MAP_1;
            }
        }
    }
    public void OnMoveBlock(Block block, Vector3 from, Vector3 to)
    {
        if (blockPos.ContainsKey(from))
        {
            blockPos.Remove(from);
        }
        SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos,to,block);



    }

    public void DeBugBlocks()
    {
        LevelLoader l = LevelLoader.instance;
        foreach (Vector3 c in l.blockPos.Keys)
        {
            print(c);
        }
    }


    public void OnBlockRotation(Vector3 from)
    {
        SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, from, null);
    }
    public void OnBlockToInventory(Vector3 from)
    {
        if (blockPos.ContainsKey(from))
        {
            blockPos.Remove(from);
        }
    }
    public void OnMoveBlockFromSelection(Block block, Vector3 from, Vector3 to)
    {
        SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, from, null);
        SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, to, block);
        
        //LevelLoader l = LevelLoader.instance;
        //foreach (Vector3 c in l.blockPos.Keys)
        //{
        //    print(c);
        //}
        
    }
    public void OnMoveBlockToSelection(Block block, Vector3 from, Vector3 to)
    {
        SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, from, null);
        //SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, to, block);
    }
    private Vector3[] GetBSLocations()
    {
        Vector3[] locations = new Vector3[9];
        locations[0] = BS_Center.position;
        for (int i=1;i<locations.Length;i++)
        {
            locations[i] = BS_Center.position+new Vector3(DRAGGABLE_BLOCK_GAP* ((i+1)/2)*(i%2==0?-1:1) , 0, 0);
        }
        return locations;
    }

    /// <summary>
    /// World position to map id (0:left, 1:right)
    /// </summary>
    public static int PosToMapID(Vector3 pos)
    {
        return pos.x < center.x ? 0 : 1;
    }
    public static bool IsPosInSelectionArea(Vector3 pos)
    {
        return pos.z < instance.BS_Top.position.z;
    }
    /// <summary>
    /// World position to cell position (in either maps). Return -1 if not found.
    /// </summary>
    /// <param name="wpos"></param>
    /// <returns></returns>
    public static Vector3 WorldToCellPos(Vector3 wpos)
    {
        LevelLoader l = LevelLoader.instance;
        //if(wpos.x<l.boundsLeft.min.x || wpos.x>l.boundsRight.max.x || wpos.z < l.boundsLeft.min.z || wpos.z > l.boundsLeft.max.z)
        //{
        //    return Vector3.one * -1;
        //}
        float min_delta = float.MaxValue;
        Vector3 best_pos = Vector3.zero;
        Vector3[,] to_comp = wpos.x < center.x ? l.blockposLeft : l.blockposRight;
        foreach (Vector3 comp in to_comp)
        {
            float delta = Mathf.Abs(comp.x - wpos.x) + Mathf.Abs(comp.z - wpos.z);
            if (delta < min_delta)
            {
                min_delta = delta;
                best_pos = comp;
            }
        }
        return best_pos;
    }

    public static Vector3 CellToWorldPos (bool isLeft, int x, int y)
    {
        LevelLoader l = LevelLoader.instance;
        return isLeft ? l.blockposLeft[x, y] : l.blockposRight[x,y];
    }


    public static bool HasBlockOnCellPos(Vector3 cpos)
    {


        LevelLoader l = LevelLoader.instance;
        
        foreach (Vector3 c in l.blockPos.Keys)
        {
            print(c);
        }

        //return l.blockPos.ContainsKey(cpos) && l.blockPos[cpos] != null;
        return l.blockPos.ContainsKey(cpos);
    }


    public void AlignBlocks()
    {
        //align blocks
        blocks = GameObject.FindGameObjectsWithTag(CommonReference.TAG_BLOCK);
        BLOCK_SCALE_MAP = Vector3.one * BLOCK_SCALE_RATIO / levelDimensions.x;
        print(BLOCK_SCALE_MAP);
        BLOCK_SCALE_SELECT_AREA = Vector3.one * DRAGGABLE_BLOCK_SCALE;
        foreach (GameObject block in blocks)
        {
            Vector3 pos = block.transform.position;


            //This is disabled for now due to the bug that the block would not align
            //if(pos.z < boundsLeft.min.z || pos.z>boundsLeft.max.z || pos.x<boundsLeft.min.x || pos.x>boundsRight.max.x)
            //{
            //    continue; // out of bounds, do not align
            //}

            float min_delta = float.MaxValue;
            Vector3 best_pos = Vector3.zero;
            Vector3[,] to_comp = pos.x < center.x ? blockposLeft : blockposRight;

            //to_comp = pos.z < center.z ? blockposLeft: blockposRight;
            foreach (Vector3 comp in to_comp)
            {
                float delta = Mathf.Abs(comp.x - pos.x) + Mathf.Abs(comp.z - pos.z);
                if (delta < min_delta)
                {
                    min_delta = delta;
                    best_pos = comp;
                }
            }
            block.transform.position = best_pos;
            SKUtils.InsertOrUpdateKeyValueInDictionary(blockPos, best_pos, block.GetComponent<Block>());

            block.transform.localScale = BLOCK_SCALE_MAP;
            block.GetComponent<Block>().SyncLocalScale(BLOCK_SCALE_MAP);
            //load start and end blocks
            bool isLeft = pos.x < center.x;
            Block b = block.GetComponent<Block>();
            if (b.type == BlockType.Start)
            {
                if (isLeft) startLeft = b;
                else startRight = b;
                b.draggable = false;
            }
            else if (b.type == BlockType.End)
            {
                if (isLeft) endLeft = b;
                else endRight = b;
                b.draggable = false;
            }
        }
    }
}

public class MapData
{
    public int[,] map;
}
public class Level
{

}
