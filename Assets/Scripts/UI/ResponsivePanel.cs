using UnityEngine;

// Ataseaza pe orice panel UI pentru a-l face sa umple automat ecranul,
// indiferent de rezolutie sau Canvas Scaler mode.
[RequireComponent(typeof(RectTransform))]
public class ResponsivePanel : MonoBehaviour
{
    [Tooltip("Procentul din latimea ecranului ocupat de panou")]
    [SerializeField, Range(0.1f, 1f)] private float widthFill = 0.92f;

    [Tooltip("Procentul din inaltimea ecranului ocupat de panou")]
    [SerializeField, Range(0.1f, 1f)] private float heightFill = 0.92f;

    private RectTransform m_RT;
    private RectTransform m_CanvasRT;

    private void Awake()
    {
        m_RT = GetComponent<RectTransform>();

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null && !rootCanvas.isRootCanvas)
            rootCanvas = rootCanvas.rootCanvas;
        if (rootCanvas != null)
            m_CanvasRT = (RectTransform)rootCanvas.transform;
    }

    private void Start()                          => Fit();
    private void OnRectTransformDimensionsChange() => Fit();

    private void Fit()
    {
        float panelW = m_RT.rect.width;
        float panelH = m_RT.rect.height;
        if (panelW <= 0f || panelH <= 0f || m_CanvasRT == null) return;

        // Dimensiunile canvasului in unitati — tine cont de orice CanvasScaler
        float canvasW = m_CanvasRT.rect.width;
        float canvasH = m_CanvasRT.rect.height;
        if (canvasW <= 0f || canvasH <= 0f) return;

        float scaleX = canvasW * widthFill  / panelW;
        float scaleY = canvasH * heightFill / panelH;
        float scale  = Mathf.Min(scaleX, scaleY); // fit-inside: nu depaseste ecranul

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
