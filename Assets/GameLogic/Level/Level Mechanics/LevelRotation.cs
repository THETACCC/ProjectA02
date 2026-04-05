using SKCell;
using UnityEngine;

public class LevelRotation : MonoBehaviour
{
    private Transform self;

    public bool isRotating = false;
    public bool finishedRotation = false;

    private float smoothTime = 0.2f; // animation front and end smooth time
    private float maxSpeed = 720f; // max rotation speed

    private float targetYRotation;
    private float rotationVelocity;

    private LevelController levelController;

    private void Start()
    {
        GameObject LevelControllOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        if (LevelControllOBJ != null)
        {
            levelController = LevelControllOBJ.GetComponent<LevelController>();
        }

        self = transform;
        targetYRotation = self.eulerAngles.y;
    }

    private void Update()
    {
        if (levelController != null && levelController.phase == LevelPhase.Speaking)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            targetYRotation += 90f;
            finishedRotation = false;
        }

        RotateTowardsTarget();
    }

    private void RotateTowardsTarget()
    {
        float currentY = self.eulerAngles.y;

        float newY = Mathf.SmoothDampAngle(
            currentY,
            targetYRotation,
            ref rotationVelocity,
            smoothTime,
            maxSpeed,
            Time.deltaTime
        );

        self.eulerAngles = new Vector3(0f, newY, 0f);

        float diff = Mathf.Abs(Mathf.DeltaAngle(newY, targetYRotation));

        if (diff > 0.05f || Mathf.Abs(rotationVelocity) > 0.05f)
        {
            isRotating = true;
            finishedRotation = false;
        }
        else
        {
            self.eulerAngles = new Vector3(0f, targetYRotation, 0f);
            rotationVelocity = 0f;
            isRotating = false;
            finishedRotation = true;
        }
    }
}