using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SubPositionIndicator : MonoBehaviour
{
    private GameObject LevelControllerOBJ;
    private LevelController levelController;
    private MeshRenderer mr;
    private Camera mainCamera;
    public LayerMask interactableLayer;
    private bool isMouseOver = false;

    [Header("Tutorial")]
    public BlockTutorialManager01 tutorialManager01;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        mainCamera = Camera.main;
        mr.enabled = false;
    }

    private void Start()
    {
        LevelControllerOBJ = GameObject.FindGameObjectWithTag("LevelPhaseControll");
        levelController = LevelControllerOBJ.GetComponent<LevelController>();

        if (tutorialManager01 == null)
            tutorialManager01 = FindObjectOfType<BlockTutorialManager01>(true);
    }

    private void Update()
    {
        if ((levelController.phase == LevelPhase.Placing) && (levelController.phase != LevelPhase.Speaking))
        {
            bool wasMouseOver = isMouseOver;
            isMouseOver = false;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, interactableLayer))
            {
                Debug.DrawLine(ray.origin, hit.point, Color.red);

                if (hit.collider.gameObject == gameObject)
                {
                    isMouseOver = true;

                    if (!mr.enabled)
                        mr.enabled = true;

                    if (Input.GetMouseButtonUp(0))
                    {
                        PlayerController pc = CommonReference.playerCharacters[LevelLoader.PosToMapID(transform.position)];
                        Debug.Log(pc.name);

                        pc.transform.position = new Vector3(
                            transform.position.x,
                            pc.transform.position.y,
                            transform.position.z
                        );

                        if (tutorialManager01 != null)
                            tutorialManager01.NotifyPositionChanged();
                    }
                }
            }

            if (wasMouseOver && !isMouseOver)
            {
                mr.enabled = false;
            }

            if (!isMouseOver)
            {
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.blue);
            }
        }
        else
        {
            mr.enabled = false;
        }
    }
}