using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;


public enum PackageMode
{
    SinglePack = 0,
    MultiPack,
}

public class PackFolderInfo
{
    public string mFolderName;
    public string mBasePath;
    public PackageMode mPackMode;

    public PackFolderInfo(string a_folderName, string a_bathPath, PackageMode a_packMode)
    {
        mFolderName = a_folderName;
        mBasePath = a_bathPath;
        mPackMode = a_packMode;
    }
}

public class PackageConfigData 
{
    static string sPackConfigFile = Application.dataPath + @"/AssetPipleLine/Editor/Data/PackInfoEN.xml";

    static List<PackFolderInfo> sAssetFolderInfos = new List<PackFolderInfo>();
    static List<PackFolderInfo> sSceneFolderInfos = new List<PackFolderInfo>();

    public static void SetPackConfigFile(string a_file)
    {
        sPackConfigFile = a_file;
    }

    public static string GetPackConfigFile()
    {
        return sPackConfigFile;
    }

    public static string[] GetAssetFolders()
    {
        string[] tAssetFolders = new string[sAssetFolderInfos.Count];
        for(int i = 0, imax = sAssetFolderInfos.Count; i < imax; i++)
        {
            tAssetFolders[i] = sAssetFolderInfos[i].mFolderName;
        }
        return tAssetFolders;
    }

    public static string[] GetSceneFolders()
    {
        string[] tSceneFolders = new string[sSceneFolderInfos.Count];
        for (int i = 0, imax = sSceneFolderInfos.Count; i < imax; i++)
        {
            tSceneFolders[i] = sSceneFolderInfos[i].mFolderName;
        }
        return tSceneFolders;
    }

    public static string[] GetAssetFolderBasePaths()
    {
        string[] tAssetFolderBasePaths = new string[sAssetFolderInfos.Count];
        for (int i = 0, imax = sAssetFolderInfos.Count; i < imax; i++)
        {
            tAssetFolderBasePaths[i] = sAssetFolderInfos[i].mBasePath;
        }
        return tAssetFolderBasePaths;
    }

    public static string[] GetSceneFolderBasePaths()
    {
        string[] tSceneFolderBasePaths = new string[sSceneFolderInfos.Count];
        for (int i = 0, imax = sSceneFolderInfos.Count; i < imax; i++)
        {
            tSceneFolderBasePaths[i] = sSceneFolderInfos[i].mBasePath;
        }
        return tSceneFolderBasePaths;
    }

    public static PackageMode[] GetAssetPackModes()
    {
        PackageMode[] tAssetPackModes = new PackageMode[sAssetFolderInfos.Count];
        for (int i = 0, imax = sAssetFolderInfos.Count; i < imax; i++)
        {
            tAssetPackModes[i] = sAssetFolderInfos[i].mPackMode;
        }
        return tAssetPackModes;
    }

    public static PackageMode[] GetScenePackModes()
    {
        PackageMode[] tScenePackModes = new PackageMode[sSceneFolderInfos.Count];
        for (int i = 0, imax = sSceneFolderInfos.Count; i < imax; i++)
        {
            tScenePackModes[i] = sSceneFolderInfos[i].mPackMode;
        }
        return tScenePackModes;
    }

    public static bool AddAssetFolder(string a_folderName, string a_basePath, PackageMode a_mode)
    {
        foreach(PackFolderInfo tFolderInfo in sAssetFolderInfos)
        {
            if(string.Equals(tFolderInfo.mFolderName, a_folderName))
            {
                Debuger.LogError(a_folderName + " is already in pack list!");
                return false;
            }
        }
        sAssetFolderInfos.Add(new PackFolderInfo(a_folderName, a_basePath, a_mode));
        sAssetFolderInfos.Sort(delegate(PackFolderInfo a, PackFolderInfo b) { return string.Compare(a.mFolderName, b.mFolderName); });
        return true;
    }

    public static bool RemoveAssetFolder(string a_folderName)
    {
        for (int i = 0; i < sAssetFolderInfos.Count; i++)
        {
            if (string.Equals(sAssetFolderInfos[i].mFolderName, a_folderName))
            {
                sAssetFolderInfos.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public static bool AddSceneFolder(string a_folderName, string a_basePath, PackageMode a_mode)
    {
        foreach (PackFolderInfo tFolderInfo in sSceneFolderInfos)
        {
            if (string.Equals(tFolderInfo.mFolderName, a_folderName))
            {
                Debuger.LogError(a_folderName + " is already in pack list!");
                return false;
            }
        }
        sSceneFolderInfos.Add(new PackFolderInfo(a_folderName, a_basePath, a_mode));
        sSceneFolderInfos.Sort(delegate(PackFolderInfo a, PackFolderInfo b) { return string.Compare(a.mFolderName, b.mFolderName); });
        return true;
    }

    public static bool RemoveSceneFolder(string a_folderName)
    {
        for (int i = 0; i < sSceneFolderInfos.Count; i++)
        {
            if (string.Equals(sSceneFolderInfos[i].mFolderName, a_folderName))
            {
                sSceneFolderInfos.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public static void SetAssetFolderPackMode(string a_folderName, PackageMode a_packMode)
    {
        foreach (PackFolderInfo tFolderInfo in sAssetFolderInfos )
        {
            if (string.Equals(tFolderInfo.mFolderName, a_folderName))
            {
                tFolderInfo.mPackMode = a_packMode;
                return;
            }
        }
    }

    public static void SetSceneFolderPackMode(string a_folderName, PackageMode a_packMode)
    {
        foreach (PackFolderInfo tFolderInfo in sSceneFolderInfos)
        {
            if (string.Equals(tFolderInfo.mFolderName, a_folderName))
            {
                tFolderInfo.mPackMode = a_packMode;
                return;
            }
        }
    }

    public static string GetAssetFolderPath( string a_folderName )
    {
        foreach (PackFolderInfo tFolderInfo in sAssetFolderInfos)
        {
            if (string.Equals(tFolderInfo.mFolderName, a_folderName))
            {
                return tFolderInfo.mBasePath + tFolderInfo.mFolderName;
            }
        }
        Debuger.LogError(a_folderName + "can not find in asset pack list!");
        return null;
    }
    
    public static string GetSceneFolderPath( string a_folderName )
    {
        foreach (PackFolderInfo tFolderInfo in sSceneFolderInfos)
        {
            if (string.Equals(tFolderInfo.mFolderName, a_folderName))
            {
                return tFolderInfo.mBasePath + tFolderInfo.mFolderName;
            }
        }
        Debuger.LogError(a_folderName + "can not find in scene pack list!");
        return null;
    }

    public static void LoadPackConfigure()
    {
        sAssetFolderInfos.Clear();
        sSceneFolderInfos.Clear();
        if (File.Exists(sPackConfigFile))
        {
            string tXmlContent = File.ReadAllText(sPackConfigFile);
            XmlDocument tDoc = new XmlDocument();
            tDoc.LoadXml(tXmlContent);

            XmlElement tAssetBundles = (XmlElement)tDoc.FirstChild.SelectSingleNode("assetbundles");
            for (int i = 0, imax = tAssetBundles.ChildNodes.Count; i < imax; i++ )
            {
                string a_assetName = ((XmlElement)tAssetBundles.ChildNodes[i]).GetAttribute("name");
                string a_basePath = ((XmlElement)tAssetBundles.ChildNodes[i]).GetAttribute("basePath");
                string a_packMode = ((XmlElement)tAssetBundles.ChildNodes[i]).GetAttribute("packMode");
                sAssetFolderInfos.Add(new PackFolderInfo(a_assetName, a_basePath, (PackageMode)System.Enum.Parse(typeof(PackageMode), a_packMode, true)));
            }

            XmlElement tSceneBundles = (XmlElement)tDoc.FirstChild.SelectSingleNode("scenes");
            for (int i = 0, imax = tSceneBundles.ChildNodes.Count; i < imax; i++)
            {
                string a_sceneName = ((XmlElement)tSceneBundles.ChildNodes[i]).GetAttribute("name");
                string a_basePath = ((XmlElement)tSceneBundles.ChildNodes[i]).GetAttribute("basePath");
                string a_packMode = ((XmlElement)tSceneBundles.ChildNodes[i]).GetAttribute("packMode");
                sSceneFolderInfos.Add(new PackFolderInfo(a_sceneName, a_basePath, (PackageMode)System.Enum.Parse(typeof(PackageMode), a_packMode, true)));
            }
        }
    }

    public static void SavePackConfigure()
    {
        XmlDocument tDoc = new XmlDocument();

        XmlElement tRoot = tDoc.CreateElement("root");
        tDoc.AppendChild(tRoot);

        XmlElement tAssetBundleRoot = tDoc.CreateElement("assetbundles");
        tRoot.AppendChild(tAssetBundleRoot);

        for (int i = 0; i < sAssetFolderInfos.Count; i++)
        {
            XmlElement tXE = tDoc.CreateElement("assetbundle");
            tXE.SetAttribute("name", sAssetFolderInfos[i].mFolderName);
            tXE.SetAttribute("basePath", sAssetFolderInfos[i].mBasePath);
            tXE.SetAttribute("packMode", sAssetFolderInfos[i].mPackMode.ToString());
            tAssetBundleRoot.AppendChild(tXE);
        }

        XmlElement tSceneBundleRoot = tDoc.CreateElement("scenes");
        tRoot.AppendChild(tSceneBundleRoot);

        for (int i = 0; i < sSceneFolderInfos.Count; i++)
        {
            XmlElement tXE = tDoc.CreateElement("scene");
            tXE.SetAttribute("name", sSceneFolderInfos[i].mFolderName);
            tXE.SetAttribute("basePath", sSceneFolderInfos[i].mBasePath);
            tXE.SetAttribute("packMode", sSceneFolderInfos[i].mPackMode.ToString());
            tSceneBundleRoot.AppendChild(tXE);
        }

        tDoc.Save(sPackConfigFile);
    }

}
