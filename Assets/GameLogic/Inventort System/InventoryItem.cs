using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{

    public ItemSO itemScriptableObject;

    [SerializeField] GameObject BlockImage;
    public GameObject ActualObject;


    // Update is called once per frame

    void Start()
    {
        BlockImage = itemScriptableObject.prefab;
        ActualObject = itemScriptableObject.Actualobject;
        Instantiate(BlockImage, transform);
    }
    void Update()
    {

    }
}
