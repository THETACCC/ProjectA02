using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{

    public ItemSO itemScriptableObject;

    [SerializeField] GameObject BlockImage;



    // Update is called once per frame

    void Start()
    {
        BlockImage = itemScriptableObject.prefab;
        Instantiate(BlockImage, transform);
    }
    void Update()
    {

    }
}
