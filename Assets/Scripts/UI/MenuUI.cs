using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] SettingsUI settingsUI;

    [Header("Buttons")]
    [SerializeField] Button xButton;
    [SerializeField] Button resumeButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button settingsButton;

    private void Awake()
    {
        if (pauseMenu == null) Debug.LogWarning("MenuUI: pauseMenu is not set.");
        if (xButton == null) Debug.LogWarning("MenuUI: xButton is not set.");
        if (resumeButton == null) Debug.LogWarning("MenuUI: resumeButton is not set.");
        if (exitButton == null) Debug.LogWarning("MenuUI: exitButton is not set.");

        if (xButton != null) xButton.onClick.AddListener(ToggleMenu);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(QuitGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
    }

    private void Start()
    {
        ResumeGame();
    }

    public void OpenSettings()
    {
        if (settingsUI != null)
            settingsUI.Open();
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ToggleMenu()
    {
        if (pauseMenu != null && pauseMenu.activeSelf)
            ResumeGame();
        else
            PauseGame();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}