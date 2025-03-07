using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableGround : MonoBehaviour
{
    public bool isBreak = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        isBreak = true;
        //Destroy(gameObject);
    }


}
