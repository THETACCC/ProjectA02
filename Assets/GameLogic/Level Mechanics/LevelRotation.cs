using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelRotation : MonoBehaviour
{

    private Transform self;

    //The Bool for the block to know whether the map is rotating or not.
    public bool isRotating = false;
    public bool finishedRotation = false;

    private LevelController levelController;
    private void Start()
    {
        GameObject LevelControllOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if (LevelControllOBJ != null)
        {
            levelController = LevelControllOBJ.GetComponent<LevelController>();
        }

        self = gameObject.transform; 
    }

    private void Update()
    {
        if(levelController.phase != LevelPhase.Speaking)
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                if (!isRotating)
                {

                    StartCoroutine(RotateLevel());
                }

            }
        }


    }


    private void RotateTo(Vector3 rot)
    {
        Vector3 orot = self.rotation.eulerAngles;
        SKUtils.StartProcedure(SKCurve.QuadraticDoubleIn, 1f, (f) =>
        {
            self.rotation = Quaternion.Euler(Vector3.Lerp(orot, rot, f));
        });
    }

    IEnumerator RotateLevel()
    {
        isRotating = true;

        float rotationSpeed = 2.5f;
        float targetYRotation = transform.eulerAngles.y + 90;

        // Normalize the target rotation to be within 0 to 360 degrees
        targetYRotation = (targetYRotation + 360) % 360;


        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetYRotation)) > 0.1f)
        {
            // Calculate the next rotation step
            float step = Time.deltaTime * rotationSpeed;
            float newYRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation, step);

            // Apply the rotation
            transform.eulerAngles = new Vector3(0, newYRotation, 0);
            yield return null;
        }

        // Snap to the exact target rotation
        transform.eulerAngles = new Vector3(0, targetYRotation, 0);
        finishedRotation = true;
        isRotating = false;
    }

}
