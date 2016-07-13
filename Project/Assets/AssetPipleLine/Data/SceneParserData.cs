using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;


public class ParserSceneInfo
{
    public string mBasePath;
    public string mSceneName;
    
    public ParserSceneInfo( string  a_sceneName, string a_basePath)
    {
        mSceneName = a_sceneName;
        mBasePath = a_basePath;
    }
}


public class SceneParserData 
{
    public static readonly string sStaticObjectRoot = "GamePlayScene/Env art static";
    public static readonly string sDynamicObjectRoot = "GamePlayScene/Env art anm";
    public static readonly string sLightObjectRoot = "GamePlayScene/Env art disable";
    public static readonly string sLightProbesRoot = "GamePlayScene/Env art light probes";

    static string sSceneParserDataFile = Application.dataPath + @"/AssetPipleLine/Editor/Data/SceneParserData.xml";
    static string sSceneParserOutputDir = Application.dataPath + @"/Scenes/Exported";
    static List<ParserSceneInfo> sParserSceneInfos = new List<ParserSceneInfo>();

    public static string getDataFile()
    {
        return sSceneParserDataFile;
    }

    public static void setDataFile(string a_file)
    {
        sSceneParserDataFile = a_file;
    }

    public static string getOutputDir()
    {
        return sSceneParserOutputDir;
    }

    public static void setOutputDir(string a_dir)
    {
        sSceneParserOutputDir = a_dir;
    }

    public static string[] getParserScenes()
    {
        string[] tScenes = new string[sParserSceneInfos.Count];
        for( int i = 0, imax = sParserSceneInfos.Count; i < imax; i++)
        {
            tScenes[i] = sParserSceneInfos[i].mSceneName;
        }
        return tScenes;
    }

    public static string[] getSceneFileBasePaths()
    {
        string[] tSceneFileBasePaths = new string[sParserSceneInfos.Count];
        for (int i = 0, imax = sParserSceneInfos.Count; i < imax; i++)
        {
            tSceneFileBasePaths[i] = sParserSceneInfos[i].mBasePath;
        }
        return tSceneFileBasePaths;
    }

    public static bool addParserScene(string a_sceneName, string a_basePath)
    {
        foreach(ParserSceneInfo tInfo in sParserSceneInfos)
        {
            if (string.Equals(tInfo.mSceneName, a_sceneName))
            {
                return false;
            }
        }
        sParserSceneInfos.Add(new ParserSceneInfo(a_sceneName, a_basePath));
        sParserSceneInfos.Sort(delegate(ParserSceneInfo a, ParserSceneInfo b) { return string.Compare(a.mSceneName, b.mSceneName); });
        return true;
    }

    public static bool removeParserScene(string a_sceneName)
    {
        foreach (ParserSceneInfo tInfo in sParserSceneInfos)
        {
            if (string.Equals(tInfo.mSceneName, a_sceneName))
            {
                sParserSceneInfos.Remove(tInfo);
                return true;
            }
        }
        return false;
    }

    public static void loadParserSceneInfo()
    {   
        sParserSceneInfos.Clear();
        if (File.Exists(sSceneParserDataFile))
        {
            string tXmlContent = File.ReadAllText(sSceneParserDataFile);
            XmlDocument tDoc = new XmlDocument();
            tDoc.LoadXml(tXmlContent);

            XmlElement tScenes = (XmlElement)tDoc.FirstChild.SelectSingleNode("scenes");
            for (int i = 0, imax = tScenes.ChildNodes.Count; i < imax; i++)
            {
                string a_sceneName = ((XmlElement)tScenes.ChildNodes[i]).GetAttribute("name");
                string a_basePath = ((XmlElement)tScenes.ChildNodes[i]).GetAttribute("basePath");
                addParserScene(a_sceneName, a_basePath);
            }
        }
    }

    public static void saveParserSceneInfo()
    {
        XmlDocument tDoc = new XmlDocument();

        XmlElement tRoot = tDoc.CreateElement("root");
        tDoc.AppendChild(tRoot);

        XmlElement tSceneRoot = tDoc.CreateElement("scenes");
        tRoot.AppendChild(tSceneRoot);

        for (int i = 0; i < sParserSceneInfos.Count; i++)
        {
            XmlElement tXE = tDoc.CreateElement("scene");
            tXE.SetAttribute("name", sParserSceneInfos[i].mSceneName);
            tXE.SetAttribute("basePath", sParserSceneInfos[i].mBasePath);
            tSceneRoot.AppendChild(tXE);
        }

        tDoc.Save(sSceneParserDataFile);
    }


}
