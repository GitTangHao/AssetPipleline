using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;


public class ServerStatueGenerator : EditorWindow
{
    static bool sIsServerOn = true;
    static string sServerNotice;
    static string sAppUrl;
    static string sAssetsUrl = string.Empty;
    static string sAssetsENUrl = string.Empty;
    static string sAssetsCNUrl = string.Empty;
    static string sAssetsHKUrl = string.Empty;

    [MenuItem("Custom/ServerStatueGenerator")]
    static void GetTargetPackageFiles()
    {
        EditorWindow.GetWindowWithRect<ServerStatueGenerator>(new Rect(0, 0, 385, 438), false, "Server Statue Generator", true);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        sIsServerOn = EditorGUILayout.Toggle(new GUIContent("Server On"), sIsServerOn, GUILayout.Width(375), GUILayout.Height(16));

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Apk Url", GUILayout.Width(80), GUILayout.Height(16));
        EditorGUILayout.SelectableLabel(@"http://www.stackoverflow.com", GUILayout.Width(285), GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();
        sAppUrl = EditorGUILayout.TextField(sAppUrl, GUILayout.Width(375), GUILayout.Height(16));

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Assets Url", GUILayout.Width(82), GUILayout.Height(16));
        EditorGUILayout.SelectableLabel(@"http://aop-android.oss-cn-hangzhou.aliyuncs.com/assets/0.0.0/", GUILayout.Width(285), GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();
        sAssetsUrl = EditorGUILayout.TextField(sAssetsUrl, GUILayout.Width(375), GUILayout.Height(16));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Assets EN Url", GUILayout.Width(82), GUILayout.Height(16));
        EditorGUILayout.SelectableLabel(@"http://aop-android.oss-cn-hangzhou.aliyuncs.com/assets/0.0.0/", GUILayout.Width(285), GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();
        sAssetsENUrl = EditorGUILayout.TextField(sAssetsENUrl, GUILayout.Width(375), GUILayout.Height(16));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Assets CN Url", GUILayout.Width(82), GUILayout.Height(16));
        EditorGUILayout.SelectableLabel(@"http://aop-android.oss-cn-hangzhou.aliyuncs.com/assets/0.0.0/", GUILayout.Width(285), GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();
        sAssetsCNUrl = EditorGUILayout.TextField(sAssetsCNUrl, GUILayout.Width(375), GUILayout.Height(16));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Assets HK Url", GUILayout.Width(82), GUILayout.Height(16));
        EditorGUILayout.SelectableLabel(@"http://aop-android.oss-cn-hangzhou.aliyuncs.com/assets/0.0.0/", GUILayout.Width(285), GUILayout.Height(16));
        EditorGUILayout.EndHorizontal();
        sAssetsHKUrl = EditorGUILayout.TextField(sAssetsHKUrl, GUILayout.Width(375), GUILayout.Height(16));

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Server Notice", GUILayout.Width(375), GUILayout.Height(16));
        sServerNotice = EditorGUILayout.TextField(sServerNotice, GUILayout.Width(375), GUILayout.Height(150));

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Check", GUILayout.Width(132), GUILayout.Height(27)))
        {
            Check();
        }
        if (GUILayout.Button("Generator", GUILayout.Width(132), GUILayout.Height(27)))
        {
            GenerateServerStatue();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.EndVertical();
    }

    void GenerateServerStatue()
    {
        sAssetsUrl = checkUrl(sAssetsUrl);
        sAssetsENUrl = checkUrl(sAssetsENUrl);
        sAssetsCNUrl = checkUrl(sAssetsCNUrl);
        sAssetsHKUrl = checkUrl(sAssetsHKUrl);

        XmlDocument tDoc = new XmlDocument();

        XmlElement tRoot = tDoc.CreateElement("root");
        
        tRoot.SetAttribute("IsServerOn", sIsServerOn.ToString());
        tRoot.SetAttribute("AppUrl", sAppUrl);
        tRoot.SetAttribute("AssetsUrl", sAssetsUrl);
        tRoot.SetAttribute("AssetsENUrl", sAssetsENUrl);
        tRoot.SetAttribute("AssetsCNUrl", sAssetsCNUrl);
        tRoot.SetAttribute("AssetsHKUrl", sAssetsHKUrl);
        tRoot.SetAttribute("ServerNotice", sServerNotice);

        tDoc.AppendChild(tRoot);

        tDoc.Save(Application.dataPath + "/AssetPipleLine/Editor/Data/ServerStatueInfo.xml");
        ShowNotification(new GUIContent("Done generator server statue info."));
    }

    string checkUrl(string a_url)
    {
        if (!string.IsNullOrEmpty(a_url) && !a_url.EndsWith(@"/"))
        {
            return a_url.Insert(a_url.Length, @"/");
        }
        return a_url;
    }

    void Check()
    {
        System.Diagnostics.Process.Start("iexplore.exe", Application.dataPath + "/AssetPipleLine/Editor/Data/");
    }
}
