using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnityToolKit
{
    public static void getComponentsRecursive<T>(Transform a_transform, List<T> a_componentList)
    {
        object[] tComponents = a_transform.GetComponents(typeof(T));
        if (tComponents != null)
        {
            for (int i = 0, imax = tComponents.Length; i < imax; i++)
            {
                a_componentList.Add((T)tComponents[i]);
            }
        }
        for (int i = 0, imax = a_transform.childCount; i < imax; i++)
        {
            getComponentsRecursive<T>(a_transform.GetChild(i), a_componentList);
        }
    }

    public static void setLayerRecursive(Transform a_transform, int a_layer)
    {
        a_transform.gameObject.layer = a_layer;
        for (int i = 0, imax = a_transform.childCount; i < imax; i++ )
        {
            setLayerRecursive(a_transform.GetChild(i), a_layer);
        }
    }

    //note : this function will not work on inactive object on the scene root.
    public static void removeSceneNode(string a_nodePath)
    {
        GameObject tObj = GameObject.Find(a_nodePath);
        if (tObj != null)
        {
            GameObject.DestroyImmediate(tObj);
        }
        else
        {
            string a_rootPath = a_nodePath.Substring(0, a_nodePath.IndexOfAny(new char[]{'/', '\\'}));
            tObj = GameObject.Find(a_rootPath);
            if (tObj != null)
            {
                Transform tTrans = tObj.transform.Find(a_nodePath.Substring(a_nodePath.IndexOfAny(new char[] { '/', '\\' }) + 1));
                if (tTrans != null)
                {
                    GameObject.DestroyImmediate(tTrans.gameObject);
                }
            }
        }
    }

    public static bool quickStringEqual(string a_str1, string a_str2)
    {
        if (a_str1.Length != a_str2.Length)
        {
            return false;
        }
        if (string.ReferenceEquals(a_str1, a_str2))
        {
            return true;
        }
        return string.Equals(a_str1, a_str2);
    }

#if UNITY_EDITOR
    public static void addScriptDefine(string a_scriptDefine, BuildTargetGroup a_buildTargeGroup)
    {
        string tScriptDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(a_buildTargeGroup);
        if (!string.IsNullOrEmpty(tScriptDefines))
        {
            tScriptDefines.TrimEnd(new char[]{' ', ';'});
            if (tScriptDefines.Contains(a_scriptDefine))
            {
                return;
            }
            tScriptDefines += ";";
        }
        tScriptDefines += a_scriptDefine;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(a_buildTargeGroup, tScriptDefines);
    }

#endif
}
