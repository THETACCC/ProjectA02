using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableIceWall : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player2" || collision.gameObject.tag == "Player1")
        {
            if(collision.gameObject.tag == "Player2")
            {
                CharacterMovement movement = collision.gameObject.GetComponent<CharacterMovement>();
                if (movement != null)
                {
                    movement.is_sliding = false;
                }
            }
            else if (collision.gameObject.tag == "Player1")
            {
                CharacterMovement movement = collision.gameObject.GetComponent<CharacterMovement>();
                if (movement != null)
                {
                    movement.is_sliding = false;
                }
            }

                Destroy(this.gameObject);
        }
    }
}
