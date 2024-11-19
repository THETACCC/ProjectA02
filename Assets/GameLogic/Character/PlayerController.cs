using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public int mapSide;
    Transform visualTF;

    public LevelController controller;


    public float moveSpeed = 1.0f;
    public float spdBoost = 5f;
    public float spdBoostDamping = 0.05f;

    SKColliderResponder upperCld;
    private Rigidbody rb;

    public bool is_sliding = false;
    private Vector2 slide_dir;

    private float cur_sliding_time = 0f;
    private float cur_spd_boost = 5f;
    private Vector3 prev_pos, delta_pos;
    private float axis_x, axis_z;
    private float mainAxis;
    //Code when cannot move
    public bool canmove = true;



    private bool isRotating = false;


    public Vector3 boxSize = new Vector3(1f, 1f, 1f); // Size of the overlap box
    public float checkDistance = 3f;
    private bool collided = false;
    private bool counting = false;


    private float targetYrotationW = 0f;
    private float targetYrotationA = 270f;
    private float targetYrotationS = 180f;
    private float targetYrotationD = 90f;

    //Animator
    public Animator playerAnimator;
    // Start is called before the first frame update
    void Start()
    {
        GameObject controllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        controller = controllerOBJ.GetComponent<LevelController>();
        rb = GetComponent<Rigidbody>();
        visualTF = transform.Find("Visual");
        upperCld = SKCldResponderManager.GetResponder("PLAYER_UPPER_CLD");
        upperCld.onTriggerEnter += OnTriggerEnterUpper;
        prev_pos = transform.position;

        mapSide = LevelLoader.PosToMapID(transform.position);
    }

    void Update()
    {




        if (canmove)
        {
            if (Input.GetKey(KeyCode.W))
            {
                mainAxis = 1;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                mainAxis = 2;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                mainAxis = 3;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                mainAxis = 4;
            }
            else
            {
                mainAxis = 0;
            }
            //axis_z = Input.GetAxisRaw("Vertical");
        }
        //delta_pos = rb.position - prev_pos;


        // Log the magnitude of delta_pos for debugging

        if (is_sliding)
        {
            playerAnimator.SetBool("isRunning", true);
        }
        else
        {
            playerAnimator.SetBool("isRunning", false);
        }

    }
    private void FixedUpdate()
    {
        CharacterManager.instance.isSliding[mapSide] = is_sliding;
        delta_pos = rb.position - prev_pos;




        if (controller.phase == LevelPhase.Running)
        {
            if (!is_sliding)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);

                if (mainAxis != 0 && !isRotating)
                {
                    StartCoroutine(RotateCharacter());
                }
                
                if (axis_z != 0 && !isRotating)
                {
                    // Calculate the forward movement direction based on the character's current rotation
                    //Vector3 moveDirection = transform.forward * axis_z;

                    // Apply the movement speed and update the Rigidbody's velocity
                    //rb.velocity = moveDirection * moveSpeed;


                    //RotateTo(new Vector3(0, (1 - (axis_z + 1) / 2) * 180, 0));
                }
                


            }
            else
            {
                cur_sliding_time += Time.deltaTime;
                if (cur_spd_boost > 1)
                {
                    cur_spd_boost -= spdBoostDamping;
                }
                counting = true;
                // Calculate the forward movement direction based on the character's current rotation
                //Vector3 moveDirection = visualTF.forward * slide_dir.y;

                // Apply the movement speed and update the Rigidbody's velocity
                //rb.velocity = moveDirection * moveSpeed * cur_spd_boost;

                rb.velocity = new Vector3(visualTF.forward.x * moveSpeed * cur_spd_boost, rb.velocity.y, visualTF.forward.z * moveSpeed * cur_spd_boost);

                if (cur_sliding_time > .25f)
                {

                    if (delta_pos.magnitude < .1f) //This determines how fast the player will consider itself stopped moving
                    {
                        Debug.Log("stop");
                        counting = false;
                        collided = false;
                        is_sliding = false;
                        rb.velocity = new Vector3(0, rb.velocity.y, 0);
                    }

                }
            }



        }


        prev_pos = rb.position;
    }

    private void RotateTo(Vector3 rot)
    {
        Vector3 orot = visualTF.rotation.eulerAngles;
        SKUtils.StartProcedure(SKCurve.QuadraticDoubleIn, 0.1f, (f) =>
        {
            visualTF.rotation = Quaternion.Euler(Vector3.Lerp(orot, rot, f));
        });
    }
    private void OnTriggerEnterUpper(Collider cld)
    {
        //if(cld.CompareTag("Wall"))
        //is_sliding = false;

    }


    IEnumerator RotateCharacter()
    {
        isRotating = true;

        float rotationSpeed = 20f; //Determines how fast the player will turn
        float targetYRotation = visualTF.eulerAngles.y;

        // Set the target rotation based on the mainAxis value
        switch (mainAxis)
        {
            case 1: // W key, no rotation needed
                targetYRotation = 0f; 
                break;

            case 2: // A key, rotate 90 degrees to the left (270 degrees)
                targetYRotation = 270f;
                break;

            case 3: // S key, rotate 180 degrees
                targetYRotation = 180f;
                break;

            case 4: // D key, rotate 90 degrees to the right
                targetYRotation = 90f;
                break;
        }

        //float targetYRotation = visualTF.eulerAngles.y + 90 * axis_x;

        // Normalize the target rotation to be within 0 to 360 degrees
        targetYRotation = (targetYRotation + 360) % 360;

        Debug.Log(targetYRotation);
        while (Mathf.Abs(Mathf.DeltaAngle(visualTF.eulerAngles.y, targetYRotation)) > 0.1f)
        {
            // Calculate the next rotation step
            float step = Time.deltaTime * rotationSpeed;
            float newYRotation = Mathf.LerpAngle(visualTF.eulerAngles.y, targetYRotation, step);

            // Apply the rotation
            visualTF.eulerAngles = new Vector3(0, newYRotation, 0);
            yield return null;
        }

        // Snap to the exact target rotation
        visualTF.eulerAngles = new Vector3(0, targetYRotation, 0);

        isRotating = false;

        slide_dir = new Vector2(0, 1);
        is_sliding = true;
        cur_sliding_time = 0;
        cur_spd_boost = spdBoost; upperCld.gameObject.SetActive(false);
        upperCld.gameObject.SetActive(true);
    }
}
