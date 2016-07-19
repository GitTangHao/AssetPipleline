using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Net;
using System.Security.Cryptography;


public enum EAssetFamily
{
    None = -1,
    Common = 0,
    UIBaseCommon,
    UIAtlasCommon,
    UICommon,
    UIMainUI,
    GamePlayCommon,
    Enemy,
    Player,
    Bullet,
}

public enum EAppUpdateInfo
{
    Latest = 0,

    AppUpdate = 1,
    DllUpdate = 2,
    ResourceUpdate = 4,

    Invalid = 8, //local version newer than server??
}

public enum EResourceErrorCode
{
    None = 0,

    ServerOff,
    AppNeedUpdate,
    LoadServerStatueError,
    LoadServerAssetInfoError,
    DownloadAssetError,
}

public class AssetInfo
{
    public string mName;
    public string mMD5;
    //public string mExtention;
    public int mVersion;
    public long mSize;

    public bool mLoaded;
    public bool mLoadAll;

    public AssetBundle mAssetBundle;
    public Hashtable mObjectsTable = new Hashtable();

    public AssetInfo( string a_name, string a_md5, int a_version, long a_size )
    {
        mName = a_name;
        mMD5 = a_md5;
        mVersion = a_version;
        mSize = a_size;
        mLoaded = mLoadAll = false;
    }
}

public class AssetFamily
{
    public EAssetFamily mFamily;
    public string[] mAssets;

    public AssetFamily( EAssetFamily a_family, string[] a_Assets )
    {
        mFamily = a_family;
        mAssets = a_Assets;
    }
}

public class ServerSatueInfo
{
    public bool mIsOn;
    public string mServerNotice;
    public string mAppUrl;
    public string mAssetsUrl;
};


public delegate void ResourceErrorDelegate(EResourceErrorCode a_code, object a_info);

public class ResourceManager 
{
    static string sServeStatueInfoUrl = @"http://192.168.0.2/aop/server/ServerStatueInfo.xml";// init with default value when no configure file found.

    static readonly string sPersistentAssetURL =
#if UNITY_EDITOR
    "file:///" + Application.persistentDataPath + @"/";
#else
    #if UNITY_ANDROID || UNITY_IPHONE
        "file://" + Application.persistentDataPath + @"/";
    #else    
        "file:///" + Application.persistentDataPath + @"/";
    #endif
#endif

    static readonly string sStreamAssetURL =
#if UNITY_EDITOR
    "file://" + Application.dataPath + "/StreamingAssets/";
#else
    #if UNITY_ANDROID
        "jar:file://" + Application.dataPath + "!/assets/";
    #elif UNITY_IPHONE
	    "file://" + Application.dataPath+"/Raw/";
    #else
        "file://" + Application.dataPath + "/StreamingAssets/";
    #endif
#endif

    static readonly string sLocalMd5URL = Application.persistentDataPath + @"/AssetBundleMd5List.xml";

    static List<AssetFamily> mAssetFamilyList = new List<AssetFamily>()
    {
        new AssetFamily(EAssetFamily.Common, new string[] { "config.assetbundle","table.assetbundle", "vfx.assetbundle"}),

        new AssetFamily(EAssetFamily.UIBaseCommon, new string[] { "UICommonScripts.assetbundle", "UIFont.assetbundle", "UICommonAnimation.assetbundle"}),

        new AssetFamily(EAssetFamily.UIAtlasCommon, new string[] { "UIAtlasBG.assetbundle", "UIAtlasButton.assetbundle", "UIAtlasFrame.assetbundle",
                                                                   "UIAtlasHeadicon.assetbundle", "UIAtlasItemicon.assetbundle", "UIAtlasSkillicon.assetbundle"}),

        new AssetFamily(EAssetFamily.UICommon, new string[] { "UICommon.assetbundle"}),

        new AssetFamily(EAssetFamily.UIMainUI, new string[] { "UIBag.assetbundle", "UIBattle.assetbundle", "UIChat.assetbundle", "UIFriend.assetbundle", 
                                                              "UILogin.assetbundle", "UIMecha.assetbundle", "UIMore.assetbundle", "UIPilot.assetbundle",
                                                              "UIQuestAchieve.assetbundle", "UIShop.assetbundle", "UITeam.assetbundle", "UIPVP.assetbundle"}),

        new AssetFamily(EAssetFamily.GamePlayCommon, new string[] { "Buff.assetbundle", "crystal.assetbundle", "Items.assetbundle", "PlayerSkill.assetbundle", 
                                                                    "PlayerWeaponAndBullet.assetbundle", "UIGameCommon.assetbundle" }),

        new AssetFamily(EAssetFamily.Enemy, new string[] { "ApeKing.assetbundle", "ApeSoldier.assetbundle", "CannonKing.assetbundle", "CannonSoldier.assetbundle", 
                                                           "DogKing.assetbundle", "DogSoldier.assetbundle", "HumanKing.assetbundle", "HumanSoldier.assetbundle", "ThunderKing.assetbundle", 
                                                           "WallKing.assetbundle", "WallSoldier.assetbundle", "ThunderSoldier.assetbundle","TemplarKing.assetbundle",
                                                           "TemplarSoldier.assetbundle","GuardKing.assetbundle","SlimeSoldier.assetbundle"}),

        new AssetFamily(EAssetFamily.Player, new string[] { "rbt_Comet.assetbundle", "rbt_Eagle.assetbundle", "rbt_Saturn.assetbundle", "rbt_Shadow.assetbundle", "rbt_Ranger.assetbundle","rbt_Cute.assetbundle" }),


        new AssetFamily(EAssetFamily.Bullet, new string[] { "ApeKing.assetbundle", "ApeSoldier.assetbundle", "CannonKing.assetbundle", "CannonSoldier.assetbundle", 
                                                            "DogKing.assetbundle", "DogSoldier.assetbundle", "FishKing.assetbundle", "HumanKing.assetbundle", 
                                                            "HumanSoldier.assetbundle", "ThunderKing.assetbundle", "WallKing.assetbundle", "WallSoldier.assetbundle",
                                                            "ThunderSoldier.assetbundle", "PlayerWeaponAndBullet.assetbundle","SlimeSoldier.assetbundle"}),
    };

    struct AssetStatue
    {
        public int mVersion;
        public long mSize;
        public string mMd5;

        public AssetStatue( string a_md5, long a_size, int a_version)
        {
            mMd5 = a_md5;
            mSize = a_size;
            mVersion = a_version;
        }
    }

    public delegate void onAssetEventHandle(string a_eventInfo);
    public static event onAssetEventHandle onAssetLoadedEvent = null;
    public static event onAssetEventHandle onControlingAssetEvent = null;

    static Dictionary<string, AssetStatue> sLocalAssetStatues = new Dictionary<string, AssetStatue>();
    static Dictionary<string, AssetStatue> sServerAssetStatues = new Dictionary<string, AssetStatue>();

    public static ResourceErrorDelegate onErrorDelegate;

    static ServerSatueInfo sServerStatue;

    static string sResPublicVersion = "0.0.0";  //resource folder.
    static string sPerPublicVersion = "0.0.0";  //persistent data folder.
    static string sSerPublicVersion = "0.0.0";  //server version.

    static Hashtable sAssetInfoTable = new Hashtable();

    static bool sInited = false;

    static long sTotalBytesToDownload = 0;
    static long sTotalBytesDownloaded = 0;
    static WebException sDownloadException = null;
    static MD5CryptoServiceProvider sMd5Provider = new MD5CryptoServiceProvider();

    static XmlDocument sLocalMd5Doc;
    static XmlDocument sInfoText;

#if UNITY_ANDROID
    static AndroidJavaClass sAndroidJavaClass;
    static AndroidJavaObject sAndroidJavaObject;
#endif

    public static string GetAppVersion()
    {
        return sPerPublicVersion;
    }

    public static void Init()
    {
        if (!sInited)
        {
#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                sAndroidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                sAndroidJavaObject = sAndroidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
            }
#endif
            readServerUrl();
            cleanUpCachedBundles();
            loadLocalAssetInfos();
            sInited = true;
        }
    }

    public static string getInfoText(string a_key )
    {
        if (sInfoText == null)
        {
            TextAsset tTextAsset = null;
            if (GlobleDefine.Language == ELanguage.English)
            {
                tTextAsset = Resources.Load<TextAsset>("InfoTextEn");
            }
			else if(GlobleDefine.Language == ELanguage.TraditionalChinese)
			{
				tTextAsset = Resources.Load<TextAsset>("InfoTextHK");
			}
            else
            {
                tTextAsset = Resources.Load<TextAsset>("InfoTextCn");
            }
            sInfoText = new XmlDocument();
            sInfoText.LoadXml(tTextAsset.text);
        }
        XmlNode tNode = sInfoText.FirstChild.SelectSingleNode(a_key);
        if (tNode == null)
        {
            return a_key + " was not found in info text!";
        }
        return tNode.Attributes["value"].Value;
    }

    public static void cleanUpCachedBundles()
    {
        LogInfo(getInfoText("cleanUp"));
        string[] tCachedBundles = Directory.GetFiles(Application.temporaryCachePath + @"/", "*_decompres*", SearchOption.TopDirectoryOnly);
        foreach (string tCachedBundle in tCachedBundles)
        {
            File.Delete(tCachedBundle);
        }
    }

    public static ServerSatueInfo getServerStatue()
    {
        return sServerStatue;
    }

    public static IEnumerator checkServerStatue()
    {
        LogInfo(getInfoText("checkServerStatue"));
        Debug.Log(sServeStatueInfoUrl);
        WWW www = new WWW(sServeStatueInfoUrl);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.LogError("check server statue error " + www.error);

            if (onErrorDelegate != null)
            {
                onErrorDelegate(EResourceErrorCode.LoadServerStatueError, getInfoText("checkServerStatueError") + www.error);
            }
            yield break;
        }
        
        byte[] tXmlBytes = System.Text.Encoding.Default.GetBytes(www.text);
        Stream tXmlStream = new MemoryStream(tXmlBytes);
        XmlDocument tDoc = new XmlDocument();
        tDoc.Load(tXmlStream);
        tXmlStream.Close();

        sServerStatue = new ServerSatueInfo();
        sServerStatue.mIsOn = bool.Parse(tDoc.DocumentElement.GetAttribute("IsServerOn"));
        sServerStatue.mServerNotice = tDoc.DocumentElement.GetAttribute("ServerNotice");
        sServerStatue.mAppUrl = tDoc.DocumentElement.GetAttribute("AppUrl");

        switch(GlobleDefine.Language)
        {
            case ELanguage.SimpleChinese:
                sServerStatue.mAssetsUrl = tDoc.DocumentElement.GetAttribute("AssetsCNUrl");
                break;
            case ELanguage.TraditionalChinese:
                sServerStatue.mAssetsUrl = tDoc.DocumentElement.GetAttribute("AssetsHKUrl");
                break;
            case ELanguage.English:
                sServerStatue.mAssetsUrl = tDoc.DocumentElement.GetAttribute("AssetsENUrl");
                break;
            default:
                Debug.LogError("Invalid language " + GlobleDefine.Language);
                yield break;
        }

        if (!sServerStatue.mIsOn)
        {
            LogInfo(getInfoText("serverOff"));
            if (onErrorDelegate != null)
            {
                if (string.IsNullOrEmpty(sServerStatue.mServerNotice))
                {
                    onErrorDelegate(EResourceErrorCode.ServerOff, getInfoText("serverOff"));
                }
                else
                {
                    onErrorDelegate(EResourceErrorCode.ServerOff, getInfoText("serverOff") + sServerStatue.mServerNotice);
                }
            }
            yield break;
        }
    }

    static void readServerUrl()
    {
        TextAsset tAssetServer = Resources.Load<TextAsset>("AssetServer");
        if (tAssetServer != null)
        {
            XmlDocument tDoc = new XmlDocument();
            tDoc.LoadXml(tAssetServer.text);
            sServeStatueInfoUrl = tDoc.DocumentElement.SelectSingleNode("url").Attributes["value"].Value;
        }
    }

    static void loadLocalAssetInfos()
    {
        LogInfo(getInfoText("loadlocalReslst"));

#if UNITY_EDITOR
		string tAssetMd5Tex = File.ReadAllText (Application.dataPath+"/StreamingAssets/" + @"AssetBundleMd5List.xml");
#else
#if UNITY_IPHONE
		string tAssetMd5Tex = File.ReadAllText (Application.dataPath+"/Raw/" + @"AssetBundleMd5List.xml");
#elif UNITY_ANDROID
		byte[] tAssetBytes = sAndroidJavaObject.Call<byte[]>("readJarFile", "AssetBundleMd5List.xml");
		string tAssetMd5Tex = System.Text.Encoding.UTF8.GetString(tAssetBytes);
#else
		string tAssetMd5Tex = File.ReadAllText (Application.dataPath+"/StreamingAssets/" + @"AssetBundleMd5List.xml"); // TODO:Set path on other platforms.
#endif
#endif

        if ( string.IsNullOrEmpty(tAssetMd5Tex) )
		{
            Debug.LogError("load local resource list error! ");
            return;
        }

        bool tIsMd5Exist = File.Exists(sLocalMd5URL);
#if UNITY_STANDALONE
        string tPerXmlContent = File.ReadAllText(Application.streamingAssetsPath + @"/AssetBundleMd5List.xml");
        if (!tIsMd5Exist)
        {
			File.WriteAllText(sLocalMd5URL, tAssetMd5Tex);  //in case a new user
        }
		sLocalMd5Doc = new XmlDocument();
		sLocalMd5Doc.LoadXml(tPerXmlContent);
#else
        if (!tIsMd5Exist)
        {
			File.WriteAllText(sLocalMd5URL, tAssetMd5Tex);  //in case a new user
            string tPerXmlContent = File.ReadAllText(sLocalMd5URL);
            sLocalMd5Doc = new XmlDocument();
            sLocalMd5Doc.LoadXml(tPerXmlContent);
        }
        else
        {
            string tPerXmlContent = File.ReadAllText(sLocalMd5URL);
            sLocalMd5Doc = new XmlDocument();
            sLocalMd5Doc.LoadXml(tPerXmlContent);
            sPerPublicVersion = sLocalMd5Doc.DocumentElement.GetAttribute("publishversion");
			updateLocalAssetInfos(tAssetMd5Tex);    //in case app update
        }
#endif
        sPerPublicVersion = sLocalMd5Doc.DocumentElement.GetAttribute("publishversion");

        foreach (XmlElement tXeGroup in sLocalMd5Doc.FirstChild.ChildNodes)
        {
            foreach (XmlElement tXe in tXeGroup.ChildNodes)
            {
                string tAssetName = tXe.GetAttribute("name");
                int tAssetVersion = int.Parse(tXe.GetAttribute("version"));
                long tSize = long.Parse(tXe.GetAttribute("size"));
                string tMD5 = tXe.GetAttribute("md5");
                sLocalAssetStatues[tAssetName] = new AssetStatue(tMD5, tSize, tAssetVersion);
                sAssetInfoTable.Add(tAssetName, new AssetInfo(tAssetName, tMD5, tAssetVersion, tSize));
            }
        }
    }

    static void updateLocalAssetInfos( string a_resContent )
    {
        LogInfo(getInfoText("updatelocalReslst"));
        XmlDocument tResDoc = new XmlDocument();
        tResDoc.LoadXml(a_resContent);
        sResPublicVersion = tResDoc.DocumentElement.GetAttribute("publishversion");

        EAppUpdateInfo tAppUpdateInfo = getAppUpdateInfo(sPerPublicVersion, sResPublicVersion);

        if (tAppUpdateInfo != EAppUpdateInfo.Invalid && tAppUpdateInfo != EAppUpdateInfo.Latest)
        {
            sLocalMd5Doc.DocumentElement.SetAttribute("publishversion", sResPublicVersion);
            sLocalMd5Doc.Save(sLocalMd5URL); // do not change excuting order!!!
            foreach (XmlElement tXeGroup in tResDoc.FirstChild.ChildNodes)
            {
                foreach (XmlElement tXe in tXeGroup.ChildNodes)
                {
                    string tAssetName = tXe.GetAttribute("name");
                    int tAssetVersion = int.Parse(tXe.GetAttribute("version"));
                    long tSize = long.Parse(tXe.GetAttribute("size"));
                    string tMD5 = tXe.GetAttribute("md5");
                    deleteAsset(tAssetName);    //delete old app version asset.
                    updateNewAssetInfo(tAssetName, tMD5, tAssetVersion, tSize);
                }
            }
        }
    }

    static void updateNewAssetInfo( string a_assetName, string a_md5, int a_version, long a_size )
    {
        bool tFinishUpdated = false;
        foreach (XmlElement tXeGroup in sLocalMd5Doc.FirstChild.ChildNodes)
        {
            foreach (XmlElement tXe in tXeGroup.ChildNodes)
            {
                string tAssetName = tXe.GetAttribute("name");
                if (tAssetName.Length == a_assetName.Length && string.Equals(tAssetName, a_assetName))
                {
                    tXe.SetAttribute("md5", a_md5);
                    tXe.SetAttribute("size", a_size.ToString());
                    tXe.SetAttribute("version", a_version.ToString());
                    tFinishUpdated = true;
                    break;
                }
            }
            if (tFinishUpdated)
            {
                break;
            }
        }
        if (!tFinishUpdated)
        {
            bool tIsAssetBundle = a_assetName.EndsWith(@".assetbundle"); // asset bundle or unity3d ?
            XmlElement tXE = sLocalMd5Doc.CreateElement(tIsAssetBundle ? "assetbundle" : "unity3d");
            tXE.SetAttribute("name", a_assetName);
            tXE.SetAttribute("md5", a_md5);
            tXE.SetAttribute("size", a_size.ToString());
            tXE.SetAttribute("version", a_version.ToString());
            sLocalMd5Doc.FirstChild.ChildNodes[tIsAssetBundle ? 0 : 1].AppendChild(tXE);
        }
        sLocalMd5Doc.Save(sLocalMd5URL);
        sLocalAssetStatues[a_assetName] = new AssetStatue(a_md5, a_size, a_version);
    }

    static void removeAssetInfo( string a_assetName )
    {
        string tXmlContent = System.IO.File.ReadAllText(sLocalMd5URL);
        XmlDocument tDoc = new XmlDocument();
        tDoc.LoadXml(tXmlContent);

        XmlElement tChildToRemove = null;
        foreach (XmlElement tXeGroup in tDoc.FirstChild.ChildNodes)
        {
            foreach (XmlElement tXe in tXeGroup.ChildNodes)
            {
                string tAssetName = tXe.GetAttribute("name");
                if (tAssetName.Length == a_assetName.Length && string.Equals(tAssetName, a_assetName))
                {
                    tChildToRemove = tXe;
                    break;
                }
            }
            if (tChildToRemove != null)
            {
                tXeGroup.RemoveChild(tChildToRemove);
                break;
            }
        }
        tDoc.Save(sLocalMd5URL);
    }

    public static IEnumerator loadServerAssetInfo()
    {
        LogInfo(getInfoText("loadServerList"));
        WWW www = new WWW(sServerStatue.mAssetsUrl + @"AssetBundleMd5List.xml");
        yield return www;
        if( !string.IsNullOrEmpty(www.error) )
        {
            Debug.LogError("load server resource list error " + www.error);

            if (onErrorDelegate != null)
            {
                onErrorDelegate(EResourceErrorCode.LoadServerAssetInfoError, getInfoText("loadServerReslstErr") + www.error);
            }
            yield break;
        }

        byte[] tXmlBytes = System.Text.Encoding.Default.GetBytes(www.text);
        Stream tXmlStream = new MemoryStream(tXmlBytes);
        XmlDocument tDoc = new XmlDocument();
        tDoc.Load(tXmlStream);
        tXmlStream.Close();

        sSerPublicVersion = tDoc.DocumentElement.GetAttribute("publishversion");
        EAppUpdateInfo tAppUpdateInfo = getAppUpdateInfo(sPerPublicVersion, sSerPublicVersion);

        if (tAppUpdateInfo == EAppUpdateInfo.Invalid)
        {
            LogInfo(getInfoText("invalidVersion") + sPerPublicVersion);
            if (onErrorDelegate != null)
            {
                onErrorDelegate(EResourceErrorCode.LoadServerAssetInfoError, getInfoText("invalidVersion") + sPerPublicVersion);
            }
            yield break;
        }
        if ( tAppUpdateInfo == EAppUpdateInfo.AppUpdate)
        {
            LogInfo(getInfoText("appUpdate"));
            if (onErrorDelegate != null)
            {
                onErrorDelegate(EResourceErrorCode.AppNeedUpdate, ResourceManager.getInfoText("askForUpgrade") + " (" + sSerPublicVersion + ")");
            }
            yield break;
        }
        if ( tAppUpdateInfo == EAppUpdateInfo.DllUpdate )
        {
            LogInfo(getInfoText("dllUpdate"));
            if (onErrorDelegate != null)
            {
                onErrorDelegate(EResourceErrorCode.AppNeedUpdate, ResourceManager.getInfoText("askForUpgrade") + " (" + sSerPublicVersion + ")");
            }
            yield break;
        }
        LogInfo(getInfoText("updateInfolst"));
        updateServerAssetInfos(tDoc);
#if UNITY_ANDROID || UNITY_IPHONE
        validateLocalAssets();
#endif
    }

    static void updateServerAssetInfos(XmlDocument a_doc)
    {
        sServerAssetStatues.Clear();
        foreach (XmlElement tXeGroup in a_doc.FirstChild.ChildNodes)
        {
            foreach (XmlElement tXe in tXeGroup.ChildNodes)
            {
                string tAssetName = tXe.GetAttribute("name");
                string tMD5 = tXe.GetAttribute("md5");
                int tAssetVersion = int.Parse(tXe.GetAttribute("version"));
                long tSize = long.Parse(tXe.GetAttribute("size"));
                sServerAssetStatues.Add(tAssetName, new AssetStatue(tMD5, tSize, tAssetVersion));

                if (sAssetInfoTable.Contains(tAssetName))
                {
                    AssetInfo tInfo = (AssetInfo)sAssetInfoTable[tAssetName];
                    if (tAssetVersion != tInfo.mVersion || !string.Equals(tInfo.mMD5, tMD5))
                    {
                        tInfo.mMD5 = tMD5;
                        tInfo.mVersion = tAssetVersion;
                        tInfo.mSize = tSize;
                    }
                }
                else
                {
                    sAssetInfoTable.Add(tAssetName, new AssetInfo(tAssetName, tMD5, tAssetVersion, tSize));
                }
            }
        }
    }

    static void validateLocalAssets()
    {
        List<string> tUnusedAssets = new List<string>();

        foreach ( KeyValuePair<string, AssetStatue> tPair in sLocalAssetStatues)
        {
            if (!sServerAssetStatues.ContainsKey(tPair.Key))
            {
                tUnusedAssets.Add(tPair.Key);
            }
        }

        foreach (string tAsset in tUnusedAssets)
        {
            deleteAsset(tAsset);    //delete asset unused..
            if (sAssetInfoTable.Contains(tAsset))
            {
                sAssetInfoTable.Remove(tAsset);
            }
        }

        foreach ( KeyValuePair<string, AssetStatue> tPair in sServerAssetStatues)
        {
            AssetStatue tAssetStatue;
            if (sLocalAssetStatues.TryGetValue(tPair.Key, out tAssetStatue))
            {
                if (!UnityToolKit.quickStringEqual(tPair.Value.mMd5, tAssetStatue.mMd5))
                {
                    deleteAsset(tPair.Key);    //delete asset invalid..
                }
            }
        }
    }

    public static bool IsServerAssetInfoLoaded()
    {
        return sServerAssetStatues.Count != 0;
    }

    public static long getDownloadAssetsLength()
    {
        long tLength = 0;
        foreach (KeyValuePair<string, AssetStatue> tPair in sServerAssetStatues)
        {
            if (!isAssetDownloaded(tPair.Key))
            {
                tLength += tPair.Value.mSize;
            }
        }
        return tLength;
    }

    public static void downloadAssets(MonoBehaviour a_caller)
    {
        sTotalBytesToDownload = 0;
        sTotalBytesDownloaded = 0;
        
        List<string> tAssetNames = new List<string>();
        List<string> tAssetMd5s = new List<string>();
        List<int> tAssetVersions = new List<int>();

        foreach (KeyValuePair<string, AssetStatue> tPair in sServerAssetStatues)
        {
            if (!isAssetDownloaded(tPair.Key))
            {
                sTotalBytesToDownload += tPair.Value.mSize;
                tAssetNames.Add(tPair.Key);
                tAssetMd5s.Add(tPair.Value.mMd5);
                tAssetVersions.Add(tPair.Value.mVersion);
            }
        }

        if (tAssetNames.Count != 0)
        {
            a_caller.StartCoroutine(downloadAssets(tAssetNames.ToArray(), tAssetMd5s.ToArray(), tAssetVersions.ToArray()));
        }
    }

    static IEnumerator downloadAssets( string[] a_assetName, string[] a_md5, int[] a_version )
    {
        TimeoutWebClient tWebClient = new TimeoutWebClient(20000);
        tWebClient.DownloadDataCompleted += downloadDataCompletedHandler;
        tWebClient.DownloadProgressChanged += downloadProgressChangedHandler;

        for (int i = 0, imax = a_assetName.Length; i < imax; i++ )
        {
            LogInfo(getInfoText("download") + a_assetName[i]);

            string tUrl = sServerStatue.mAssetsUrl + a_assetName[i];

            FileStream tFileStream = new FileStream(Application.persistentDataPath + @"/tmp_" + a_assetName[i], FileMode.Create, FileAccess.ReadWrite, FileShare.Read | FileShare.Delete);
            tWebClient.DownloadDataAsync(new System.Uri(tUrl), tFileStream);

            while (tFileStream.Length <= 0)
            {
                if (sDownloadException != null)
                {
                    tFileStream.Close();
                    tWebClient.CancelAsync();
                    if (onErrorDelegate != null)
                    {
                        onErrorDelegate(EResourceErrorCode.DownloadAssetError, getInfoText("downloadAssetError") + sDownloadException.Message);
                    }
                    sDownloadException = null;
                    yield break;
                }
                yield return null;
            }
            tFileStream.Flush();
            long tAssetSize = tFileStream.Length;
            tFileStream.Close();

            File.Delete(Application.persistentDataPath + @"/" + a_assetName[i]);
            File.Move(Application.persistentDataPath + @"/tmp_" + a_assetName[i], Application.persistentDataPath + @"/" + a_assetName[i]);
#if UNITY_IPHONE
            iPhone.SetNoBackupFlag(Application.persistentDataPath + @"/" + a_assetName[i]);
#endif

            tFileStream = new FileStream(Application.persistentDataPath + @"/" + a_assetName[i], FileMode.Open, FileAccess.Read);
            if (!UnityToolKit.quickStringEqual(a_md5[i], System.BitConverter.ToString(sMd5Provider.ComputeHash(tFileStream))))   //check md5  note:save the file to disk first then check it.
            {
                if (onErrorDelegate != null)
                {
                    onErrorDelegate(EResourceErrorCode.DownloadAssetError, getInfoText("downloadAssetError") + "Failed to validate asset.");
                }
                tFileStream.Close();
                File.Delete(Application.persistentDataPath + @"/tmp_" + a_assetName[i]);
                yield break;
            }

            tFileStream.Close();
            updateNewAssetInfo(a_assetName[i], a_md5[i], a_version[i], tAssetSize);
        }

        sLocalMd5Doc.DocumentElement.SetAttribute("publishversion", sSerPublicVersion);
        sLocalMd5Doc.Save(sLocalMd5URL);

        sPerPublicVersion = sSerPublicVersion;
    }

    static void downloadDataCompletedHandler(object sender, DownloadDataCompletedEventArgs e)
    {
        if( e.Error != null)
        {
            sDownloadException = (WebException)e.Error;
            Debug.LogError( e.Error.Message );
            return;
        }

        ((FileStream)(e.UserState)).Write(e.Result, 0, e.Result.Length);
    }

    static void downloadProgressChangedHandler(object sender, DownloadProgressChangedEventArgs e)
    {
        sTotalBytesDownloaded += e.BytesReceived;
    }

    static bool isAssetDownloaded( string a_assetName )
    {
#if UNITY_STANDALONE
        return true;
#else
        AssetStatue tStatueLocal;
        AssetStatue tStatueServer;
        if (sServerAssetStatues.TryGetValue(a_assetName, out tStatueServer))
        {
            if (sLocalAssetStatues.TryGetValue(a_assetName, out tStatueLocal))
            {
                return string.Equals(tStatueLocal.mMd5, tStatueServer.mMd5);
            }
            return false;
        }
        Debug.LogWarning(a_assetName + " is not on server.");
        return true;
#endif
    }

    static void deleteAsset( string a_assetName )
    {
        File.Delete(Application.persistentDataPath + @"/" + a_assetName);
        sLocalAssetStatues.Remove(a_assetName);
        removeAssetInfo(a_assetName);
    }

    public static bool isAssetsDownloaded()
    {        
        foreach (KeyValuePair<string, AssetStatue> tPair in sServerAssetStatues)
        {
            if (!isAssetDownloaded(tPair.Key))
            {
                return false;
            }
        }
        return true;
    }

    public static void getDownloadingProgress( out long a_totalBytes, out long a_downloadedBytes )
    {
        a_totalBytes = sTotalBytesToDownload;
        a_downloadedBytes = sTotalBytesDownloaded;
    }

    public static void loadAssetFamily(EAssetFamily a_family, bool a_loadAll, bool a_unloadBundle)
    {
        foreach (AssetFamily tAssetFamily in mAssetFamilyList)
        {
            if (tAssetFamily.mFamily == a_family)
            {
                foreach (string tAsset in tAssetFamily.mAssets)
                {
                    loadAsset(tAsset, a_loadAll, a_unloadBundle);
                }
                return;
            }
        }
    }

    /* decompress the bundle will unload the bundle automatically, otherwise unload it manually.*/
    public static void loadAsset(string a_assetName, bool a_loadAll, bool a_unloadBundle)
    {
        AssetInfo tAssetInfo = loadAssetBundle( a_assetName );

        if (a_loadAll && !tAssetInfo.mLoadAll)
        {
            Object[] tObjs = tAssetInfo.mAssetBundle.LoadAll();
            for (int i = 0, imax = tObjs.Length; i < imax; i++ )
            {
                if (tAssetInfo.mObjectsTable.Contains(tObjs[i].name + tObjs[i].GetType().ToString()))
                {
                    continue; 
                }
                tAssetInfo.mObjectsTable.Add(tObjs[i].name + tObjs[i].GetType().ToString(), tObjs[i]);
            }
            tAssetInfo.mLoadAll = true;
            if (a_unloadBundle)
            {
                tAssetInfo.mAssetBundle.Unload(false);
                tAssetInfo.mAssetBundle = null;
            }
        }
        if( onAssetLoadedEvent != null)
        {
            onAssetLoadedEvent(a_assetName);
        }
    }

#if UNITY_STANDALONE
    static AssetInfo loadAssetBundle(string a_assetName)
    {
        AssetInfo tAssetInfo = (AssetInfo)sAssetInfoTable[a_assetName];

        if (tAssetInfo == null)
        {
            Debug.LogError("Unable to find asset " + a_assetName);
            return null;
        }

        if (!tAssetInfo.mLoaded)
        {
            string tDecompressFile = Application.temporaryCachePath + @"/" + a_assetName.Insert(a_assetName.LastIndexOf('.'), "_decompress");
            if (!File.Exists(tDecompressFile))
            {
                CompressTool.DecompressLZ4File(Application.streamingAssetsPath + @"/" + a_assetName, tDecompressFile);
            }
            tAssetInfo.mAssetBundle = AssetBundle.CreateFromFile(tDecompressFile);
            tAssetInfo.mLoaded = true;
        }
        return tAssetInfo;
    }
#else
    static AssetInfo loadAssetBundle(string a_assetName)
    {
        AssetInfo tAssetInfo = (AssetInfo)sAssetInfoTable[a_assetName];

        if (tAssetInfo == null)
        {
            Debug.LogError("Unable to find asset " + a_assetName);
            return null;
        }

        if (!tAssetInfo.mLoaded)
        {
            string tDecompressFile = Application.temporaryCachePath + @"/" + a_assetName.Insert(a_assetName.LastIndexOf('.'), "_decompress");
            if (!File.Exists(tDecompressFile))
            {
                if (File.Exists(Application.persistentDataPath + @"/" + a_assetName))
                {
                    CompressTool.DecompressLZ4File(Application.persistentDataPath + @"/" + a_assetName, tDecompressFile);
                }
                else
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
#if UNITY_ANDROID
                        byte[] tAssetBytes = sAndroidJavaObject.Call<byte[]>("readJarFile", a_assetName);
                        if (tAssetBytes != null && tAssetBytes.Length > 0)
                        {
                            string tTmpCompressed = Application.temporaryCachePath + @"/" + a_assetName.Insert(a_assetName.LastIndexOf('.'), "_tmpcompress");
                            FileStream tFileStream = new FileStream(tTmpCompressed, FileMode.Create);
                            tFileStream.Write(tAssetBytes, 0, tAssetBytes.Length);
                            tFileStream.Flush();
                            tFileStream.Close();
                            tFileStream = null;
                            CompressTool.DecompressLZ4File(tTmpCompressed, tDecompressFile);
                            File.Delete(tTmpCompressed);
                        }
                        else
                        {
                            Debug.LogError("Failed to load file : " + sStreamAssetURL + a_assetName);
                            return null;
                        }
#endif
                    }
                    else
                    {
                        CompressTool.DecompressLZ4File(Application.streamingAssetsPath + @"/" + a_assetName, tDecompressFile);
                    }
                }
            }
            tAssetInfo.mAssetBundle = AssetBundle.CreateFromFile(tDecompressFile);
            tAssetInfo.mLoaded = true;
        }
        return tAssetInfo;
    }
#endif

    public static void unloadAssetFamily( EAssetFamily a_family )
    {
        foreach (AssetFamily tAssetFamily in mAssetFamilyList)
        {
            if (tAssetFamily.mFamily == a_family)
            {
                foreach (string tAsset in tAssetFamily.mAssets)
                {
                    unloadAsset(tAsset);
                }
                return;
            }
        }
        Debug.LogError("Unable to find asset family " + a_family);
    }

    public static void unloadAsset( string a_assetName, bool a_safeUnload = false )
    {
        AssetInfo tAssetInfo = (AssetInfo)sAssetInfoTable[a_assetName];

        if (tAssetInfo == null)
        {
            Debug.LogError("Unable to find asset bundle " + a_assetName);
            return;
        }

        tAssetInfo.mObjectsTable.Clear();
        tAssetInfo.mLoadAll = false;

        if( tAssetInfo.mLoaded )
        {
            if (tAssetInfo.mAssetBundle)
            {
                tAssetInfo.mAssetBundle.Unload(!a_safeUnload);
                tAssetInfo.mAssetBundle = null;
            }
            tAssetInfo.mLoaded = false;
        }
       // Resources.UnloadUnusedAssets();
    }

    public static bool isAssetLoaded( string a_assetName )
    {
        AssetInfo tAssetInfo = (AssetInfo)sAssetInfoTable[a_assetName];
        if (tAssetInfo == null)
        {
            return false;
        }
        return tAssetInfo.mLoaded;
    }

    public static bool isAssetLoaded( EAssetFamily a_family )
    {
        string[] tAssets = null;

        foreach( AssetFamily tAssetFamily in mAssetFamilyList )
        {
            if( tAssetFamily.mFamily == a_family)
            {
                tAssets = tAssetFamily.mAssets;
                break;
            }
        }
        if( tAssets == null )
        {
            return false;
        }
        foreach( string tAsset in tAssets )
        {
            if( !isAssetLoaded(tAsset) )
            {
                return false;
            }
        }
        return true;
    }

    public static T getObject<T>( string a_objName, string a_assetName )
    {
        AssetInfo tAssetInfo = (AssetInfo)sAssetInfoTable[a_assetName];

        if (tAssetInfo == null)
        {
            return default(T);
        }

        if (tAssetInfo.mLoaded)
        {
            if (!tAssetInfo.mLoadAll)
            {
                object tobj = tAssetInfo.mAssetBundle.Load(a_objName, typeof(T));
                return (T)tobj;
            }
            return (T)tAssetInfo.mObjectsTable[a_objName + typeof(T).ToString()];
        }
        return default(T);
    }

    public static T getObject<T>(string a_objName, string[] a_assets)
    {
        foreach( string tAsset in a_assets )
        {
            T tObj = getObject<T>(a_objName, tAsset);
            if( tObj != null)
            {
                return tObj;
            }
        }
        return default(T);
    }

    public static T getObject<T>( string a_objName, EAssetFamily a_family )
    {
        foreach (AssetFamily tAssetFamily in mAssetFamilyList)
        {
            if (tAssetFamily.mFamily == a_family)
            {
                return getObject<T>(a_objName, tAssetFamily.mAssets);
            }
        }
        return default(T);
    }

    static EAppUpdateInfo getAppUpdateInfo(string a_loaclVer, string a_serverVer)
    {
        int[] tLocalVer = UnityToolKit.toIntArray(a_loaclVer, '.');
        int[] tServerVer = UnityToolKit.toIntArray(a_serverVer, '.');

        EAppUpdateInfo tAppUpdateInfo = EAppUpdateInfo.Latest;
        
        if (tLocalVer[0] < tServerVer[0])
        {
            tAppUpdateInfo = EAppUpdateInfo.AppUpdate;
        }
        else if (tLocalVer[0] == tServerVer[0])
        {
            if (tLocalVer[1] < tServerVer[1])
            {
                tAppUpdateInfo = EAppUpdateInfo.DllUpdate;
            }
            else if (tLocalVer[1] == tServerVer[1])
            {
                if (tLocalVer[2] < tServerVer[2])
                {
                    tAppUpdateInfo = EAppUpdateInfo.ResourceUpdate;
                }
                else if (tLocalVer[2] > tServerVer[2])
                {
                    tAppUpdateInfo = EAppUpdateInfo.Invalid;
                }
            }
            else
            {
                tAppUpdateInfo = EAppUpdateInfo.Invalid;
            }
        }
        else
        {
            tAppUpdateInfo = EAppUpdateInfo.Invalid;
        }

        return tAppUpdateInfo;
    }

    public static string[] getAssets(EAssetFamily a_family)
    {
        foreach (AssetFamily tFamily in mAssetFamilyList)
        {
            if (a_family == tFamily.mFamily)
            {
                return tFamily.mAssets;
            }
        }
        return null;
    }

    public static void UnloadGameResource()
    {
        ResourceManager.unloadAssetFamily(EAssetFamily.GamePlayCommon);
        ResourceManager.unloadAssetFamily(EAssetFamily.Enemy);
        ResourceManager.unloadAssetFamily(EAssetFamily.Player);
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
    }

    public static void Reset()
    {
        if (sInited)
        {
            onAssetLoadedEvent = null;
            onControlingAssetEvent = null;
            onErrorDelegate = null;

            sLocalAssetStatues.Clear();
            sServerAssetStatues.Clear();

            sServerStatue = null;

            sResPublicVersion = "0.0.0";
            sPerPublicVersion = "0.0.0";
            sSerPublicVersion = "0.0.0";

            foreach (KeyValuePair<string, AssetInfo> tPair in sAssetInfoTable)
            {
                ResourceManager.unloadAsset(tPair.Key);
            }
            sAssetInfoTable.Clear();

            sTotalBytesToDownload = 0;
            sTotalBytesDownloaded = 0;
            sDownloadException = null;

            sLocalMd5Doc = null;
            sInfoText = null;

#if UNITY_ANDROID
            if (Application.platform == RuntimePlatform.Android)
            {
                sAndroidJavaClass.Dispose();
                sAndroidJavaObject.Dispose();
                sAndroidJavaClass = null;
                sAndroidJavaObject = null;
            }
#endif
            Resources.UnloadUnusedAssets();
            sInited = false;
        }
    }


    static void LogInfo(string a_info)
    {
        //Debug.Log(info);
        if (onControlingAssetEvent != null)
        {
            onControlingAssetEvent(a_info);
        }
    }
}
