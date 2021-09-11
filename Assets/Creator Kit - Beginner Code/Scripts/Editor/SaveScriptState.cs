using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using UnityEngine.UI;

[CustomEditor(typeof(MonoScript))]
public class SaveScriptState : Editor
{
    struct BackupData
    {
        public string date;
        public string time;

        public string filePath;
    }

    MonoScript m_Script;
    
    const int kMaxChars = 7000;
    GUIStyle m_TextStyle;

    Vector2 m_ScrollPos;
    BackupData[] m_BackupFiles;
    string m_BackupFolder;
    
    string targetTitle { get {
        if (targets.Length == 1)
            return target.name;
        else
            return targets.Length + " " + ObjectNames.NicifyVariableName (ObjectNames.GetClassName (target)) + "s";
    } }

    void OnEnable()
    {
        m_BackupFolder = Application.dataPath + "/../Library/ScriptBackup";
        
        m_BackupFiles = new BackupData[0];
        m_Script = target as MonoScript;
        m_ScrollPos = Vector2.zero;
        RefreshBackupList();
    }

    void RefreshBackupList()
    {
        if (!Directory.Exists(m_BackupFolder))
            return;
        
        var files = Directory.GetFiles(m_BackupFolder, $"{target.name}*");
        m_BackupFiles = new BackupData[files.Length];

        for(int i = 0; i < files.Length; ++i)
        {
            string[] infos = Path.GetFileNameWithoutExtension(files[i]).Split('_');

            if (infos.Length < 3)
            {
                Debug.LogError("");
            }
            
            m_BackupFiles[i].time = infos[infos.Length - 1];
            m_BackupFiles[i].date = infos[infos.Length - 2];
            m_BackupFiles[i].filePath = files[i];
        }
    }
		
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Save State"))
        {
            
            var date = System.DateTime.Now;
            string filename = target.name + "_" + date.Year + "." + date.Month + "." + date.Day + "_" + date.Hour + "." + date.Minute + ".cs";

            if (!Directory.Exists(m_BackupFolder))
                Directory.CreateDirectory(m_BackupFolder);
            
            File.WriteAllText(m_BackupFolder + "/" + filename, m_Script.text);
            RefreshBackupList();
        }

        m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos);

        for (int i = 0; i < m_BackupFiles.Length; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{m_BackupFiles[i].date} at {m_BackupFiles[i].time}");
            
            if (GUILayout.Button("Restore"))
            {
                string path = AssetDatabase.GetAssetPath(m_Script);
                string fullPath = path.Replace("Assets", Application.dataPath);
                System.IO.File.WriteAllText(fullPath, File.ReadAllText(m_BackupFiles[i].filePath));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            if (GUILayout.Button("Remove"))
            {
                File.Delete(m_BackupFiles[i].filePath);
                ArrayUtility.RemoveAt(ref m_BackupFiles, i);
                i--;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        GUILayout.EndScrollView();
        
        if (m_TextStyle == null)
            m_TextStyle = "ScriptText";
			
        bool enabledTemp = GUI.enabled;
        GUI.enabled = true;
        TextAsset textAsset = target as TextAsset;
        if (textAsset != null)
        {
            string text;
            if (targets.Length > 1)
            {
                text = targetTitle;
            }
            else
            {
                text = textAsset.ToString();
                if (text.Length > kMaxChars)
                    text = text.Substring (0, kMaxChars) + "...\n\n<...etc...>";
            }
            
            Rect rect = GUILayoutUtility.GetRect (EditorGUIUtility.TrTempContent(text), m_TextStyle);

            GUI.Box (rect, text, m_TextStyle);
        }
        GUI.enabled = enabledTemp;
    }
}
