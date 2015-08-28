using System;
using System.IO;
using UnityEngine;
using UnityEditor;

public static class WriteToTextAsset {

    private static string projectRootPath {
        get { return Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length); }
    }

    public static void Write(TextAsset target, string text) {
        if (target == null)
            throw new Exception("Target text asset is null! Can't write!");
        if (text == null)
            throw new Exception("Input text is null! Can't write!");

        string path = AssetDatabase.GetAssetPath(target);

        File.WriteAllText(projectRootPath + path, text);
    }

}
