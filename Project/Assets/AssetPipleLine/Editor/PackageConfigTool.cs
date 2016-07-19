using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Security.Cryptography;


public class PackageConfigTool : EditorWindow
{
    static Vector2 sAssetScroll;
    static Vector2 sSceneScroll;

    static PackageMode[] sAssetPackModes;
    static PackageMode[] sScenePackModes;

    static string[] sAssetFolders = null;
    static string[] sSceneFolders = null;

    [MenuItem("AssetPipleLine/PackageConfigTool")]
    static void OpenPackConfigTool()
    {
        PackageConfigData.LoadPackConfigure();
        EditorWindow.GetWindowWithRect<PackageConfigTool>(new Rect(0, 0, 680, 532), false, "Package Config Tool", true);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Package Config Tool");
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Package Config File : " + PackageConfigData.GetPackConfigFile(), GUILayout.Width(570));
        if (GUILayout.Button("Set", GUILayout.Width(64), GUILayout.Height(15)))
        {
            setPackConfigFile();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Bundle Folder", GUILayout.Width(210));
        EditorGUILayout.LabelField("Pack Mode", GUILayout.Width(140));
        EditorGUILayout.LabelField("Check Path", GUILayout.Width(150));
        EditorGUILayout.LabelField("Remove Folder", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        sAssetScroll = EditorGUILayout.BeginScrollView(sAssetScroll, GUILayout.Width(660), GUILayout.Height(125) );

        sAssetFolders = PackageConfigData.GetAssetFolders();
        sAssetPackModes = PackageConfigData.GetAssetPackModes();

        for (int i = 0; i < sAssetFolders.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(sAssetFolders[i].Substring(sAssetFolders[i].LastIndexOfAny(new char[] { '/', '\\' }) + 1), GUILayout.Width(200), GUILayout.Height(20));
            sAssetPackModes[i] = (PackageMode)EditorGUILayout.EnumPopup("", sAssetPackModes[i], GUILayout.Width(84), GUILayout.Height(14) );
            PackageConfigData.SetAssetFolderPackMode(sAssetFolders[i], sAssetPackModes[i]);
            EditorGUILayout.LabelField(" ", GUILayout.Width(56), GUILayout.Height(14));
            if (GUILayout.Button("Check Path", GUILayout.Width(84), GUILayout.Height(14)))
            {
                Debug.Log(Application.dataPath + PackageConfigData.GetAssetFolderBasePaths()[i] + sAssetFolders[i]);
            }
            EditorGUILayout.LabelField(" ", GUILayout.Width(66), GUILayout.Height(14));
            if (GUILayout.Button("Remove Folder", GUILayout.Width(100), GUILayout.Height(14)))
            {
                PackageConfigData.RemoveAssetFolder(sAssetFolders[i]);
                ShowNotification(new GUIContent("Done!"));
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");

        GUILayout.Space(32);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stream Scene Folder", GUILayout.Width(210));
        EditorGUILayout.LabelField("Pack Mode", GUILayout.Width(140));
        EditorGUILayout.LabelField("Check Path", GUILayout.Width(150));
        EditorGUILayout.LabelField("Remove Folder", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        sSceneScroll = EditorGUILayout.BeginScrollView(sSceneScroll, GUILayout.Width(660), GUILayout.Height(125));

        sSceneFolders = PackageConfigData.GetSceneFolders();
        sScenePackModes = PackageConfigData.GetScenePackModes();

        for (int i = 0; i < sSceneFolders.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(sSceneFolders[i].Substring(sSceneFolders[i].LastIndexOfAny(new char[] { '/', '\\' }) + 1), GUILayout.Width(200), GUILayout.Height(20));
            sScenePackModes[i] = (PackageMode)EditorGUILayout.EnumPopup("", sScenePackModes[i], GUILayout.Width(84), GUILayout.Height(14));
            PackageConfigData.SetSceneFolderPackMode(sSceneFolders[i], sScenePackModes[i]);
            EditorGUILayout.LabelField(" ", GUILayout.Width(56), GUILayout.Height(14));
            if (GUILayout.Button("Check Path", GUILayout.Width(84), GUILayout.Height(14)))
            {
                Debug.Log(Application.dataPath + PackageConfigData.GetSceneFolderBasePaths()[i] + sSceneFolders[i]);
            }
            EditorGUILayout.LabelField(" ", GUILayout.Width(66), GUILayout.Height(14));
            if (GUILayout.Button("Remove Folder", GUILayout.Width(100), GUILayout.Height(14)))
            {
                PackageConfigData.RemoveSceneFolder(sSceneFolders[i]);
                ShowNotification(new GUIContent("Done!"));
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Add Asset Bundle Folder", GUILayout.Width(160), GUILayout.Height(26)))
        {
            addAssetFolder();
        }
        if (GUILayout.Button("Add Stream Scene Folder", GUILayout.Width(160), GUILayout.Height(26)))
        {
            addSceneFolder();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        if (GUILayout.Button("Reload Pack Config", GUILayout.Width(160), GUILayout.Height(26)))
        {
            PackageConfigData.LoadPackConfigure();
            ShowNotification(new GUIContent("Done!"));
        }
        if (GUILayout.Button("Save Pack Config", GUILayout.Width(160), GUILayout.Height(26)))
        {
            PackageConfigData.SavePackConfigure();
            ShowNotification(new GUIContent("Done!"));
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void setPackConfigFile()
    {
        string tDefaultPath = PackageConfigData.GetPackConfigFile();
        tDefaultPath = tDefaultPath.Substring(0, tDefaultPath.LastIndexOfAny(new char[] { '/', '\\' }));
        string tFile = EditorUtility.OpenFilePanel("Select an pack config file", tDefaultPath, "xml");
        if (!string.IsNullOrEmpty(tFile) && tFile.EndsWith(".xml"))
        {
            PackageConfigData.SetPackConfigFile(tFile);
            PackageConfigData.LoadPackConfigure();
            ShowNotification(new GUIContent("Done!"));
            return;
        }
        ShowNotification(new GUIContent("Failed!"));
    }

    void addAssetFolder()
    {
        string tFolder = EditorUtility.OpenFolderPanel("Select an asset folder", Application.dataPath, "");
        if (!string.IsNullOrEmpty(tFolder) && tFolder.Contains(Application.dataPath))
        {
            string tBasePath = tFolder.Substring(tFolder.IndexOf("Assets/") + 6);
            tBasePath = tBasePath.Substring(0, tBasePath.LastIndexOf("/") + 1);
            string tFolderName = tFolder.Substring(tFolder.LastIndexOf("/") + 1);

            if (PackageConfigData.AddAssetFolder(tFolderName, tBasePath, PackageMode.SinglePack))
            {
                ShowNotification(new GUIContent("Success!"));
                return;
            }
        }
        ShowNotification(new GUIContent("Failed!"));
    }

    void addSceneFolder()
    {
        string tPath = EditorUtility.OpenFolderPanel("Select Scene Folder", Application.dataPath, "");
        if (!string.IsNullOrEmpty(tPath) && tPath.Contains(Application.dataPath))
        {
            string tBasePath = tPath.Substring(tPath.IndexOf("Assets/") + 6);
            tBasePath = tBasePath.Substring(0, tBasePath.LastIndexOf("/") + 1);
            string tFolderName = tPath.Substring(tPath.LastIndexOf("/") + 1);

            if (PackageConfigData.AddSceneFolder(tFolderName, tBasePath, PackageMode.SinglePack))
            {
                ShowNotification(new GUIContent("Success!"));
                return;
            }
        }
        ShowNotification(new GUIContent("Failed!"));
    }
}
