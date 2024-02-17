using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKCell;
using UnityEngine.UIElements;


public class CharacterMovement : MonoBehaviour
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

    //Code when cannot move
    public bool canmove = true;

    private bool isRotating = false;


    public Vector3 boxSize = new Vector3(1f, 1f, 1f); // Size of the overlap box
    public float checkDistance = 3f;
    private bool collided = false;
    private bool counting = false;

    void Start()
    {
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
            if(Input.GetKey(KeyCode.D))
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

            if(Input.GetKey(KeyCode.W))
            {
                axis_z = 1;
            }
            else
            {
                axis_z = 0;
            }
            //axis_z = Input.GetAxisRaw("Vertical");
        }
        //delta_pos = rb.position - prev_pos;


        // Log the magnitude of delta_pos for debugging



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
                    upperCld.gameObject.SetActive(true);

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

                if (cur_sliding_time > .5f)
                {

                    if (delta_pos.magnitude < 0.01f)
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
        CommonUtils.StartProcedure(SKCurve.QuadraticDoubleIn, 0.2f, (f) =>
        {
            visualTF.rotation = Quaternion.Euler(Vector3.Lerp(orot, rot, f));
        });
    }
    private void OnTriggerEnterUpper(Collider cld)
    {
        //if(cld.CompareTag("Wall"))
        //is_sliding = false;

    }

    /*
    IEnumerator RotateCharacter()
    {
        isRotating = true;

        Vector3 currentRotation = visualTF.eulerAngles;
        float rotationSpeed = 10f;


        float targetYRotation = (visualTF.eulerAngles.y + 90 * axis_x);


        Debug.Log(targetYRotation);
        while (Mathf.Abs(Mathf.DeltaAngle(visualTF.eulerAngles.y, targetYRotation)) > 0.01f )
        {
            visualTF.eulerAngles = Vector3.Lerp(visualTF.eulerAngles, new Vector3(0, targetYRotation, 0), Time.deltaTime * rotationSpeed);
            //RotateTo(new Vector3(0, targetYRotation, 0));
            yield return null;
        }

        if (Mathf.Abs(Mathf.DeltaAngle(visualTF.eulerAngles.y, targetYRotation)) <= 0.01f )
        {
            visualTF.eulerAngles = new Vector3(0, targetYRotation,0);
        }


        isRotating = false;
    }
    */
    IEnumerator RotateCharacter()
    {
        isRotating = true;

        float rotationSpeed = 10f;
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
