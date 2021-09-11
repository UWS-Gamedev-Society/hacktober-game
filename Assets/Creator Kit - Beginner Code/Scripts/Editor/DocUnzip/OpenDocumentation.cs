using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.IO.Compression;

public class OpenDocumentation
{
    [MenuItem("Beginner Code/Open Documentation", priority = 100)]
    static void OpenDoc()
    {
        CheckExistingDocumentation();
        
        Uri helpUri = new Uri(Application.dataPath + "/../Library/CreatorKitDoc/index.html");
        Help.BrowseURL(helpUri.AbsoluteUri);
    }

    static void CheckExistingDocumentation()
    {
        var docZipID = AssetDatabase.FindAssets("CreatorKitDocumentation");
        if (docZipID.Length == 0)
        {
            Debug.LogError("Couldn't find the documentation zip");
            return;
        }

        string path = Application.dataPath + "/../Library/CreatorKitDoc";
        var docZipPath = AssetDatabase.GUIDToAssetPath(docZipID[0]).Replace("Assets", Application.dataPath);

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            if(Directory.GetCreationTime(path) < File.GetLastWriteTime(docZipPath))
            {//newest doc, update
                Directory.Delete(path, true);
                Directory.CreateDirectory(path);
            }
        }

        //empty dir. uncompress the documentation
        if (Directory.GetFiles(path).Length == 0)
        {
            ZipFile.ExtractToDirectory(docZipPath, path);
            Debug.Log("Documentation was decompressed to the Library folder");
        } 
    }
}
