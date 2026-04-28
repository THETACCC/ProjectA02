using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableGround : MonoBehaviour
{
    public bool isBreak = false;
    public GameObject myVisualObject;
    public BoxCollider myCollider;
    // Start is called before the first frame update
    void Start()
    {
        myCollider = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
       if( LevelController.instance.phase == LevelPhase.Draging )
        {
            myCollider.enabled = false;
        }
        else
        {
            myCollider.enabled = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Player1" )
        {
            //isBreak = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Player1")
        {
            Invoke(nameof(BreakAndDestroy), 1f);

        }
    }

    private void BreakAndDestroy()
    {
        if (myVisualObject != null)
        {
            myVisualObject.gameObject.SetActive(false);
        }
        isBreak = true;

        //Destroy(gameObject);
    }


}
