using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Security.Cryptography;


public class PackageTool : EditorWindow
{
    //pack readonly data.
    static readonly string[] sBuiltInScenes = new string[] { "UILoading.unity" };
    static readonly BuildAssetBundleOptions sBuildAssetBundleOption = BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies |
                                                                      BuildAssetBundleOptions.DeterministicAssetBundle;
    static readonly string sPackConfigDataFolder = Application.dataPath + "/AssetPipleLine/Editor/Data/";

    //UI window & scroll view
    static Rect sMd5WindowRect = new Rect(10, 589, 520, 78);
    static Rect sVersionWindowRect = new Rect(10, 431, 520, 127);
    static Vector2 sAssetScroll;
    static Vector2 sSceneScroll;    

    //UI Pack tool buttons
    static bool sCompressPackage = true;
    static bool sForceReImport;
    static bool sPackSelected;
    static bool sPackUI;
    static bool sPackAllAssetBundles;
    static bool sPackAllScenes;
    static bool sPackAll;
    static bool sPackPrimePack;
    static bool sRunPackCmd;

    //UI Version tool buttons
    static bool sDoBuild = true;
    static bool sCleanAssetBundles = true;
    static bool sCleanScenes = true;
    static bool sBuildAssetBundles = true;
    static bool sBuildScenes = true;
    static bool sBuildApk;
    static bool sBuildIpa;
    static int sAssetServerSelected;

    //UI Md5 tool buttons
    static bool sUpdateMd5File;
    static bool sCheckMd5File;

    //Pack config file data
    static string[] sAssetFolders;
    static string[] sSceneFolders;
    static string[] sAssetFolderBasePaths;
    static string[] sSceneFolderBasePaths;
    static PackageMode[] sAssetPackModes;
    static PackageMode[] sScenePackModes;
    static bool[] sIsPackScene;
    static bool[] sIsPackAsset;

    //User data
    static BuildTarget sBuildTarget = BuildTarget.StandaloneWindows64;
    static string sPackagePath = Application.streamingAssetsPath + @"/";
    static string sBuildVersion = string.Empty;
    static string sApkPath = string.Empty;
    static string sIpaPath = string.Empty;
    static Dictionary<string, string> sAssetServerList = new Dictionary<string,string>();
    static GUIContent[] sServersPopup = new GUIContent[0];


    [MenuItem("AssetPipleLine/PackageTool")]
    static void OpenPackTool()
    {
        PackageConfigData.LoadPackConfigure();
#if UNITY_ANDROID
        sBuildTarget = BuildTarget.Android;
#elif UNITY_IPHONE
        sBuildTarget = BuildTarget.iPhone;
#elif UNITY_STANDALONE_WIN
        sBuildTarget = BuildTarget.StandaloneWindows64;
#elif UNITY_STANDALONE_OSX
        sBuildTarget = BuildTarget.StandaloneOSXIntel64;
#endif
        EditorWindow.GetWindowWithRect<PackageTool>(new Rect(0, 0, 680, 682), false, "Package Tool", true);
    }

    void OnGUI()
    {
        updatePackConfig();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Package Tool");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Package Config File : ", GUILayout.Width(125));
        EditorGUILayout.SelectableLabel(PackageConfigData.GetPackConfigFile(), GUILayout.Width(476), GUILayout.Height(15));
        if (GUILayout.Button("Set", GUILayout.Width(64), GUILayout.Height(14)))
        {
            setPackConfigFile();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Force re-import", GUILayout.Width(94), GUILayout.Height(20));
        sForceReImport = EditorGUILayout.Toggle(sForceReImport, GUILayout.Width(15), GUILayout.Height(15));
        EditorGUILayout.LabelField("  Compress package", GUILayout.Width(123), GUILayout.Height(20));
        sCompressPackage = EditorGUILayout.Toggle(sCompressPackage, GUILayout.Width(15), GUILayout.Height(15));
        EditorGUILayout.LabelField("  Package platform", GUILayout.Width(114), GUILayout.Height(20));
        sBuildTarget = (BuildTarget)EditorGUILayout.EnumPopup(sBuildTarget, GUILayout.Width(150), GUILayout.Height(20));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset Bundle Folder");
        EditorGUILayout.LabelField("  Pack Mode");
        EditorGUILayout.LabelField("Pack This Folder");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        sAssetScroll = EditorGUILayout.BeginScrollView(sAssetScroll, GUILayout.Width(660), GUILayout.Height(125) );

        for (int i = 0; i < sAssetFolders.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(sAssetFolders[i], GUILayout.Width(220), GUILayout.Height(20));
            sAssetPackModes[i] = (PackageMode)EditorGUILayout.EnumPopup("", sAssetPackModes[i], GUILayout.Width(84), GUILayout.Height(12) );
            PackageConfigData.SetAssetFolderPackMode(sAssetFolders[i], sAssetPackModes[i]);
            EditorGUILayout.LabelField(" ", GUILayout.Width(180), GUILayout.Height(20));
            sIsPackAsset[i] = EditorGUILayout.Toggle("", sIsPackAsset[i], GUILayout.Width(12), GUILayout.Height(12));
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");

        GUILayout.Space(14);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stream Scene Folder");
        EditorGUILayout.LabelField("  Pack Mode");
        EditorGUILayout.LabelField("Pack This Folder");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        sSceneScroll = EditorGUILayout.BeginScrollView(sSceneScroll, GUILayout.Width(660), GUILayout.Height(95));

        for (int i = 0; i < sSceneFolders.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(sSceneFolders[i], GUILayout.Width(220), GUILayout.Height(20));
            sScenePackModes[i] = (PackageMode)EditorGUILayout.EnumPopup("", sScenePackModes[i], GUILayout.Width(84), GUILayout.Height(12));
            PackageConfigData.SetSceneFolderPackMode(sSceneFolders[i], sScenePackModes[i]);
            EditorGUILayout.LabelField(" ", GUILayout.Width(180), GUILayout.Height(20));
            sIsPackScene[i] = EditorGUILayout.Toggle("", sIsPackScene[i], GUILayout.Width(12), GUILayout.Height(12));
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal(); 

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("-------------------------------------------------------------------------------------------------------------------------------");

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sPackSelected = GUILayout.Button("Pack Selected", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sPackUI = GUILayout.Button("Pack UI", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sPackAllAssetBundles = GUILayout.Button("Pack All AssetBundle", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sPackAllScenes = GUILayout.Button("Pack All Scene", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sPackAll = GUILayout.Button("Pack All", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sPackPrimePack = GUILayout.Button("Pack Prime Pack", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sRunPackCmd = GUILayout.Button("Run Pack Cmd", GUILayout.Width(132), GUILayout.Height(28));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();

        BeginWindows();
        sVersionWindowRect = GUILayout.Window(1, sVersionWindowRect, VersionTool, "Version Tool");
        sMd5WindowRect = GUILayout.Window(2, sMd5WindowRect, Md5Tool, "Md5 Tool");
        EndWindows();
    }

    void setPackConfigFile()
    {
        string tDefaultPath = PackageConfigData.GetPackConfigFile();
        tDefaultPath = tDefaultPath.Substring(0, tDefaultPath.LastIndexOfAny(new char[]{'/', '\\'}));
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

    static void updatePackConfig()
    {
        sAssetFolders = PackageConfigData.GetAssetFolders();
        sAssetPackModes = PackageConfigData.GetAssetPackModes();
        sAssetFolderBasePaths = PackageConfigData.GetAssetFolderBasePaths();
        if (sIsPackAsset == null || sAssetFolders.Length != sIsPackAsset.Length)
        {
            if (sAssetFolders.Length != 0)
            {
                sIsPackAsset = new bool[sAssetFolders.Length];
            }
        }

        sSceneFolders = PackageConfigData.GetSceneFolders();
        sScenePackModes = PackageConfigData.GetScenePackModes();
        sSceneFolderBasePaths = PackageConfigData.GetSceneFolderBasePaths();
        if (sIsPackScene == null || sSceneFolders.Length != sIsPackScene.Length)
        {
            if (sSceneFolders.Length != 0)
            {
                sIsPackScene = new bool[sSceneFolders.Length];
            }
        }

        sAssetServerList.Clear();
        TextAsset tTextAsset = Resources.LoadAssetAtPath<TextAsset>(sPackConfigDataFolder.Substring(sPackConfigDataFolder.IndexOf(@"Assets/")) + @"AssetServers.xml");
        if (tTextAsset != null)
        {
            XmlDocument tDoc = new XmlDocument();
            tDoc.LoadXml(tTextAsset.text);

            sServersPopup = new GUIContent[tDoc.DocumentElement.ChildNodes.Count];

            int i = 0;
            foreach (XmlNode tNode in tDoc.DocumentElement.ChildNodes)
            {
                string tName = tNode.Attributes["name"].Value;
                string tUrl = tNode.Attributes["url"].Value;
                sAssetServerList.Add(tName, tUrl);
                sServersPopup[i++] = new GUIContent(tName);
            }
        }
    }

    static void updatePackDir()
    {
        if (Directory.Exists(@"C:/") || Directory.Exists(@"D:/")) //PC
        {
            sApkPath = @"D:/AOP_V" + sBuildVersion + string.Format(".t{0:MMddHHmm}_", System.DateTime.Now) +
                       EditorUserBuildSettings.androidBuildSubtarget.ToString() + (EditorUserBuildSettings.development ? "_Debug" : "_Release") + @".apk";
            sIpaPath = @"D:/AOP_V" + sBuildVersion + string.Format(".t{0:MMddHHmm}_", System.DateTime.Now) +
                       "PVRTC" + (EditorUserBuildSettings.development ? "_Debug" : "_Release");
        }
        else //MAC
        {
            sApkPath = @"/Users/pengjian/Desktop/BuildVersion/Android/AOP_V" + sBuildVersion + string.Format(".t{0:MMddHHmm}_", System.DateTime.Now) +
                EditorUserBuildSettings.androidBuildSubtarget.ToString() + (EditorUserBuildSettings.development ? "_Debug" : "_Release") + @".apk";
            sIpaPath = @"/Users/pengjian/Desktop/BuildVersion/IPhone/AOP_V" + sBuildVersion + string.Format(".t{0:MMddHHmm}_", System.DateTime.Now) +
                "PVRTC" + (EditorUserBuildSettings.development ? "_Debug" : "_Release");
        }
    }

    static void updatePackDir( string a_rootDir )
    {
        if( string.IsNullOrEmpty(a_rootDir) )
        {
            updatePackDir();
        }
        else
        {
            sApkPath = a_rootDir + @"/AOP_V" + sBuildVersion + string.Format(".t{0:MMddHHmm}_", System.DateTime.Now) +
                    EditorUserBuildSettings.androidBuildSubtarget.ToString() + (EditorUserBuildSettings.development ? "_Debug" : "_Release") + @".apk";
            sIpaPath = a_rootDir + @"/AOP_V" + sBuildVersion + string.Format(".t{0:MMddHHmm}_", System.DateTime.Now) +
                        "PVRTC" + (EditorUserBuildSettings.development ? "_Debug" : "_Release");
        }
    }

    void Update()
    {
        updatePackDir();
        if( sPackSelected )
        {
            bool tSelected = false;
            for (int i = 0; i < sAssetFolders.Length; i++)
            {
                if( sIsPackAsset[i] )
                {
                    PackAssetFolder(Application.dataPath + sAssetFolderBasePaths[i] + sAssetFolders[i], sAssetPackModes[i]);
                    tSelected = true;
                }
            }
            for (int i = 0; i < sSceneFolders.Length; i++)
            {
                if( sIsPackScene[i] )
                {
                    PackSceneFolder(Application.dataPath + sSceneFolderBasePaths[i] + sSceneFolders[i], sScenePackModes[i]);
                    tSelected = true;
                }
            }
            sPackSelected = false;
            if( tSelected )
            {
                ShowNotification( new GUIContent("Done package selected!") );
            }
            else
            {
                ShowNotification( new GUIContent("You didn't select any package!") );
            }
        }
        else if( sPackUI )
        {
            RunPackCmd(sPackConfigDataFolder + @"PackageCmdUI.txt");
            sPackUI = false;
            ShowNotification(new GUIContent("Done package ui asset bundles!"));
        }
        else if( sPackAllAssetBundles )
        {
            RunPackCmd(sPackConfigDataFolder + @"PackageCmdAllAssetBundle.txt");
            sPackAllAssetBundles = false;
            ShowNotification(new GUIContent("Done package all asset bundles!"));
        }
        else if( sPackAllScenes )
        {
            for (int i = 0; i < sSceneFolders.Length; i++)
            {
                PackSceneFolder(Application.dataPath + sSceneFolderBasePaths[i] + sSceneFolders[i], sScenePackModes[i]);
            }
            sPackAllScenes = false;
            ShowNotification(new GUIContent("Done package all scenes!"));
        }
        else if( sPackAll )
        {
            RunPackCmd(sPackConfigDataFolder + @"PackageCmdAllAssetBundle.txt");
            for (int i = 0; i < sSceneFolders.Length; i++)
            {
                PackSceneFolder(Application.dataPath + sSceneFolderBasePaths[i] + sSceneFolders[i], sScenePackModes[i]);
            }
            sPackAll = false;
            ShowNotification( new GUIContent( "Done package all!") );
        }
        else if( sPackPrimePack )
        {
            CleanUpAssetBunldes();
            CleanUpSceneFiles();
            RunPackCmd(sPackConfigDataFolder + @"PackageCmdPrimePack.txt");
            sPackPrimePack = false;
            ShowNotification(new GUIContent("Done package prime pack!"));
        }
        else if (sRunPackCmd)
        {
            RunPackCmd(sPackConfigDataFolder + @"PackageCmdCustom.txt");
            ShowNotification(new GUIContent("Done running package command!"));
        }
    }

    void VersionTool( int a_windowId )
    {
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Clean Assetbundles", GUILayout.Width(116), GUILayout.Height(19));
        sCleanAssetBundles = EditorGUILayout.Toggle(sCleanAssetBundles, GUILayout.Width(15), GUILayout.Height(15));
        EditorGUILayout.LabelField("   Clean Scenes", GUILayout.Width(94), GUILayout.Height(19));
        sCleanScenes = EditorGUILayout.Toggle(sCleanScenes, GUILayout.Width(15), GUILayout.Height(15));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Version Number:", GUILayout.Width(105), GUILayout.Height(15) );
        sBuildVersion = EditorGUILayout.TextField(sBuildVersion, GUILayout.Width(78), GUILayout.Height(15));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Build Assetbundles", GUILayout.Width(116), GUILayout.Height(19));
        sBuildAssetBundles = EditorGUILayout.Toggle(sBuildAssetBundles, GUILayout.Width(15), GUILayout.Height(15));
        EditorGUILayout.LabelField("   Build Scenes", GUILayout.Width(94), GUILayout.Height(19));
        sBuildScenes = EditorGUILayout.Toggle(sBuildScenes, GUILayout.Width(15), GUILayout.Height(15));
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Do Build", GUILayout.Width(55), GUILayout.Height(19));
        sDoBuild = EditorGUILayout.Toggle(sDoBuild, GUILayout.Width(15), GUILayout.Height(15));
        GUILayout.EndHorizontal();

        if (sAssetServerList.Count > 0 && sServersPopup.Length > 0)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Asset Server:", GUILayout.Width(90), GUILayout.Height(15));
            sAssetServerSelected = EditorGUILayout.Popup(sAssetServerSelected, sServersPopup, GUILayout.Width(108));
            if (GUILayout.Button("Set", GUILayout.Width(48), GUILayout.Height(14)))
            {
                setAssetServer();
                ShowNotification(new GUIContent("Done!"));
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Server Url:", GUILayout.Width(90), GUILayout.Height(15));
            EditorGUILayout.SelectableLabel(sAssetServerList[sServersPopup[sAssetServerSelected].text], GUILayout.Width(408), GUILayout.Height(16));
            GUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("APK Path :", GUILayout.Width(65), GUILayout.Height(20));
        EditorGUILayout.SelectableLabel(sApkPath, GUILayout.Width(340), GUILayout.Height(20));
        EditorGUILayout.Space();
        sBuildApk = GUILayout.Button("Build APK", GUILayout.Width(80), GUILayout.Height(18));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("IPA Path :", GUILayout.Width(65), GUILayout.Height(20));
        EditorGUILayout.SelectableLabel(sIpaPath, GUILayout.Width(340), GUILayout.Height(20));
        EditorGUILayout.Space();
        sBuildIpa = GUILayout.Button("Build IPA", GUILayout.Width(80), GUILayout.Height(18));
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if( sBuildApk || sBuildIpa )
        {
            int[] tVersionCode = UnityToolKit.toIntArray(sBuildVersion, '.');
			
            if (sBuildVersion.Length == 0 || sBuildVersion.Contains(" ") || tVersionCode.Length != 3)
            {
                ShowNotification( new GUIContent("Input version number first!") );
            }
            else
            {
                PackageConfigData.LoadPackConfigure();// use the pack config file in case user changed the pack tool.
                updatePackConfig();

                PlayerSettings.bundleVersion = sBuildVersion;
                if (sBuildApk)
                {
                    PlayerSettings.Android.bundleVersionCode = tVersionCode[0] * 1000000 + tVersionCode[1] * 1000 + tVersionCode[2];
                }
                else
                {
                    PlayerSettings.shortBundleVersion = sBuildVersion;  // with out build number.
                }
                sBuildTarget = sBuildApk ? BuildTarget.Android : BuildTarget.iPhone;

                if (sCleanScenes)
                {
                    EditorUtility.DisplayProgressBar("Clean Up", "Cleaning up scenes, please wait...", 0.0f);
                    CleanUpSceneFiles();
                    EditorUtility.ClearProgressBar();
                }
                if (sCleanAssetBundles)
                {
                    EditorUtility.DisplayProgressBar("Clean Up", "Cleaning up asset bundles, please wait...", 0.0f);
                    CleanUpAssetBunldes();
                    EditorUtility.ClearProgressBar();
                }

                EditorUtility.DisplayProgressBar("Set server", "Setting asset server, please wait...", 0.0f);
                setAssetServer();
                EditorUtility.ClearProgressBar();

                if (sBuildAssetBundles)
                {
                    EditorUtility.DisplayProgressBar("Export Scene", "Exporting scene, please wait...", 0.0f);
                    SceneParserTool.exportAllScene();
                    EditorUtility.ClearProgressBar();
                    AssetDatabase.Refresh();
                    RunPackCmd(sPackConfigDataFolder + @"PackageCmdAllAssetBundle.txt");
                }

                if (sBuildScenes)
                {
                    EditorUtility.DisplayProgressBar("Optimize Scene", "Optimizing scene, please wait...", 0.0f);
                    SceneOptimizeTool.HandleOptimize(SceneOptimizeTool.EOptimizeScope.AllScene, true, true, true, true);
                    EditorUtility.ClearProgressBar();
                    for (int i = 0; i < sSceneFolders.Length; i++)
                    {
                        PackSceneFolder(Application.dataPath + sSceneFolderBasePaths[i] + sSceneFolders[i], sScenePackModes[i]);
                    }
                }

                CleanUpMd5Files();
                UpdateMd5File();
                if (sDoBuild)
                {
                    BuildVersion();
                    ShowNotification( new GUIContent("Done build version.") );
                }
                else
                {
                    ShowNotification(new GUIContent("Done build resource."));
                }
            }
            sBuildApk = sBuildIpa = false;
        }
    }

    static void setAssetServer()
    {
        if (sAssetServerSelected >= 0 && sAssetServerSelected < sAssetServerList.Count)
        {
            AssetDatabase.DeleteAsset(@"Assets/AssetPipleLine/Resources/AssetServer.xml");

            XmlDocument tDoc = new XmlDocument();
            XmlElement tRoot = tDoc.CreateElement("AssetServer");
            tDoc.AppendChild(tRoot);

            XmlElement tName = tDoc.CreateElement("name");
            tName.SetAttribute("value", sServersPopup[sAssetServerSelected].text);
            tRoot.AppendChild(tName);

            XmlElement tUrl = tDoc.CreateElement("url");
            tUrl.SetAttribute("value", sAssetServerList[sServersPopup[sAssetServerSelected].text]);
            tRoot.AppendChild(tUrl);
            tDoc.Save(Application.dataPath + @"/AssetPipleLine/Resources/AssetServer.xml");
            AssetDatabase.Refresh();
        }
    }

    static bool IsBuiltInScene( string a_sceneFile )
    {
        string tScene = a_sceneFile.Substring(a_sceneFile.LastIndexOf('/') + 1);
        foreach( string a_scene in sBuiltInScenes )
        {
            if (a_scene.CompareTo(tScene) == 0)
            {
                return true;
            }
        }
        return false;
    }

    static void RunPackCmd( string a_cmdFile)
    {
        if (!File.Exists(a_cmdFile))
        {
            Debug.LogError("Pack cmd file not found! " + a_cmdFile);
            return;
        }

        bool tIsStreamScene = false;
        bool tIsIndepend = false;
        PackageMode tPackMode = PackageMode.SinglePack;

        List<string> tPackedList = new List<string>();
        string[] tCmds = File.ReadAllLines(a_cmdFile);
        foreach(string tCmd in tCmds)
        {
            tCmd.TrimStart(new char[]{' '});
            tCmd.TrimEnd(new char[]{' '});
            if (string.IsNullOrEmpty(tCmd) || tCmd.StartsWith(@"//"))
            {
                continue;
            }
            if (string.Equals(tCmd, "Push"))
            {
                BuildPipeline.PushAssetDependencies();
                continue;
            }
            if (string.Equals(tCmd, "Pop"))
            {
                BuildPipeline.PopAssetDependencies();
                continue;
            }
            if (string.Equals(tCmd, "Set SinglePackMode"))
            {
                tPackMode = PackageMode.SinglePack;
                continue;
            }
            if (string.Equals(tCmd, "Set MultiPackMode"))
            {
                tPackMode = PackageMode.MultiPack;
                continue;
            }
            if (string.Equals(tCmd, "Set PackIndepend"))
            {
                tIsIndepend = true;
                continue;
            }
            if (string.Equals(tCmd, "Set PackIndependFalse"))
            {
                tIsIndepend = false;
                continue;
            }
            if (string.Equals(tCmd, "Set TargetAssetBundle"))
            {
                tIsStreamScene = false;
                continue;
            }
            if (string.Equals(tCmd, "Set TargetStreamScene"))
            {
                tIsStreamScene = true;
                continue;
            }
            if (string.Equals(tCmd, "PackRest"))
            {
                if (!tIsStreamScene)
                {
                    for (int i = 0, imax = sAssetFolders.Length; i < imax; i++)
                    {
                        if (!tPackedList.Contains(sAssetFolders[i].Substring(sAssetFolders[i].LastIndexOf('/') + 1)))
                        {
                            PackAssetFolder(Application.dataPath + sAssetFolderBasePaths[i] + sAssetFolders[i], sAssetPackModes[i], tIsIndepend);
                        }
                    }
                }
                break;
            }
            if (tIsStreamScene)
            {
                PackSceneFolder(Application.dataPath + PackageConfigData.GetSceneFolderPath(tCmd), tPackMode, tIsIndepend);
            }
            else
            {
                PackAssetFolder(Application.dataPath + PackageConfigData.GetAssetFolderPath(tCmd), tPackMode, tIsIndepend); 
            }
            tPackedList.Add(tCmd);
        }
    }

    static void PackAssetFolder( string a_folder, PackageMode a_packageMode, bool a_independ = false )
    {
        if (sForceReImport)
        {
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(a_folder.Substring(a_folder.IndexOf(@"Assets/")), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            //AssetDatabase.Refresh();
        }

        if (!Directory.Exists(a_folder))
        {
            return;
        }

        List<Object> tPackageObjects = new List<Object>();
        string[] tFileList = Directory.GetFiles(a_folder, "*.*", SearchOption.AllDirectories);


        foreach (string tFile in tFileList)
        {
            if (tFile.EndsWith(".prefab") || tFile.EndsWith(".png") || tFile.EndsWith(".jpg") || tFile.EndsWith(".psd") || tFile.EndsWith(".tga") || tFile.EndsWith(".bytes")
                || tFile.EndsWith(".xml") || tFile.EndsWith(".mat") || tFile.EndsWith(".cs") || tFile.EndsWith(".anim") || tFile.EndsWith(".controller"))
            {
                Object tObj = AssetDatabase.LoadMainAssetAtPath(tFile.Substring(tFile.IndexOf(@"Assets/")).Replace("\\", "/"));
                if (a_packageMode == PackageMode.MultiPack)
                {
                    if (a_independ)
                    {
                        BuildPipeline.PushAssetDependencies();
                    }
                    string tFileWithOutExt = sPackagePath + tFile.Substring(0, tFile.LastIndexOf('.')).Substring(tFile.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
                    BuildPipeline.BuildAssetBundle(tObj, null, tFileWithOutExt + @".assetbundle", sBuildAssetBundleOption, sBuildTarget);
                    if (a_independ)
                    {
                        BuildPipeline.PopAssetDependencies();
                    }
                    if (sCompressPackage)
                    {
                        EditorUtility.DisplayProgressBar("Compressing bundle file, please hold on.", "Compressing " + tFileWithOutExt + @".assetbundle", 0);
                        CompressTool.CompressLZ4File(tFileWithOutExt + @".assetbundle", tFileWithOutExt + @"_tmp.assetbundle");
                        File.Delete(tFileWithOutExt + @".assetbundle");
                        File.Move(tFileWithOutExt + @"_tmp.assetbundle", tFileWithOutExt + @".assetbundle");
                        System.GC.Collect();
                        EditorUtility.ClearProgressBar();
                    }
                    if (a_independ)
                    {
                        BuildPipeline.PopAssetDependencies();
                    }
                }
                else
                {
                    tPackageObjects.Add(tObj);
                }
            }
            else if( !tFile.EndsWith(".meta"))
            {
                Debug.LogError(tFile + " is not supported for packing.");
            }
        }
        if( a_packageMode == PackageMode.SinglePack && tPackageObjects.Count != 0 )
        {
            if (a_independ)
            {
                BuildPipeline.PushAssetDependencies();
            }
            string tFileWithOutExt = sPackagePath + a_folder.Substring(a_folder.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            BuildPipeline.BuildAssetBundle( null, tPackageObjects.ToArray(), tFileWithOutExt + @".assetbundle", sBuildAssetBundleOption, sBuildTarget);
            if (a_independ)
            {
                BuildPipeline.PopAssetDependencies();
            }
            if (sCompressPackage)
            {
                EditorUtility.DisplayProgressBar("Compressing bundle file, please hold on.", "Compressing " + tFileWithOutExt + @".assetbundle", 0);
                CompressTool.CompressLZ4File(tFileWithOutExt + @".assetbundle", tFileWithOutExt + @"_tmp.assetbundle");
                File.Delete(tFileWithOutExt + @".assetbundle");
                File.Move(tFileWithOutExt + @"_tmp.assetbundle", tFileWithOutExt + @".assetbundle");
                System.GC.Collect();
                EditorUtility.ClearProgressBar();
            }
        }
        AssetDatabase.Refresh();
    }

    static void PackSceneFolder(string a_folder, PackageMode a_packageMode, bool a_independ = false)
    {
        if (sForceReImport)
        {
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(a_folder, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
            AssetDatabase.Refresh();
        }

        if (!Directory.Exists(a_folder))
        {
            return;
        }

        string[] tSceneArray = Directory.GetFiles(a_folder, "*.unity", SearchOption.TopDirectoryOnly);

        if (tSceneArray.Length == 0)
        {
            return;
        }
        
        for (int i = 0; i < tSceneArray.Length; i++ )
        {
            tSceneArray[i] = tSceneArray[i].Substring(tSceneArray[i].LastIndexOf(@"Assets/")).Replace("\\", "/");
        }

        if (a_packageMode == PackageMode.MultiPack)
        {
            foreach( string tScene in tSceneArray )
            {
                if (a_independ)
                {
                    BuildPipeline.PushAssetDependencies();
                }
                string tFileWithOutExt = tScene.Substring(tScene.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
                tFileWithOutExt = tFileWithOutExt.Substring(0, tFileWithOutExt.LastIndexOf('.'));
                string tError = BuildPipeline.BuildStreamedSceneAssetBundle(new string[]{ tScene }, sPackagePath + tFileWithOutExt + @".unity3d",
                                                                            sBuildTarget, BuildOptions.BuildAdditionalStreamedScenes | BuildOptions.UncompressedAssetBundle);
                if (a_independ)
                {
                    BuildPipeline.PopAssetDependencies();
                }

                if (!string.IsNullOrEmpty(tError))
                {
                    Debug.LogError(tError);
                    continue;
                }
                AssetDatabase.Refresh();
                if (sCompressPackage)
                {
                    EditorUtility.DisplayProgressBar("Compressing bundle file, please hold on.", "Compressing " + sPackagePath + tFileWithOutExt + @".unity3d", 0);
                    CompressTool.CompressLZ4File(sPackagePath + tFileWithOutExt + @".unity3d", sPackagePath + tFileWithOutExt + @"_tmp.unity3d");
                    File.Delete(sPackagePath + tFileWithOutExt + @".unity3d");
                    File.Move(sPackagePath + tFileWithOutExt + @"_tmp.unity3d", sPackagePath + tFileWithOutExt + @".unity3d");
                    System.GC.Collect();
                    EditorUtility.ClearProgressBar();
                }
                System.GC.Collect();
            }
        }
        else
        {
            if (a_independ)
            {
                BuildPipeline.PushAssetDependencies();
            }
            string tFileWithOutExt = a_folder.Substring(a_folder.LastIndexOf('/') + 1);
            string tError = BuildPipeline.BuildStreamedSceneAssetBundle(tSceneArray, sPackagePath + tFileWithOutExt + @".unity3d",
                                                        sBuildTarget, BuildOptions.BuildAdditionalStreamedScenes | BuildOptions.UncompressedAssetBundle);
            if (a_independ)
            {
                BuildPipeline.PopAssetDependencies();
            }
            if (!string.IsNullOrEmpty(tError))
            {
                Debug.LogError(tError);
                return;
            }
            AssetDatabase.Refresh();
            if (sCompressPackage)
            {
                EditorUtility.DisplayProgressBar("Compressing bundle file, please hold on.", "Compressing " + sPackagePath + tFileWithOutExt + @".unity3d", 0);
                CompressTool.CompressLZ4File(sPackagePath + tFileWithOutExt + @".unity3d", sPackagePath + tFileWithOutExt + @"_tmp.unity3d");
                File.Delete(sPackagePath + tFileWithOutExt + @".unity3d");
                File.Move(sPackagePath + tFileWithOutExt + @"_tmp.unity3d", sPackagePath + tFileWithOutExt + @".unity3d");
                System.GC.Collect();
                EditorUtility.ClearProgressBar();
            }
            System.GC.Collect();
        }

        AssetDatabase.Refresh();
    }


    // to do : support setAssetServer in jenkins.
    static void BuildVersionCmd()
    {
        AssetDatabase.Refresh();

        List<string> tScenes = new List<string>();
        foreach (EditorBuildSettingsScene tScene in EditorBuildSettings.scenes)
        {
            if (IsBuiltInScene(tScene.path))
            {
                tScenes.Add(tScene.path);
            }
        }

        BuildOptions tBuildOption = BuildOptions.None;
        if (EditorUserBuildSettings.development)
        {
            tBuildOption |= BuildOptions.Development;
        }
        if (EditorUserBuildSettings.connectProfiler)
        {
            tBuildOption |= BuildOptions.ConnectWithProfiler;
        }
        if (EditorUserBuildSettings.allowDebugging)
        {
            tBuildOption |= BuildOptions.AllowDebugging;
        }

        bool tBuildAssetBundles = true;
        bool tBuildScenes = true;
        bool tPrimePack = true;
        sBuildTarget = BuildTarget.Android;

        string[] tArgs = System.Environment.GetCommandLineArgs();
        string tRootDir = null;
        for( int i = 0; i < tArgs.Length; i++ )
        {
            if (tArgs[i].Contains("-packConfigFile"))
            {
                if (tArgs[i + 1].EndsWith(".xml"))
                {
                    PackageConfigData.SetPackConfigFile(Application.dataPath + tArgs[i + 1]);
                }
                continue;
            }
            if (tArgs[i].Contains("-addScriptDefine"))
            {
                UnityToolKit.addScriptDefine(tArgs[i + 1], sBuildTarget == BuildTarget.Android ? BuildTargetGroup.Android : BuildTargetGroup.iPhone);
                continue;
            }
            if( tArgs[i].Contains("-buildTargetCus") )
            {
                if (tArgs[i + 1].Equals("android"))
                {
                    sBuildTarget = BuildTarget.Android;
                }
                else if (tArgs[i + 1].Equals("iphone"))
                {
                    sBuildTarget = BuildTarget.iPhone;
                }
                continue;
            }
            if (tArgs[i].Contains("-buildAssetBundles"))
            {
                if (tArgs[i + 1].Equals("false", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    tBuildAssetBundles = false;
                }
                continue;
            }
            if (tArgs[i].Contains("-buildScenes"))
            {
                if (tArgs[i + 1].Equals("false", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    tBuildScenes = false;
                }
                continue;
            }
            if (tArgs[i].Contains("-primePack"))
            {
                if (tArgs[i + 1].Equals("false", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    tPrimePack = false;
                }
                continue;
            }
            if (tArgs[i].Contains("-version"))
            {
                sBuildVersion = tArgs[i + 1];
                PlayerSettings.bundleVersion = sBuildVersion;
                int[] tVersionCode = UnityToolKit.toIntArray(sBuildVersion, '.');
                PlayerSettings.Android.bundleVersionCode = tVersionCode[0] * 1000000 + tVersionCode[1] * 1000 + tVersionCode[2];
                continue;
            }
            if (tArgs[i].Contains("-packDir"))
            {
                tRootDir = tArgs[i+1];
                continue;
            }
            if (tArgs[i].Contains("-texFmt"))
            {
                AndroidBuildSubtarget[] tFmts = (AndroidBuildSubtarget[])System.Enum.GetValues( typeof(AndroidBuildSubtarget) );
                foreach (AndroidBuildSubtarget tTargetFmt in tFmts)
                {
                    if( string.Compare(tArgs[i+1], tTargetFmt.ToString(), true) == 0 )
                    {
                        EditorUserBuildSettings.androidBuildSubtarget = tTargetFmt;
                        break;
                    }
                }
            }
        }

        PackageConfigData.LoadPackConfigure();
        updatePackConfig();

        if (tBuildAssetBundles)
        {
            CleanUpAssetBunldes();

            if (tPrimePack)
            {
                RunPackCmd(sPackConfigDataFolder + @"PackageCmdPrimePack.txt");
            }
            else
            {
                if(tBuildScenes)
                {
                    EditorUtility.DisplayProgressBar("Export Scene", "Exporting scene, please wait...", 0.0f);
                    SceneParserTool.exportAllScene();
                    EditorUtility.DisplayProgressBar("Optimize Scene", "Optimizing scene, please wait...", 0.0f);
                    SceneOptimizeTool.HandleOptimize(SceneOptimizeTool.EOptimizeScope.AllScene, true, true, true, true);
                    EditorUtility.ClearProgressBar();
                }
                RunPackCmd(sPackConfigDataFolder + @"PackageCmdAllAssetBundle.txt");
            }
        }

        if (tBuildScenes)
        {
            CleanUpSceneFiles();
            if (!tPrimePack)
            {
                for (int i = 0; i < sSceneFolders.Length; i++)
                {
                    PackSceneFolder(Application.dataPath + sSceneFolderBasePaths[i] + sSceneFolders[i], sScenePackModes[i]);
                }
            }
        }

        CleanUpMd5Files();
        UpdateMd5File();

        updatePackDir(tRootDir);
        string tBuildPath = (sBuildTarget == BuildTarget.Android ? sApkPath : sIpaPath);

        string tError = BuildPipeline.BuildPlayer(tScenes.ToArray(), tBuildPath, sBuildTarget, tBuildOption);

        if (tError.Length != 0)
        {
            Debug.LogError(tError);
            UnityEditor.EditorApplication.Exit(1);
        }
        AssetDatabase.Refresh();
    }

    static void BuildVersion()
    {
        AssetDatabase.Refresh();

        List<string> tScenes = new List<string>();
        foreach (EditorBuildSettingsScene tScene in EditorBuildSettings.scenes)
        {
            if (IsBuiltInScene(tScene.path) && !tScenes.Contains(tScene.path))
            {
                tScenes.Add(tScene.path);
            }
        }

        BuildOptions tBuildOption = BuildOptions.None;
        if ( EditorUserBuildSettings.development )
        {
            tBuildOption |= BuildOptions.Development;
        }
        if (EditorUserBuildSettings.connectProfiler)
        {
            tBuildOption |= BuildOptions.ConnectWithProfiler;
        }
        if (EditorUserBuildSettings.allowDebugging)
        {
            tBuildOption |= BuildOptions.AllowDebugging;
        }
        string tBuildPath = ( sBuildTarget == BuildTarget.Android ? sApkPath: sIpaPath );
        string tError = BuildPipeline.BuildPlayer(tScenes.ToArray(), tBuildPath, sBuildTarget, tBuildOption);
        if (tError.Length != 0)
        {
            Debug.LogError(tError);
        }
        AssetDatabase.Refresh();
    }

    static void CleanUpAssetBunldes()
    {
        string[] tAssets = Directory.GetFiles(sPackagePath, "*.assetbundl*", SearchOption.AllDirectories);
        foreach( string tAsset in tAssets )
        {
            AssetDatabase.DeleteAsset(tAsset.Substring(tAsset.IndexOf("Assets")));
        }
        AssetDatabase.Refresh();
    }

    static void CleanUpSceneFiles()
    {
        string[] tAssets = Directory.GetFiles(sPackagePath, "*.unity3*", SearchOption.AllDirectories);
        foreach (string tAsset in tAssets)
        {
            AssetDatabase.DeleteAsset(tAsset.Substring(tAsset.IndexOf("Assets")));
        }
        AssetDatabase.Refresh();
    }

    void Md5Tool( int a_windowId )
    {
        GUILayout.BeginVertical();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        EditorGUILayout.Space();
        sUpdateMd5File = GUILayout.Button("Update Full Md5", GUILayout.Width(114), GUILayout.Height(20));
        sCheckMd5File = GUILayout.Button("Check Full Md5", GUILayout.Width(114), GUILayout.Height(20));
        GUILayout.EndHorizontal();
        EditorGUILayout.Space();
        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Md5 Path :", GUILayout.Width(65), GUILayout.Height(20));
        EditorGUILayout.SelectableLabel(sPackagePath + @"AssetBundleMd5List.xml", GUILayout.Width(430), GUILayout.Height(26));
        EditorGUILayout.Space();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        if( sUpdateMd5File )
        {
            if (sBuildVersion.Length == 0 || sBuildVersion.Contains(" "))
            {
                ShowNotification(new GUIContent("Input build version number first!"));
            }
            else
            {
                ShowNotification(new GUIContent("Updating full md5 file...Please wait..."));
                UpdateMd5File();
                ShowNotification(new GUIContent("Full md5 file updated!"));
            }
            sUpdateMd5File = false;
        }
        if( sCheckMd5File )
        {
            if (File.Exists(sPackagePath + @"AssetBundleMd5List.xml"))
            {
                System.Diagnostics.Process.Start("iexplore.exe", sPackagePath + @"AssetBundleMd5List.xml");
                sCheckMd5File = false;
            }
            else
            {
                ShowNotification(new GUIContent("MD5 file not found!"));
            }
        }
    }

    static void UpdateMd5File()
    {
        Dictionary<string, long> tFileSizeList;
        Dictionary<string, string> tMd5List = GenerateMd5List(out tFileSizeList);

        EditorUtility.DisplayProgressBar("Generate md5 file", sPackagePath + @"AssetBundleMd5List.xml", 0);

        bool tIsUpdate = false;
        XmlDocument tOldDoc = null;
        if (File.Exists(sPackagePath + @"AssetBundleMd5List.xml"))
        {
            tOldDoc = new XmlDocument();
            tOldDoc.Load(sPackagePath + @"AssetBundleMd5List.xml");
            tIsUpdate = true;
        }

        XmlDocument tNewDoc = new XmlDocument();

        XmlElement tRoot = tNewDoc.CreateElement("root");
        tRoot.SetAttribute("publishversion", sBuildVersion);
        tNewDoc.AppendChild(tRoot);

        XmlElement tAssetBundleRoot = tNewDoc.CreateElement("assetbundles");
        tRoot.AppendChild(tAssetBundleRoot);

        XmlElement tUnity3dRoot = tNewDoc.CreateElement("unity3ds");
        tRoot.AppendChild(tUnity3dRoot);

        foreach (KeyValuePair<string, string> tPair in tMd5List)
        {
            bool tIsAssetBundle = tPair.Key.EndsWith(@".assetbundle"); // asset bundle or unity3d ?
            XmlElement tXE = tNewDoc.CreateElement( tIsAssetBundle ? "assetbundle" : "unity3d");
            tXE.SetAttribute("name", tPair.Key);
            tXE.SetAttribute("md5", tPair.Value);
            foreach(KeyValuePair<string, long> tPair2 in tFileSizeList)
            {
                if( tPair2.Key.CompareTo(tPair.Key) == 0)
                {
                    tXE.SetAttribute("size", tPair2.Value.ToString());
                    break;
                }
            }
            int tVersion = 1;
            if( tIsUpdate )
            {
                foreach (XmlElement a_xe in tOldDoc.FirstChild.ChildNodes)
                {
                    if (0 == string.Compare(a_xe.GetAttribute("name"), tPair.Key))
                    {
                        Debug.Log(a_xe.GetAttribute("version"));
                        tVersion = int.Parse(a_xe.GetAttribute("version"));
                        if (0 != string.Compare(a_xe.GetAttribute("md5"), tPair.Value))
                        {
                            tVersion++;
                        }
                        break;
                    }
                }
            }
            tXE.SetAttribute("version", tVersion.ToString());
            if( tIsAssetBundle )
            {
                tAssetBundleRoot.AppendChild(tXE);
            }
            else
            {
                tUnity3dRoot.AppendChild(tXE);
            }
        }

        string tMd5FileName = @"AssetBundleMd5List.xml";
        EditorUtility.DisplayProgressBar("Generate md5 file", sPackagePath + tMd5FileName, 1);

        if( tIsUpdate )
        {
            tOldDoc.Save(sPackagePath + tMd5FileName);
            RenameOldMd5File();
        }
        tNewDoc.Save(sPackagePath + tMd5FileName);

        System.Text.Encoding tUtf8NoBom = new System.Text.UTF8Encoding(false);
        string tFileStr = File.ReadAllText(sPackagePath + tMd5FileName, tUtf8NoBom);
        File.WriteAllText(sPackagePath + tMd5FileName, tFileStr, tUtf8NoBom);

        EditorUtility.ClearProgressBar();
    }

    static Dictionary<string, string> GenerateMd5List(out Dictionary<string, long> a_fileSizeList )
    {
        a_fileSizeList = new Dictionary<string, long>();
        Dictionary<string, string> tMd5List = new Dictionary<string, string>();
        MD5CryptoServiceProvider tMd5Generator = new MD5CryptoServiceProvider();

        int tProgress = 1;
        string[] tBundleFiles = Directory.GetFiles(sPackagePath, "*.assetbundle", SearchOption.AllDirectories);
        string[] tSceneFiles = Directory.GetFiles(sPackagePath, "*.unity3d", SearchOption.AllDirectories);
        string[] tAssetFiles = new string[tBundleFiles.Length + tSceneFiles.Length];
        System.Array.Copy(tBundleFiles, tAssetFiles, tBundleFiles.Length);
        System.Array.Copy(tSceneFiles, 0, tAssetFiles, tBundleFiles.Length, tSceneFiles.Length);
        
        foreach (string tAsset in tAssetFiles)
        {
            string tAssetBundleName = Path.GetFileName( tAsset );

            EditorUtility.DisplayProgressBar("Calculate md5", string.Format(@"{0} ({1} of {2})", tAssetBundleName, tProgress, tAssetFiles.Length), (float)tProgress / tAssetFiles.Length);
            tProgress++;

            FileStream tAssetBundle = new FileStream( tAsset, FileMode.Open, FileAccess.Read );
            string tAssetBundleMD5  = System.BitConverter.ToString( tMd5Generator.ComputeHash(tAssetBundle) );

            if ( tMd5List.ContainsKey( tAssetBundleName ) )
            {
                Debug.LogError("Asset bundle is not unique. " + tAssetBundleName);
            }
            else
            {
                a_fileSizeList.Add(tAssetBundleName, tAssetBundle.Length);
                tMd5List.Add( tAssetBundleName, tAssetBundleMD5 );
            }
            tAssetBundle.Close();
        }
        EditorUtility.ClearProgressBar();
        return tMd5List;
    }

    static void RenameOldMd5File()
    {
        string tFile = @"AssetBundleMd5List";
        string tOldMd5File = sPackagePath + string.Format(tFile + @"_{0:MMddHHmmss}.xml", System.DateTime.Now);
        File.Move(sPackagePath + tFile + @".xml", tOldMd5File);
    }

    static void CleanUpMd5Files()
    {
        string[] tAssets = Directory.GetFiles(sPackagePath, "AssetBundleMd5Lis*", SearchOption.AllDirectories);
        foreach (string tAsset in tAssets)
        {
            AssetDatabase.DeleteAsset(tAsset.Substring(tAsset.IndexOf("Assets")));
        }
    }
}
