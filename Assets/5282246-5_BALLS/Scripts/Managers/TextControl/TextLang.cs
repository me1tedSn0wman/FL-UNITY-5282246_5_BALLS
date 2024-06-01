public enum TextLang { 
    Rus,
    Eng,
    Esp,
    Fra,
    Tur,
}

[System.Serializable]
public struct TextLineDef {
    public TextLang textLang;
    public string text;
}