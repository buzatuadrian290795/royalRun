using UnityEngine;

// Gestioneaza vibratiile dispozitivului in cadrul jocului
public class VibrationManager : MonoBehaviour
{
    // Singleton: o singura instanta accesibila global din orice script
    public static VibrationManager Instance { get; private set; }

    private void Awake()
    {
        // Daca exista deja o instanta, distruge duplicatul si iesi
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Pastreaza obiectul la schimbarea scenei
    }

    // Declanseaza o vibratie de 'milliseconds' ms (implicit 50ms)
    public void Vibrate(long milliseconds = 50)
    {
        // Verifica daca vibratiile sunt activate in setari; daca nu, iesi
        if (SettingsManager.Instance != null && !SettingsManager.Instance.VibrationEnabled)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
        // Pe Android: acceseaza serviciul de vibratii prin Java nativ
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        vibrator.Call("vibrate", milliseconds); // Porneste vibratia
#elif UNITY_IOS && !UNITY_EDITOR
        // Pe iOS: Unity ofera direct aceasta metoda (ignora durata, vibratie standard)
        Handheld.Vibrate();
#endif
        // Pe Editor sau alte platforme: nu se intampla nimic (cod ignorat la compilare)
    }
}