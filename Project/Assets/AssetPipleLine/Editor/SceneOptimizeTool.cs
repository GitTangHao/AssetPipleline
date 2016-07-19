using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Security.Cryptography;


public class SceneOptimizeTool : EditorWindow
{
    public enum EOptimizeScope
    {
        CurrentScene = 0,
        AllScene,
    }

    public static readonly string sCopyDir = @"Assets/TempPackFolder/";

    static EOptimizeScope sOptimizeScope = EOptimizeScope.CurrentScene;

    static bool sOptimizeLight = false;
    static bool sOptimizeAnimator = false;
    static bool sRemoveArtNode = false;
    static bool sAll = false;

    static bool sCopy = true;

    [MenuItem("AssetPipleLine/SceneOptimizeTool")]
    static void GetTargetPackageFiles()
    {
        EditorWindow.GetWindowWithRect<SceneOptimizeTool>(new Rect(0, 0, 280, 135), false, "Optimize Tool", true);
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.Space();

        sOptimizeScope = (EOptimizeScope)EditorGUILayout.EnumPopup("Optimize Scope :", sOptimizeScope, GUILayout.Width(260), GUILayout.Height(20));

        sCopy = GUILayout.Toggle(sCopy, new GUIContent("Copy Original"));
        if (sCopy)
        {
            EditorGUILayout.SelectableLabel(@"OutputDir: " + sCopyDir);
        }

        EditorGUILayout.BeginHorizontal();
        sOptimizeLight = GUILayout.Button("Optimize Light", GUILayout.Width(132), GUILayout.Height(22));
        sOptimizeAnimator = GUILayout.Button("Optimize Animator", GUILayout.Width(132), GUILayout.Height(22));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        sRemoveArtNode = GUILayout.Button("Remove Art Node", GUILayout.Width(132), GUILayout.Height(22));
        sAll = GUILayout.Button("All", GUILayout.Width(132), GUILayout.Height(22));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        if (sAll)
        {
            sOptimizeLight = sOptimizeAnimator = sRemoveArtNode = true;
        }
        if (sOptimizeLight || sOptimizeAnimator || sRemoveArtNode)
        {
            ShowNotification(new GUIContent("Please wait..."));
            HandleOptimize(sOptimizeScope, sOptimizeLight, sOptimizeAnimator, sRemoveArtNode, sCopy);
            ShowNotification(new GUIContent("Done!"));
        }
    }

    //optimize scene, if a_copy set to true, this will copy an origin scene to copy path and optimized it.
    public static void HandleOptimize(EOptimizeScope a_scope, bool a_optimizeLight, bool a_optimizeAnimator, bool a_optimizeArtNode, bool a_copy)
    {
        if (a_copy)
        {
            AssetDatabase.DeleteAsset(sCopyDir.Remove(sCopyDir.Length-1));
            AssetDatabase.CreateFolder(@"Assets", @"TempPackFolder");
            AssetDatabase.Refresh();
        }

        if (a_scope == EOptimizeScope.CurrentScene)
        {
            if (string.IsNullOrEmpty(EditorApplication.currentScene))
            {
                Debug.LogError("You didn't open any scene.");
                return;
            }
            if (a_copy)
            {
                string tCopyPath = Path.Combine(sCopyDir, EditorApplication.currentScene.Substring(EditorApplication.currentScene.LastIndexOf("/")));
                AssetDatabase.CopyAsset(EditorApplication.currentScene, tCopyPath);
                EditorApplication.OpenScene(tCopyPath);
            }
            OptimizeCurrentScene(a_optimizeLight, a_optimizeAnimator, a_optimizeArtNode);
            EditorApplication.SaveScene();
        }
        else
        {
            SceneParserData.loadParserSceneInfo();
            string[] tSceneFiles = SceneParserData.getParserScenes();
            if (a_copy)
            {
                for (int i = 0, imax = tSceneFiles.Length; i < imax; i++)
                {
                    AssetDatabase.CopyAsset("Assets" + SceneParserData.getSceneFileBasePaths()[i] + tSceneFiles[i], sCopyDir + tSceneFiles[i]);
                }
                AssetDatabase.Refresh();    //it's important to refresh database when you create new assets in unity.
                for (int i = 0, imax = tSceneFiles.Length; i < imax; i++)
                {
                    EditorApplication.OpenScene(sCopyDir + tSceneFiles[i]);
                    OptimizeCurrentScene(a_optimizeLight, a_optimizeAnimator, a_optimizeArtNode);
                    EditorApplication.SaveScene(sCopyDir + tSceneFiles[i]);
                }
            }
            else
            {
                for (int i = 0, imax = tSceneFiles.Length; i < imax; i++)
                {
                    EditorApplication.OpenScene("Assets" + SceneParserData.getSceneFileBasePaths()[i] + tSceneFiles[i]);
                    OptimizeCurrentScene(a_optimizeLight, a_optimizeAnimator, a_optimizeArtNode);
                    EditorApplication.SaveScene("Assets" + SceneParserData.getSceneFileBasePaths()[i] + tSceneFiles[i]);
                }
            }
            AssetDatabase.Refresh();
        }
    }

    static void OptimizeCurrentScene(bool a_optimizeLight, bool a_optimizeAnimator, bool a_optimizeArtNode)
    {
        if (a_optimizeArtNode)
        {
            RemoveArtNodes();
        }

        if (a_optimizeLight)
        {
            OptimizeLight();
        }

        if (a_optimizeAnimator)
        {
            OptimizeAnimator();
        }
    }

    static void OptimizeLight()
    {
        GameObject[] tObjs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject tObj in tObjs)
        {
            List<Light> tLights = new List<Light>();
            UnityToolKit.getComponentsRecursive<Light>(tObj.transform, tLights);
            foreach (Light tLight in tLights)
            {
                tLight.enabled = false;
            }
        }
    }

    static void OptimizeAnimator()
    {
        GameObject[] tObjs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject tObj in tObjs)
        {
            List<Animator> tAnimators = new List<Animator>();
            UnityToolKit.getComponentsRecursive<Animator>(tObj.transform, tAnimators);
            foreach (Animator tAnimator in tAnimators)
            {
                tAnimator.cullingMode = AnimatorCullingMode.BasedOnRenderers;
            }

            List<Animation> tAnimations = new List<Animation>();
            UnityToolKit.getComponentsRecursive<Animation>(tObj.transform, tAnimations);
            foreach (Animation tAnimation in tAnimations)
            {
                tAnimation.cullingType = AnimationCullingType.BasedOnRenderers;
            }
        }
    }

    static void RemoveArtNodes()
    {
        UnityToolKit.removeSceneNode(SceneParserData.sStaticObjectRoot);
        UnityToolKit.removeSceneNode(SceneParserData.sDynamicObjectRoot);
        UnityToolKit.removeSceneNode(SceneParserData.sLightObjectRoot);
        UnityToolKit.removeSceneNode(SceneParserData.sLightProbesRoot);
        RenderSettings.skybox = null;
    }
}
