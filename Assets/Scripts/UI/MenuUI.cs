using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject pauseMenu;

    [Header("Buttons")]
    [SerializeField] Button xButton;
    [SerializeField] Button resumeButton;
    [SerializeField] Button exitButton;

    bool isMenuOpen;

    private void Start()
    {
        if (xButton != null)
            xButton.onClick.AddListener(ToggleMenu);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(QuitGame);

        ResumeGame();
    }

    public void PauseGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(true);

        isMenuOpen = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);

        isMenuOpen = false;
        Time.timeScale = 1f;
    }

    public void ToggleMenu()
    {
        if (isMenuOpen)
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
}