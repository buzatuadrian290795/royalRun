using UnityEngine;

// Gestioneaza vibratiile dispozitivului in cadrul jocului
public class VibrationManager : MonoBehaviour
{
    // Singleton: o singura instanta accesibila global din orice script
    public static VibrationManager Instance { get; private set; }

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject m_Vibrator;
#endif

    private void Awake()
    {
        // Daca exista deja o instanta, distruge duplicatul si iesi
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Pastreaza obiectul la schimbarea scenei

#if UNITY_ANDROID && !UNITY_EDITOR
        // Cacheza referinta la serviciul de vibratii o singura data
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        m_Vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#endif
    }

    // Declanseaza o vibratie de 'milliseconds' ms (implicit 50ms)
    public void Vibrate(long milliseconds = 50)
    {
        // Verifica daca vibratiile sunt activate in setari; daca nu, iesi
        if (SettingsManager.Instance != null && !SettingsManager.Instance.VibrationEnabled)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        m_Vibrator?.Call("vibrate", milliseconds);
#elif UNITY_IOS && !UNITY_EDITOR
        // Pe iOS: Unity ofera direct aceasta metoda (ignora durata, vibratie standard)
        Handheld.Vibrate();
#endif
        // Pe Editor sau alte platforme: nu se intampla nimic (cod ignorat la compilare)
    }
}