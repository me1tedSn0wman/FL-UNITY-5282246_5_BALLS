using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SafeAreaCanvasUI : MonoBehaviour
{

    private RectTransform panel;
    private Canvas canvas;
    private Rect safeArea;

    public void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeAreaSize();

        GameManager.OnScreenSizeChanged += ApplySafeAreaSize;
    }

    public void ApplySafeAreaSize(Vector2Int newScreenSize) {
        ApplySafeAreaSize();

    }

    public void ApplySafeAreaSize() {
        safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        int screenWidth = Screen.width;
        int screenHeight = Screen.height;

        if (Screen.width > 0 && Screen.height > 0) {
            anchorMin.x /= screenWidth;
            anchorMin.y /= screenHeight;
            anchorMax.x /= screenWidth;
            anchorMax.y /= screenHeight;

        }

        if (anchorMin.x >= 0 && anchorMin.y >= 0 && anchorMax.x >= 0 && anchorMax.y >= 0)
        {
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }
    }

    public void OnDestroy()
    {
        GameManager.OnScreenSizeChanged -= ApplySafeAreaSize;
    }
}
