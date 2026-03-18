using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Gestioneaza meniul de pauza: pause/resume, setari, iesire din joc
public class MenuUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameObject pauseMenu;
    [SerializeField] SettingsUI settingsUI;

    [Header("Buttons")]
    [SerializeField] Button xButton;       // Buton toggle meniu (X)
    [SerializeField] Button resumeButton;
    [SerializeField] Button exitButton;
    [SerializeField] Button settingsButton;

    private void Awake()
    {
        // Avertismente in Editor daca referintele lipsesc
        if (pauseMenu == null) Debug.LogWarning("MenuUI: pauseMenu is not set.");
        if (xButton == null) Debug.LogWarning("MenuUI: xButton is not set.");
        if (resumeButton == null) Debug.LogWarning("MenuUI: resumeButton is not set.");
        if (exitButton == null) Debug.LogWarning("MenuUI: exitButton is not set.");

        // Aboneaza butoanele la metodele corespunzatoare
        if (xButton != null) xButton.onClick.AddListener(ToggleMenu);
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (exitButton != null) exitButton.onClick.AddListener(QuitGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
    }

    private void Start()
    {
        ResumeGame(); // Asigura ca jocul porneste fara meniu deschis
    }

    // Deschide meniul de setari
    public void OpenSettings()
    {
        settingsUI?.Open();
    }

    // Afiseaza meniul de pauza si opreste timpul
    public void PauseGame()
    {
        if (pauseMenu != null) pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    // Ascunde meniul de pauza si reia timpul
    public void ResumeGame()
    {
        if (pauseMenu != null) pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    // Alterna intre pauza si resume in functie de starea curenta
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
        Application.Quit(); // Inchide aplicatia pe device
#if UNITY_EDITOR
        EditorApplication.isPlaying = false; // Opreste Play Mode in Editor
#endif
    }

    // Siguranta: reseteaza timpul daca obiectul e distrus cu meniul deschis
    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}