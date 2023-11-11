using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockCldGenerator : MonoBehaviour
{
    public bool[,] cld = new bool[3, 3];

    private Block block;

    public static GameObject blockRef;
    public static Vector3[] blockRefSizes = new Vector3[9];
    public static Vector3[] blockRefCenters = new Vector3[9];

    private Vector3 base_center = new Vector3(0,0.73f,0);
    private Vector3 base_size = new Vector3(11.8f, 2.46f, 11.8f);
    private void Awake()
    {
        block = GetComponent<Block>();  

        BoxCollider cld = gameObject.AddComponent<BoxCollider>();
        cld.center = base_center;
        cld.size = base_size;
    }

    public void Generate()
    {
        block = GetComponent<Block>();

        GameObject cld_0 = block.transform.Find("CLD_0").gameObject;
        GameObject cld_1 = block.transform.Find("CLD_1").gameObject;

        Collider[] colliders = cld_0.GetComponents<Collider>();
        foreach(Collider collider in colliders)
        {
            DestroyImmediate(collider);
        }
        colliders = cld_1.GetComponents<Collider>();
        foreach (Collider collider in colliders)
        {
            DestroyImmediate(collider);
        }

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject toAdd = cld[i,j]?cld_1:cld_0;
                BoxCollider bcld = toAdd.AddComponent<BoxCollider>();
                bcld.center = blockRefCenters[i * 3 + j];
                bcld.size = blockRefSizes[i * 3 + j];
            }
        }

    }
}
