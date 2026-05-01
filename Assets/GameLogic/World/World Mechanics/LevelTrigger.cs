using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using SKCell;

[DisallowMultipleComponent]
public class LevelTrigger : MonoBehaviour
{
    [Header("Scene binding")]
    [Tooltip("For levels: Chapter0_Level0 / Chapter1_Level6 etc. For worlds: Chapter0World / Chapter1World etc.")]
    public SceneTitle scenetitle;

    [Header("Per-trigger UI (local)")]
    [Tooltip("Local prompt UI controlled by this trigger only. Default OFF, ON when player enters, OFF on exit.")]
    [SerializeField] private GameObject uiSelectLevel;

    [Header("Optional")]
    public GameObject spaceIndicator;

    [Header("Input")]
    [Tooltip("Let this trigger detect Space directly. This keeps scene loading working even when WorldLevelUI is hidden.")]
    [SerializeField] private bool listenSpaceDirectly = true;

    private FlowManager flowManager;
    private bool startloading = false;
    private bool playerInside = false;

    private int chapterIndex0 = 0;
    private int levelIndex0 = 0;

    private bool isWorldScene = false;

    private Collider[] _colliders;
    private Renderer[] _renderers;

    private const string PP_SceneRegistry = "JZ_SavedPosScenes";

    private void Awake()
    {
        string s = scenetitle.ToString();

        // World scene example:
        // Chapter0World / Chapter1World / Chapter2World
        isWorldScene = Regex.IsMatch(
            s,
            @"^Chapter\s*\d+\s*World$",
            RegexOptions.IgnoreCase
        );

        // Only normal level scenes need chapter/level parsing for SaveManager unlock.
        // World scenes should not be forced into Chapter#_Level# format.
        if (!isWorldScene)
        {
            var m = Regex.Match(
                s,
                @"Chapter\s*(\d+)\s*[_\-\s]\s*Level\s*(\d+)",
                RegexOptions.IgnoreCase
            );

            if (m.Success)
            {
                int chap = int.Parse(m.Groups[1].Value);
                int lvl = int.Parse(m.Groups[2].Value);

                // Scene names are already 0-based:
                // Chapter0_Level0, Chapter0_Level1...
                chapterIndex0 = Mathf.Max(0, chap);
                levelIndex0 = Mathf.Max(0, lvl);
            }
            else
            {
                Debug.LogError(
                    $"[LevelTrigger:{name}] scenetitle '{s}' is not a normal level or world scene name.",
                    this
                );

                chapterIndex0 = 0;
                levelIndex0 = 0;
            }
        }

        _colliders = GetComponentsInChildren<Collider>(true);
        _renderers = GetComponentsInChildren<Renderer>(true);

        SetSoftHidden(true);

        if (spaceIndicator) spaceIndicator.SetActive(false);
        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        GameObject fmObj = GameObject.Find("FlowManager");
        if (fmObj) flowManager = fmObj.GetComponent<FlowManager>();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitAndDecideVisibility());
    }

    private void Update()
    {
        if (!listenSpaceDirectly) return;
        if (!playerInside) return;
        if (startloading) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextLevel();
        }
    }

    private IEnumerator WaitAndDecideVisibility()
    {
        int tries = 0;

        while (SaveManager.Instance == null && tries < 10)
        {
            tries++;
            yield return null;
        }

        if (SaveManager.Instance == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        bool unlocked = true;

        // World scene triggers should always be visible if the object exists.
        // Normal level triggers still use SaveManager unlock.
        if (!isWorldScene)
        {
            unlocked = SaveManager.Instance.IsLevelUnlocked(chapterIndex0, levelIndex0);
        }

        Debug.Log(
            $"[LevelTrigger:{name}] Unlocked? {unlocked} " +
            $"(world={isWorldScene}, c={chapterIndex0}, l0={levelIndex0}, scenetitle={scenetitle})",
            this
        );

        if (!unlocked)
        {
            gameObject.SetActive(false);
            yield break;
        }

        SetSoftHidden(false);
        CheckInitialPlayerOverlap();
    }

    private void SetSoftHidden(bool hidden)
    {
        if (_colliders != null)
        {
            foreach (Collider c in _colliders)
            {
                if (c) c.enabled = !hidden;
            }
        }

        if (_renderers != null)
        {
            foreach (Renderer r in _renderers)
            {
                if (r) r.enabled = !hidden;
            }
        }
    }

    private void CheckInitialPlayerOverlap()
    {
        Collider trigger = GetComponent<Collider>();
        if (!trigger || !trigger.enabled) return;

        Collider[] hits = Physics.OverlapBox(
            trigger.bounds.center,
            trigger.bounds.extents,
            trigger.transform.rotation
        );

        foreach (Collider h in hits)
        {
            if (h.CompareTag("Player"))
            {
                playerInside = true;

                // Keep original UI behavior.
                // Do not special-case world UI here.
                if (spaceIndicator) spaceIndicator.SetActive(true);
                if (uiSelectLevel) uiSelectLevel.SetActive(true);

                if (WorldLevelUI.Instance)
                {
                    WorldLevelUI.Instance.EnterTrigger(this);
                }

                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;

        // Keep original UI behavior.
        // Any per-trigger UI assigned in inspector should still appear.
        if (spaceIndicator) spaceIndicator.SetActive(true);
        if (uiSelectLevel) uiSelectLevel.SetActive(true);

        if (WorldLevelUI.Instance)
        {
            WorldLevelUI.Instance.EnterTrigger(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;

        // Keep original UI behavior.
        if (spaceIndicator) spaceIndicator.SetActive(false);
        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        if (WorldLevelUI.Instance)
        {
            WorldLevelUI.Instance.ExitTrigger(this);
        }
    }

    private void OnDisable()
    {
        playerInside = false;

        if (spaceIndicator) spaceIndicator.SetActive(false);
        if (uiSelectLevel) uiSelectLevel.SetActive(false);

        if (WorldLevelUI.Instance)
        {
            WorldLevelUI.Instance.ExitTrigger(this);
        }
    }

    public void LoadNextLevel()
    {
        if (startloading) return;
        startloading = true;

        string currentSceneName = SceneManager.GetActiveScene().name;
        string targetSceneName = scenetitle.ToString();

        RegisterSceneForSavedPos(currentSceneName);

        // Save current scene return position.
        // Usually this is the World scene position.
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerX", transform.position.x);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerY", transform.position.y);
        PlayerPrefs.SetFloat(currentSceneName + "_LastTriggerZ", transform.position.z);

        PlayerPrefs.SetString("LastTriggerScene", currentSceneName);
        PlayerPrefs.Save();

        SKUtils.InvokeAction(0.2f, () =>
        {
            if (flowManager)
            {
                flowManager.LoadScene(new SceneInfo()
                {
                    index = scenetitle
                });
            }
            else
            {
                SceneManager.LoadScene(targetSceneName);
            }
        });
    }

    private void RegisterSceneForSavedPos(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        string raw = PlayerPrefs.GetString(PP_SceneRegistry, "");
        string token = sceneName + ";";

        if (!raw.Contains(token))
        {
            raw += token;
            PlayerPrefs.SetString(PP_SceneRegistry, raw);
        }
    }
}