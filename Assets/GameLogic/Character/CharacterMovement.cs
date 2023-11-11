using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SKCell;

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
        if(canmove)
        {
            axis_x = Input.GetAxisRaw("Horizontal");
            axis_z = Input.GetAxisRaw("Vertical");
        }


        CharacterManager.instance.isSliding[mapSide] = is_sliding;
    }
    private void FixedUpdate()
    {
        delta_pos = rb.position - prev_pos;
        if (controller.phase == LevelPhase.Running)
        {
            if (!CharacterManager.instance.IsSlidingEither())
            {
                rb.velocity = Vector3.zero;
                if (axis_x != 0)
                {
                    slide_dir = new Vector2(axis_x, 0);
                    is_sliding = true;
                    cur_sliding_time = 0;
                    cur_spd_boost = spdBoost;
                    upperCld.gameObject.SetActive(false);
                    upperCld.gameObject.SetActive(true);

                    RotateTo(new Vector3(0, axis_x * 90, 0));
                }
                else if (axis_z != 0)
                {
                    slide_dir = new Vector2(0, axis_z);
                    is_sliding = true;
                    cur_sliding_time = 0;
                    cur_spd_boost = spdBoost; upperCld.gameObject.SetActive(false);
                    upperCld.gameObject.SetActive(true);

                    RotateTo(new Vector3(0, (1 - (axis_z + 1) / 2) * 180, 0));
                }
            }
            else
            {
                cur_sliding_time += Time.deltaTime;
                if (cur_spd_boost > 1)
                {
                    cur_spd_boost -= spdBoostDamping;
                }
                rb.velocity = new Vector3(slide_dir.x * moveSpeed * cur_spd_boost, rb.velocity.y, slide_dir.y * moveSpeed * cur_spd_boost);
                if (cur_sliding_time > 0.5f && delta_pos.magnitude < 0.01f)
                {
                    is_sliding = false;
                    rb.velocity = Vector3.zero;
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
}
