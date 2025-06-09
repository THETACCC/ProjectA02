using UnityEngine;
using UnityEngine.EventSystems;

public class MainMenuPlayerMovement : MonoBehaviour
{
    [System.Serializable]
    public class PlayerTargets
    {
        public Transform startGame;
        public Transform settings;
        public Transform credits;
        public Transform exit; // default
    }

    public GameObject player1;
    public GameObject player2;
    public PlayerTargets player1Targets;
    public PlayerTargets player2Targets;
    public float moveSpeed = 5f;
    public float rotateSpeed = 5f;

    private Transform player1Target;
    private Transform player2Target;

    private void Start()
    {
        // Start at default exit position
        player1Target = player1Targets.exit;
        player2Target = player2Targets.exit;
    }

    private void Update()
    {
        MoveAndRotate(player1, player1Target);
        MoveAndRotate(player2, player2Target);
    }

    private void MoveAndRotate(GameObject player, Transform target)
    {
        if (target == null) return;

        // Smooth position
        player.transform.position = Vector3.Lerp(
            player.transform.position,
            target.position,
            Time.deltaTime * moveSpeed
        );

        // Smooth rotation
        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            target.rotation,
            Time.deltaTime * rotateSpeed
        );
    }

    // Public methods to call from UI Button Events
    public void OnHoverStartGame()
    {
        player1Target = player1Targets.startGame;
        player2Target = player2Targets.startGame;
    }

    public void OnHoverSettings()
    {
        player1Target = player1Targets.settings;
        player2Target = player2Targets.settings;
    }

    public void OnHoverCredits()
    {
        player1Target = player1Targets.credits;
        player2Target = player2Targets.credits;
    }

    public void OnHoverExit()
    {
        player1Target = player1Targets.exit;
        player2Target = player2Targets.exit;
    }
}
