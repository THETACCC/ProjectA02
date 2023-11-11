using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartBlockSelector : MonoBehaviour
{
    public float elevation = 2;
    [SerializeField] Collider blockCld;
    [SerializeField] GameObject indicatorPrefab;

    Vector3[] subPositions = new Vector3[9];
    private void Start()
    {
        Bounds bounds = blockCld.bounds;
        Vector3 upperRight = bounds.max;
        float gap = bounds.extents.x * 2.0f / 3.0f;
        Vector3 startPos = upperRight - Vector3.one * gap / 2;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                subPositions[i * 3 + j] = startPos - new Vector3(i*gap, 0, j*gap);
                subPositions[i * 3 + j].y = bounds.max.y+ elevation;
            }
        }

        for (int i = 0;i < 9; i++)
        {
            GameObject go = GameObject.Instantiate(indicatorPrefab);
            go.transform.position = subPositions[i];    
        }
    }
}
