using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextControlManager : Soliton<TextControlManager>
{
    [SerializeField] private TextLang _textLang = TextLang.Eng;
    public static TextLang textLang
    {
        get { return Instance._textLang; }
        set {
            SetTextLang(value);
        }
    }

    public static event Action<TextLang> OnLanguageChange;

    public static void SetTextLang(TextLang value) {
        Instance._textLang = value;
//        Debug.Log("Test Subscribed");
        OnLanguageChange(value);
    }

    public static int GetTextLangInt() {
        switch (textLang) {
            case TextLang.Rus:
                return 0;
            case TextLang.Eng:
                return 1;
            case TextLang.Esp:
                return 2;
            case TextLang.Fra:
                return 3;
            case TextLang.Tur:
                return 4;
        }
        return 0;
    }
}
