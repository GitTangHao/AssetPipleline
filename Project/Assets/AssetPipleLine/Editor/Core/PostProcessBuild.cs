using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System.Collections.Generic;
using System.IO;


public class PostProcessBuild 
{
    static readonly string sIOSNativeCodePath = Application.dataPath + "/AssetPipleLine/Native/iOS";

    [PostProcessBuildAttribute(1)]
    public static void OnPostprocessBuild(BuildTarget a_buildTarget, string a_pathToBuiltProject)
    {
        if (BuildTarget.iPhone == a_buildTarget)
        {
            if(Directory.Exists(sIOSNativeCodePath))
            {
                List<string> tFiles = new List<string>();
                tFiles.AddRange(Directory.GetFiles(sIOSNativeCodePath, "*.h", SearchOption.AllDirectories));
                tFiles.AddRange(Directory.GetFiles(sIOSNativeCodePath, "*.mm", SearchOption.AllDirectories));
                foreach (string a_file in tFiles)
                {
                    string tFileName = a_file.Substring(a_file.LastIndexOfAny(new char[] { '/', '\\' }) + 1);
                    File.Copy(a_file, a_pathToBuiltProject + @"/Classes/" + tFileName, true);
                }
            }
        }

        //Debug.Log(a_pathToBuiltProject);
    }
}
