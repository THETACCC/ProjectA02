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
    public bool isLevelDragging = false;


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

    //Delay player Input
    public bool startMoving = false;


    //Get the other player's situation
    public PlayerController Player1Controll;
    public PlayerController Player2Controll;

    public GroundAlignment Alignement;

    void Start()
    {
        GameObject Player1 = GameObject.FindGameObjectWithTag("Player1");
        GameObject Player2 = GameObject.FindGameObjectWithTag("Player2");
        Player1Controll= Player1.GetComponent<PlayerController>();
        Player2Controll= Player2.GetComponent<PlayerController>();


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


        if (canmove && !isLevelDragging)
        {
            if((Player1Controll.is_sliding == false) && (Player2Controll.is_sliding == false))
            {
                if (Input.GetKey(KeyCode.D))
                {

                    axis_x = 1;
                }
                else if (Input.GetKey(KeyCode.A))
                {

                    axis_x = -1;
                }
                else
                {
                    axis_x = 0;
                }
                if (Input.GetKey(KeyCode.Space))
                {

                    axis_z = 1;
                }
                else
                {
                    axis_z = 0;
                }
            }
            else
            {
                axis_x = 0;
                axis_z = 0;
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

        if(startMoving)
        {
            if (rb.velocity.magnitude > 0.05f)
            {
                controller.phase = LevelPhase.Sprinting;
            }
            else
            {
                controller.phase = LevelPhase.Running;
            }
        }





        if (controller.phase == LevelPhase.Running || controller.phase == LevelPhase.Sprinting)
        {
            if(!startMoving)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                Invoke("EnableMovement", .5f);
            }

            if(startMoving)
            {
                if (!is_sliding)
                {

                    rb.velocity = new Vector3(0, rb.velocity.y, 0);

                    if (axis_x != 0 && !isRotating)
                    {
                        StartCoroutine(RotateCharacter());
                    }

                    if (axis_z != 0 && !isRotating)
                    {
                        // Calculate the forward movement direction based on the character's current rotation
                        //Vector3 moveDirection = transform.forward * axis_z;

                        // Apply the movement speed and update the Rigidbody's velocity
                        //rb.velocity = moveDirection * moveSpeed;

                        slide_dir = new Vector2(0, 1);
                        is_sliding = true;
                        cur_sliding_time = 0;
                        cur_spd_boost = spdBoost; upperCld.gameObject.SetActive(false);
                        //upperCld.gameObject.SetActive(true);

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

                            // Move player slightly backward upon stopping
                            Vector3 moveBackDirection = -visualTF.forward; // Move back along the player's current forward direction
                            float moveBackDistance = 0.25f; // Adjust this value based on how far back you want to move

                            //rb.MovePosition(rb.position + moveBackDirection * moveBackDistance);

                            rb.velocity = new Vector3(0, rb.velocity.y, 0);

                            // Align player to the center of the colliding object
                            Alignement.AlignPlayerToCollidingObject();
                        }

                    }
                }
            }





        }


        prev_pos = rb.position;
    }

    public void PlayerSetBack()
    {
        Vector3 moveBackDirection = -visualTF.forward; // Move back along the player's current forward direction
        float moveBackDistance = 0.25f; // Adjust this value based on how far back you want to move

        rb.MovePosition(rb.position + moveBackDirection * moveBackDistance);
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

    private void EnableMovement()
    {
        startMoving = true;
    }

    IEnumerator RotateCharacter()
    {
        isRotating = true;

        float rotationSpeed = 20f; //Determines how fast the player will turn
        float targetYRotation = visualTF.eulerAngles.y + 90 * axis_x;

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
    }


}
