using UnityEngine;
using System.Collections;


public enum ELanguage
{
    Unknown = -1,

    SimpleChinese = 0,
    TraditionalChinese,
    English,
}


public class GlobleDefine
{
    static ELanguage sLanguage = ELanguage.Unknown;

    public static ELanguage Language
    {
        set 
        {
            switch(value)
            {
                case ELanguage.SimpleChinese:
                    PlayerPrefs.SetString(@"Language", @"SimpleChinese");
                    break;
                case ELanguage.TraditionalChinese:
                    PlayerPrefs.SetString(@"Language", @"TraditionalChinese");
                    break;
                case ELanguage.English:
                    PlayerPrefs.SetString(@"Language", @"English");
                    break;
            }
            sLanguage = value;  
        }
        get
        {
            if (sLanguage == ELanguage.Unknown)
            {
                string tLang = PlayerPrefs.GetString(@"Language", string.Empty);
                if (UnityToolKit.quickStringEqual(tLang, @"SimpleChinese"))
                {
                    sLanguage = ELanguage.SimpleChinese;
                }
                else if (UnityToolKit.quickStringEqual(tLang, @"TraditionalChinese"))
                {
                    sLanguage = ELanguage.TraditionalChinese;
                }
                else if (UnityToolKit.quickStringEqual(tLang, @"English"))
                {
                    sLanguage = ELanguage.English;
                }
            }
            return sLanguage;
        }
    }
}