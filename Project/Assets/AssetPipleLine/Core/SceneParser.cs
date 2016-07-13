using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneParser
{
	public static void GenerateSceneByXml(byte[] a_xmlContent, GameObject a_staticObjsParent, GameObject a_dynamicObjsParent)
    {
        Stream tXmlStream = new MemoryStream(a_xmlContent);
        XmlDocument tDoc = new XmlDocument();
        tDoc.Load(tXmlStream);
        tXmlStream.Close();

        XmlNodeList tStaticNodeList = tDoc.SelectNodes("/scene/staticSceneObjects/gameObject");
        XmlNodeList tDynamicNodeList = tDoc.SelectNodes("/scene/dynamicSceneObjects/gameObject");

        List<GameObject> tBatchingObjs = null;

        foreach (XmlNode tStaticNode in tStaticNodeList)
        {
            ParseGameObjectNode(tStaticNode, a_staticObjsParent, out tBatchingObjs);
        }
        foreach (XmlNode tDynamicNode in tDynamicNodeList)
        {
            ParseGameObjectNode(tDynamicNode, a_dynamicObjsParent, out tBatchingObjs);
        }

        StaticBatchingUtility.Combine(a_staticObjsParent);
        // do static batching combine many times, only the last time calling will be valid. A unity bug ???
        //StaticBatchingUtility.Combine(tBatchingObjs.ToArray(), a_dynamicObjsParent); 

        tDoc = null;
    }

    static void ParseGameObjectNode(XmlNode a_node, GameObject a_parentObj, out List<GameObject> a_batchingStaticObjs)
    {
        a_batchingStaticObjs = new List<GameObject>();
        GameObject tPrefObj = ResourceManager.getObject<GameObject>(a_node.Attributes["objectAsset"].Value, new string[]{"common.assetbundle", "base.assetbundle", "space.assetbundle", "island.assetbundle"});
#if UNITY_EDITOR
        if (tPrefObj == null)
        {
            Debug.LogError("miss node : " + a_node.Attributes["objectAsset"].Value);
            return;
        }
#endif
        GameObject tGameObj = (GameObject)GameObject.Instantiate(tPrefObj);

        if (a_parentObj != null)
        {
            tGameObj.transform.SetParent(a_parentObj.transform);
        }

        tGameObj.name = a_node.Attributes["objectName"].Value;

        XmlNode tPosNode = a_node.SelectSingleNode("descendant::position");
        XmlNode tRotNode = a_node.SelectSingleNode("descendant::rotation");
        XmlNode tScaNode = a_node.SelectSingleNode("descendant::scale");

        tGameObj.transform.position = new Vector3(float.Parse(tPosNode.Attributes["x"].Value), float.Parse(tPosNode.Attributes["y"].Value), float.Parse(tPosNode.Attributes["z"].Value));
        tGameObj.transform.rotation = Quaternion.Euler(new Vector3(float.Parse(tRotNode.Attributes["x"].Value), float.Parse(tRotNode.Attributes["y"].Value), float.Parse(tRotNode.Attributes["z"].Value)));
        tGameObj.transform.localScale = new Vector3(float.Parse(tScaNode.Attributes["x"].Value), float.Parse(tScaNode.Attributes["y"].Value), float.Parse(tScaNode.Attributes["z"].Value));

        XmlNode tRenderersNode = a_node.SelectSingleNode("descendant::renderers");
        int tRendererCount = int.Parse(tRenderersNode.Attributes["count"].Value);
        if (tRendererCount > 0)
        {
            List<Renderer> tRenderers = new List<Renderer>();
            UnityToolKit.getComponentsRecursive<Renderer>(tGameObj.transform, tRenderers);

            for (int i = 0, imax = tRenderersNode.ChildNodes.Count; i < imax; i++)
            {
#if UNITY_EDITOR
                if (tRenderersNode.ChildNodes.Count != tRenderers.Count)
                {
                    Debug.LogError("error node: " + a_node.Attributes["objectAsset"].Value + " pos: " + tGameObj.transform.position);
                    return;
                }
#endif
                tRenderers[i].lightmapIndex = int.Parse(tRenderersNode.ChildNodes[i].Attributes["lightmapIndex"].Value);
                Vector4 tTilingOffset = Vector4.zero;
                tTilingOffset.x = float.Parse(tRenderersNode.ChildNodes[i].Attributes["tilingX"].Value);
                tTilingOffset.y = float.Parse(tRenderersNode.ChildNodes[i].Attributes["tilingY"].Value);
                tTilingOffset.z = float.Parse(tRenderersNode.ChildNodes[i].Attributes["x"].Value);
                tTilingOffset.w = float.Parse(tRenderersNode.ChildNodes[i].Attributes["y"].Value);
                tRenderers[i].lightmapTilingOffset = tTilingOffset;
                if (bool.Parse(tRenderersNode.ChildNodes[i].Attributes["batchingStatic"].Value))
                {
                    if (!a_batchingStaticObjs.Contains(tRenderers[i].gameObject))
                    {
                        a_batchingStaticObjs.Add(tRenderers[i].gameObject);
                    }
                }
            }
        }
    }
	
#if UNITY_EDITOR
	public static void ExportSceneToXml( string a_xmlPath, string a_sceneName)
    {
        XmlDocument tDocument = new XmlDocument();

        XmlDeclaration xmlDeclaration = tDocument.CreateXmlDeclaration("1.0", "utf-8", null);
        tDocument.AppendChild(xmlDeclaration);

        XmlElement tSceneElement = tDocument.CreateElement("scene");
        tSceneElement.SetAttribute("sceneName", a_sceneName);
        tDocument.AppendChild(tSceneElement);

        XmlElement tStaticObjectsElement = tDocument.CreateElement("staticSceneObjects");
        tSceneElement.AppendChild(tStaticObjectsElement);

        XmlElement tDynamicObjectsElement = tDocument.CreateElement("dynamicSceneObjects");
        tSceneElement.AppendChild(tDynamicObjectsElement);

        GameObject tStaticObjectParent = GameObject.Find(SceneParserData.sStaticObjectRoot);
        GameObject tDynamicObjectParent = GameObject.Find(SceneParserData.sDynamicObjectRoot);

        Object[] tSceneObjects = Object.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject tSceneObject in tSceneObjects)
        {
            if (tSceneObject.activeSelf && UnityEditor.PrefabUtility.GetPrefabType(tSceneObject) == UnityEditor.PrefabType.PrefabInstance)
            {
                XmlElement tObjectRootElement = null;
                if (tSceneObject.transform.parent == tStaticObjectParent.transform)
                {
                    tObjectRootElement = tStaticObjectsElement;
                }
                else if (tSceneObject.transform.parent == tDynamicObjectParent.transform)
                {
                    tObjectRootElement = tDynamicObjectsElement;
                }
                else
                {
                    continue;
                }

                Object tPrefabObject = UnityEditor.PrefabUtility.GetPrefabParent(tSceneObject);
                if (tPrefabObject != null)
                {
                    XmlElement tPositionElement = tDocument.CreateElement("position");
                    tPositionElement.SetAttribute("x", tSceneObject.transform.position.x.ToString());
                    tPositionElement.SetAttribute("y", tSceneObject.transform.position.y.ToString());
                    tPositionElement.SetAttribute("z", tSceneObject.transform.position.z.ToString());

                    XmlElement tRotationElement = tDocument.CreateElement("rotation");
                    tRotationElement.SetAttribute("x", tSceneObject.transform.rotation.eulerAngles.x.ToString());
                    tRotationElement.SetAttribute("y", tSceneObject.transform.rotation.eulerAngles.y.ToString());
                    tRotationElement.SetAttribute("z", tSceneObject.transform.rotation.eulerAngles.z.ToString());

                    XmlElement tScaleElement = tDocument.CreateElement("scale");
                    tScaleElement.SetAttribute("x", tSceneObject.transform.localScale.x.ToString());
                    tScaleElement.SetAttribute("y", tSceneObject.transform.localScale.y.ToString());
                    tScaleElement.SetAttribute("z", tSceneObject.transform.localScale.z.ToString());

                    XmlElement tTransformElement = tDocument.CreateElement("transform");
                    tTransformElement.AppendChild(tPositionElement);
                    tTransformElement.AppendChild(tRotationElement);
                    tTransformElement.AppendChild(tScaleElement);

                    List<Renderer> tRenderers = new List<Renderer>();
                    UnityToolKit.getComponentsRecursive<Renderer>(tSceneObject.transform, tRenderers);

                    XmlElement tRenderersElement = tDocument.CreateElement("renderers");
                    tRenderersElement.SetAttribute("count", tRenderers.Count.ToString());

                    foreach (Renderer tRenderer in tRenderers)
                    {
                        XmlElement tRendererElement = tDocument.CreateElement("renderer");
                        tRendererElement.SetAttribute("name", tRenderer.name);

                        tRendererElement.SetAttribute("lightmapIndex", tRenderer.lightmapIndex.ToString());
                        tRendererElement.SetAttribute("tilingX", tRenderer.lightmapTilingOffset.x.ToString());
                        tRendererElement.SetAttribute("tilingY", tRenderer.lightmapTilingOffset.y.ToString());
                        tRendererElement.SetAttribute("x", tRenderer.lightmapTilingOffset.z.ToString());
                        tRendererElement.SetAttribute("y", tRenderer.lightmapTilingOffset.w.ToString());

                        bool tIsBatchingStatic = UnityEditor.GameObjectUtility.AreStaticEditorFlagsSet(tRenderer.gameObject, UnityEditor.StaticEditorFlags.BatchingStatic);
                        tRendererElement.SetAttribute("batchingStatic", tIsBatchingStatic.ToString());

                        tRenderersElement.AppendChild(tRendererElement);
                    }

                    XmlElement tGameObjectElement = tDocument.CreateElement("gameObject");
                    tGameObjectElement.SetAttribute("objectName", tSceneObject.name);
                    tGameObjectElement.SetAttribute("objectAsset", tPrefabObject.name);
                    tGameObjectElement.AppendChild(tTransformElement);
                    tGameObjectElement.AppendChild(tRenderersElement);

                    tObjectRootElement.AppendChild(tGameObjectElement);
                }
            }
        }
        tDocument.Save(a_xmlPath);

        AssetDatabase.Refresh();
        string tFolder = a_xmlPath.Substring(a_xmlPath.IndexOf(@"Assets/"));
        tFolder = tFolder.Substring(0, tFolder.LastIndexOfAny(new char[]{'/', '\\'}));
        AssetDatabase.ImportAsset(a_xmlPath.Substring(a_xmlPath.IndexOf(@"Assets/")), ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
	}

#endif

}
