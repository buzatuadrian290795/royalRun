using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject pauseMenu;

    [Header("Buttons")]
    [SerializeField] Button xButton;
    [SerializeField] Button resumeButton;
    [SerializeField] Button exitButton;

    //bool isMenuOpen;

    private void Awake()
    {
        if (pauseMenu == null) Debug.LogWarning("MenuUI: pauseMenu is not set.");
        if (xButton == null) Debug.LogWarning("MenuUI: xButton is not set.");
        if (resumeButton == null) Debug.LogWarning("MenuUI: resumeButton is not set.");
        if (exitButton == null) Debug.LogWarning("MenuUI: exitButton is not set.");

        if (xButton != null) xButton.onClick.AddListener(ToggleMenu);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(QuitGame);
    }

    private void Start()
    {
        ResumeGame();
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        //isMenuOpen = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        //isMenuOpen = false;
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