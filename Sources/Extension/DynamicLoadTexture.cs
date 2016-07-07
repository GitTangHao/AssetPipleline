using UnityEngine;
using System.Collections;

public class DynamicLoadTexture : MonoBehaviour 
{
    [SerializeField]
    ELanguage mLanguage = ELanguage.Unknown;
    [SerializeField]
    UITexture mUITexture;
    [SerializeField]
    string mTexName = string.Empty;
    [SerializeField]
    string mMaskTexName = string.Empty;
	
	void Awake ()
    {
        if (!isAvail())
        {
            mUITexture.enabled = false;
        }
        else
        {
            ResourceManager.loadAsset(mTexName + @".assetbundle", false, false);
            if (!string.IsNullOrEmpty(mMaskTexName))
            {
                ResourceManager.loadAsset(mMaskTexName + @".assetbundle", false, false);
                Material tMat = mUITexture.material;
                if (tMat == null)
                {
                    tMat = new Material(Shader.Find("Custom/TransparentColorMask"));
                    mUITexture.material = tMat;
                }
                tMat.SetTexture("_MainTex", ResourceManager.getObject<Texture>(mTexName, mTexName + @".assetbundle"));
                tMat.SetTexture("_MaskTex", ResourceManager.getObject<Texture>(mMaskTexName, mMaskTexName + @".assetbundle"));
                ResourceManager.unloadAsset(mMaskTexName + @".assetbundle", true);
            }
            else
            {
                mUITexture.mainTexture = ResourceManager.getObject<Texture>(mTexName, mTexName + @".assetbundle");
            }
            mUITexture.enabled = true;
            ResourceManager.unloadAsset(mTexName + @".assetbundle", true);
        }
	}

    bool isAvail()
    {
        if (mLanguage == ELanguage.Unknown)
        {
            return true;
        }

        ELanguage tGameLanguage = GlobleDefine.Language;

        if (mLanguage == ELanguage.SimpleChinese || mLanguage == ELanguage.TraditionalChinese)
        {
            return (tGameLanguage == ELanguage.SimpleChinese || tGameLanguage == ELanguage.TraditionalChinese);
        }

        return (mLanguage == tGameLanguage);
    }
}
