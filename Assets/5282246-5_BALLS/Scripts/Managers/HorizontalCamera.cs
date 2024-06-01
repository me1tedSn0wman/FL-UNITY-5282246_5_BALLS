using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class HorizontalCamera : MonoBehaviour
{
    private Camera m_camera;
    private float lastAspect;

#pragma warning disable 0649


    public float wantedHeight=1136f;
    public float wantedWidth= 640f;
    private float wantedAspect;

    [SerializeField]
    private float m_orthographicSize = 5f;
    public float OrthographicSize
    {
        get { return m_orthographicSize; }
        set
        {
            if (m_orthographicSize != value)
            {
                m_orthographicSize = value;
                RefreshCamera();
            }
        }
    }
#pragma warning restore 0649

    private void OnEnable()
    {
        wantedAspect = wantedWidth / wantedHeight;
        RefreshCamera();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= Update;
        UnityEditor.EditorApplication.update += Update;
#endif
    }

    private void Update()
    {
        wantedAspect = wantedWidth / wantedHeight;
        float aspect = m_camera.aspect;
        if (aspect != lastAspect)
            AdjustCamera(aspect);
    }

    public void RefreshCamera()
    {
        if (!m_camera)
            m_camera = GetComponent<Camera>();

        AdjustCamera(m_camera.aspect);
    }

    private void AdjustCamera(float aspect)
    {
        lastAspect = aspect;
        float _1OverAspect = 1f / aspect;

        if (aspect >= wantedAspect)
            _1OverAspect = 1f / wantedAspect;

        m_camera.orthographicSize = m_orthographicSize * _1OverAspect;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RefreshCamera();
    }

    private void OnDisable()
    {
        UnityEditor.EditorApplication.update -= Update;
    }
#endif
}