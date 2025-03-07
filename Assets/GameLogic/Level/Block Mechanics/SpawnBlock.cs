using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBlock : MonoBehaviour
{
    public GameObject blockToSpawn;
    public bool isSpawn = false;
    // Start is called before the first frame update
    void Start()
    {
        blockToSpawn = GetComponentInChildren<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player2")
        {
            StartSpawnBool();
        }
    }
    
    private void StartSpawnBool()
    {
        isSpawn= true;
    }

    //This will be referenced from the Block Script
    public void StartSpawn()
    {
        blockToSpawn.SetActive(true);
    }


}
