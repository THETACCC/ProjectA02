using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBlock : MonoBehaviour
{
    public GameObject blockToSpawn;
    public bool isSpawn = false;
    public BoxCollider myCollider;

    [Header("Protection")]
    public float minimumStayTime = 0.2f;

    private bool playerEntered = false;
    private float enterTime = 0f;

    void Start()
    {
        myCollider = GetComponent<BoxCollider>();

        if (transform.childCount > 0)
            blockToSpawn = transform.GetChild(0).gameObject;

        if (blockToSpawn != null)
            blockToSpawn.SetActive(false);
    }

    void Update()
    {
        bool shouldEnable = LevelController.instance.phase != LevelPhase.Draging;

        if (myCollider != null && myCollider.enabled != shouldEnable)
        {
            // If collider is being disabled, cancel current trigger state
            if (!shouldEnable)
            {
                playerEntered = false;
            }

            myCollider.enabled = shouldEnable;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSpawn) return;
        if (LevelController.instance.phase == LevelPhase.Draging) return;

        if (other.CompareTag("Player2"))
        {
            playerEntered = true;
            enterTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isSpawn) return;
        if (!playerEntered) return;
        if (LevelController.instance.phase == LevelPhase.Draging) return;

        if (other.CompareTag("Player2"))
        {
            float stayedLongEnough = Time.time - enterTime;

            if (stayedLongEnough >= minimumStayTime)
            {
                StartSpawnBool();
                StartSpawn();
            }

            playerEntered = false;
        }
    }

    private void StartSpawnBool()
    {
        isSpawn = true;
    }

    public void StartSpawn()
    {
        if (blockToSpawn != null)
            blockToSpawn.SetActive(true);
    }
}