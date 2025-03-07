using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentRotation : MonoBehaviour
{
    private Transform self;

    //The Bool for the block to know whether the map is rotating or not.
    public bool isRotating = false;
    public bool finishedRotation = false;
    private void Start()
    {
        self = gameObject.transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (!isRotating)
            {
                float RandomRot = Random.Range(1, 3);
                Debug.Log(RandomRot);
                if(RandomRot == 1)
                {

                    StartCoroutine(RotateLevel());
                }
                else if(RandomRot == 2)
                {
                    StartCoroutine(RotateLevelZ());
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
        //targetYRotation = (targetYRotation + 360) % 360;


        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetYRotation)) > 0.1f)
        {
            // Calculate the next rotation step
            float step = Time.deltaTime * rotationSpeed;
            float newYRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetYRotation, step);

            // Apply the rotation
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, newYRotation, transform.eulerAngles.z);
            yield return null;
        }

        // Snap to the exact target rotation
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, targetYRotation, transform.eulerAngles.z);
        finishedRotation = true;
        isRotating = false;
    }

    IEnumerator RotateLevelX()
    {
        isRotating = true;

        float rotationSpeed = 2.5f;
        float targetYRotation = transform.localPosition.x + 100;

        // Normalize the target rotation to be within 0 to 360 degrees
        //targetYRotation = (targetYRotation + 360) % 360;


        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.x, targetYRotation)) > 0.1f)
        {
            // Calculate the next rotation step
            float step = Time.deltaTime * rotationSpeed;
            float newYRotation = Mathf.LerpAngle(transform.eulerAngles.x, targetYRotation, step);

            // Apply the rotation
            transform.eulerAngles = new Vector3(newYRotation, transform.eulerAngles.y, transform.eulerAngles.z);
            yield return null;
        }

        // Snap to the exact target rotation
        transform.eulerAngles = new Vector3(targetYRotation, transform.eulerAngles.y, transform.eulerAngles.z);
        finishedRotation = true;
        isRotating = false;
    }

    IEnumerator RotateLevelZ()
    {
        isRotating = true;

        float rotationSpeed = 2.5f;
        float targetYRotation = transform.eulerAngles.z + 90;

        // Normalize the target rotation to be within 0 to 360 degrees
        //targetYRotation = (targetYRotation + 360) % 360;


        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.z, targetYRotation)) > 0.1f)
        {
            // Calculate the next rotation step
            float step = Time.deltaTime * rotationSpeed;
            float newYRotation = Mathf.LerpAngle(transform.eulerAngles.z, targetYRotation, step);

            // Apply the rotation
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, newYRotation);
            yield return null;
        }

        // Snap to the exact target rotation
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, targetYRotation);
        finishedRotation = true;
        isRotating = false;
    }
}
