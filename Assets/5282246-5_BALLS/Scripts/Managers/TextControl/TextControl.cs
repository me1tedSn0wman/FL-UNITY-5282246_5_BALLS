using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextControl : MonoBehaviour
{
    private TextMeshProUGUI text_UI;
    [SerializeField] private TextLines textLines;

    public void Awake()
    {
        text_UI = GetComponent<TextMeshProUGUI>();
        UpdateText(TextControlManager.textLang);
        TextControlManager.OnLanguageChange += UpdateText;
    }

    public void UpdateText(TextLang textLang) {
        text_UI.text = textLines.GetTextLine(textLang);
    }

    public void OnDestroy()
    {
        TextControlManager.OnLanguageChange -= UpdateText;
    }
}
