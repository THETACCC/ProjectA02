using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIChangePlayer : MonoBehaviour
{
    public Image imageToMove; // Assign this in the inspector with your UI Image
    private Vector2 leftPosition;
    private Vector2 rightPosition;
    private float moveTime = 0f;
    public float moveDuration = 1f; // Duration of the move from one side to the other
    private bool isMoving = false;
    private bool moveToRight = false; // Flag to control direction
    private GameObject LevelControllerOBJ;
    private LevelController levelController;

    private GameObject player1;
    private PlayerCharacter character;
    void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1");
        character = player1.GetComponent<PlayerCharacter>();
        LevelControllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        levelController = LevelControllerOBJ.GetComponent<LevelController>();
        // Assuming the canvas is set to Screen Space - Overlay and matches screen size
        leftPosition = new Vector2(-480, 0); // Bottom left of the screen
        rightPosition = new Vector2(480, 0); // Bottom right of the screen
        imageToMove.rectTransform.anchoredPosition = leftPosition; // Start position
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isMoving && character.currentPlayer == Player.Player1)
        {
            // Toggle direction and start moving
            moveToRight = !moveToRight;
            isMoving = true;
            moveTime = 0f; // Reset lerp timer
        }
        else if (Input.GetKeyDown(KeyCode.G) && !isMoving && character.currentPlayer == Player.Player2)
        {
            moveToRight = !moveToRight;
            isMoving = true;
            moveTime = 0f; // Reset lerp timer
        }

            if (isMoving)
        {
            // Smoothly increment moveTime over moveDuration
            moveTime += Time.deltaTime / moveDuration;
            Vector2 startPosition = moveToRight ? leftPosition : rightPosition;
            Vector2 endPosition = moveToRight ? rightPosition : leftPosition;

            // Use SmoothStep for a smoother lerp effect
            float smoothTime = Mathf.SmoothStep(0f, 1f, moveTime);
            imageToMove.rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, smoothTime);

            if (moveTime >= 1f)
            {
                // Stop moving once the destination is reached
                isMoving = false;
                moveTime = 0f; // Reset for the next move
            }
        }
    }
}
