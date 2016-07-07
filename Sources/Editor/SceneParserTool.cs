using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;


public class SceneParserTool : EditorWindow
{
    static Vector2 sSceneScroll;
    static string[] sSceneFiles = null;
    static bool[] sIsExport = null;

    [MenuItem("Custom/SceneParserTool")]
    static void OpenSceneParserTool()
    {
        SceneParserData.loadParserSceneInfo();
        EditorWindow.GetWindowWithRect<SceneParserTool>(new Rect(0, 0, 680, 420), false, "Scene Parser Tool", true);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene Parser Tool");
        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Scene Parser Config File : " + SceneParserData.getDataFile(), GUILayout.Width(608));
        if (GUILayout.Button("Set", GUILayout.Width(64), GUILayout.Height(15)))
        {
            setConfigDataFile();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Output directory : " + SceneParserData.getOutputDir(), GUILayout.Width(608));
        if (GUILayout.Button("Set", GUILayout.Width(64), GUILayout.Height(15)))
        {
            setOutputDir();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Stream Scene Folder", GUILayout.Width(240));
        EditorGUILayout.LabelField("Check Path", GUILayout.Width(170));
        EditorGUILayout.LabelField("Remove Folder", GUILayout.Width(170));
        EditorGUILayout.LabelField("Export", GUILayout.Width(150));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("----------------------------------------------------------------------------------------------------------------------------------");
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        sSceneScroll = EditorGUILayout.BeginScrollView(sSceneScroll, GUILayout.Width(660), GUILayout.Height(200));
        sSceneFiles = SceneParserData.getParserScenes();
        if (sIsExport == null || sIsExport.Length != sSceneFiles.Length)
        {
            sIsExport = new bool[sSceneFiles.Length];
        }
        for (int i = 0; i < sSceneFiles.Length; i++)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(sSceneFiles[i], GUILayout.Width(200), GUILayout.Height(20));
            EditorGUILayout.LabelField(" ", GUILayout.Width(30), GUILayout.Height(14));
            if (GUILayout.Button("Check Path", GUILayout.Width(84), GUILayout.Height(14)))
            {
                Debug.Log(Application.dataPath + SceneParserData.getSceneFileBasePaths()[i] + sSceneFiles[i]);
            }
            EditorGUILayout.LabelField(" ", GUILayout.Width(83), GUILayout.Height(14));
            if (GUILayout.Button("Remove Folder", GUILayout.Width(100), GUILayout.Height(14)))
            {
                SceneParserData.removeParserScene(sSceneFiles[i]);
                ShowNotification(new GUIContent("Done!"));
            }
            EditorGUILayout.LabelField(" ", GUILayout.Width(85), GUILayout.Height(14));
            sIsExport[i] = EditorGUILayout.Toggle("", sIsExport[i], GUILayout.Width(14), GUILayout.Height(14));
            EditorGUILayout.Space();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.LabelField("----------------------------------------------------------------------------------------------------------------------------------");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Stream Scene File", GUILayout.Width(160), GUILayout.Height(26)))
        {
            addSceneFile();
        }
        if (GUILayout.Button("Add Stream Scene Folder", GUILayout.Width(160), GUILayout.Height(26)))
        {
            addSceneFiles();
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Export Selected", GUILayout.Width(160), GUILayout.Height(26)))
        {
            exportSelected();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Reload Scene Config", GUILayout.Width(160), GUILayout.Height(26)))
        {
            SceneParserData.loadParserSceneInfo();
            ShowNotification(new GUIContent("Done!"));
        }
        if (GUILayout.Button("Save Scene Config", GUILayout.Width(160), GUILayout.Height(26)))
        {
            SceneParserData.saveParserSceneInfo();
            ShowNotification(new GUIContent("Done!"));
        }
        EditorGUILayout.Space();
        if (GUILayout.Button("Export All", GUILayout.Width(160), GUILayout.Height(26)))
        {
            exportAll();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void setConfigDataFile()
    {
        string tDefaultPath = SceneParserData.getDataFile();
        tDefaultPath = tDefaultPath.Substring(0, tDefaultPath.LastIndexOfAny(new char[] { '/', '\\' }));
        string tFile = EditorUtility.OpenFilePanel("Select an config data file", tDefaultPath, "xml");
        if (!string.IsNullOrEmpty(tFile) && tFile.EndsWith(".xml"))
        {
            SceneParserData.setDataFile(tFile);
            SceneParserData.loadParserSceneInfo();
            ShowNotification(new GUIContent("Done!"));
            return;
        }
        ShowNotification(new GUIContent("Failed!"));
    }

    void setOutputDir()
    {
        string tOutputDir = EditorUtility.OpenFolderPanel("Select an output director", Application.dataPath, "");
        if (!string.IsNullOrEmpty(tOutputDir) && tOutputDir.Contains(Application.dataPath))
        {
            SceneParserData.setOutputDir(tOutputDir);
            ShowNotification(new GUIContent("Done!"));
            return;
        }
        ShowNotification(new GUIContent("Failed!"));
    }

    void addSceneFile()
    {
        string tFile = EditorUtility.OpenFilePanel("Select an scene file", Application.dataPath, "unity");
        if (!string.IsNullOrEmpty(tFile) && tFile.Contains(Application.dataPath) && tFile.EndsWith(".unity"))
        {
            string tBasePath = tFile.Substring(tFile.IndexOf("Assets/") + 6);
            tBasePath = tBasePath.Substring(0, tBasePath.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
            string tFileName = tFile.Substring(tFile.LastIndexOfAny(new char[] { '/', '\\' }) + 1);

            if (SceneParserData.addParserScene(tFileName, tBasePath))
            {
                ShowNotification(new GUIContent("Success!"));
                return;
            }
        }
        ShowNotification(new GUIContent("Failed!"));
    }

    void addSceneFiles()
    {
        string tPath = EditorUtility.OpenFolderPanel("Select an Scene Folder", Application.dataPath, "");
        if (!string.IsNullOrEmpty(tPath) && tPath.Contains(Application.dataPath))
        {
            string[] tFiles = Directory.GetFiles(tPath, "*.unity", SearchOption.AllDirectories);
            for(int i = 0; i < tFiles.Length; i++)
            {
                string tBasePath = tFiles[i].Substring(tFiles[i].IndexOf("Assets/") + 6);
                tBasePath = tBasePath.Substring(0, tBasePath.LastIndexOfAny(new char[]{'/','\\'}) + 1);
                string tFileName = tFiles[i].Substring(tFiles[i].LastIndexOfAny(new char[] { '/', '\\' }) + 1);
                SceneParserData.addParserScene(tFileName, tBasePath);
            }
            ShowNotification(new GUIContent("Done!"));
        }
        ShowNotification(new GUIContent("Failed!"));
    }

    void exportSelected()
    {
        for (int i = 0, imax = sSceneFiles.Length; i < imax; i++)
        {
            if (sIsExport[i])
            {
                EditorApplication.OpenScene("Assets" + SceneParserData.getSceneFileBasePaths()[i] + sSceneFiles[i]);
                SceneParser.ExportSceneToXml(SceneParserData.getOutputDir() + "/" + sSceneFiles[i].Substring(0, sSceneFiles[i].LastIndexOf('.')) + ".xml", EditorApplication.currentScene);
            }
        }
        ShowNotification(new GUIContent("Done!"));
    }

    void exportAll()
    {
        for( int i = 0, imax = sSceneFiles.Length; i < imax; i++)
        {
            EditorApplication.OpenScene("Assets" + SceneParserData.getSceneFileBasePaths()[i] + sSceneFiles[i]);
            SceneParser.ExportSceneToXml(SceneParserData.getOutputDir() + "/" + sSceneFiles[i].Substring(0, sSceneFiles[i].LastIndexOf('.')) + ".xml", EditorApplication.currentScene);
        }
        ShowNotification(new GUIContent("Done!"));
    }

    public static void exportAllScene(string a_dataFile = null)
    {
        if (!string.IsNullOrEmpty(a_dataFile))
        {
            SceneParserData.setDataFile(a_dataFile);
        }
        SceneParserData.loadParserSceneInfo();
        string[] tSceneFiles = SceneParserData.getParserScenes();
        for (int i = 0, imax = tSceneFiles.Length; i < imax; i++)
        {
            EditorApplication.OpenScene("Assets" + SceneParserData.getSceneFileBasePaths()[i] + tSceneFiles[i]);
            SceneParser.ExportSceneToXml(SceneParserData.getOutputDir() + "/" + tSceneFiles[i].Substring(0, tSceneFiles[i].LastIndexOf('.')) + ".xml", EditorApplication.currentScene);
        }
    }
}
