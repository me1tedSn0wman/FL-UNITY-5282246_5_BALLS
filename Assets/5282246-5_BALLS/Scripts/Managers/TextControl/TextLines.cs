using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Text Line Def", menuName = "Scriptable Objects/Text Management/Text Line Def", order =1)]
public class TextLines : ScriptableObject
{
    public List<TextLineDef> textLines;

    public string GetTextLine(TextLang textLang) {
        string txt = "";

        for (int i = 0; i < textLines.Count; i++) {
            if (textLang == textLines[i].textLang) { 
                txt = textLines[i].text;
                return txt;
            }
        }
        return txt;
    }
}
