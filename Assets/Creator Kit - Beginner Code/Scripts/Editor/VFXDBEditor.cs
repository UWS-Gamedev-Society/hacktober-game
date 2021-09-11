using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CreatorKitCode;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VFXDatabase))]
public class VFXDBEditor : Editor
{
    SerializedProperty m_EntryProperty;
    bool m_NeedGeneration;
    
    void OnEnable()
    {
        m_EntryProperty = serializedObject.FindProperty(nameof(VFXDatabase.Entries));
        m_NeedGeneration = false;
    }

    void OnDisable()
    {
        if (m_NeedGeneration)
        {
            Debug.Log("RegeneratingEnum");
            RegenerateEnum();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        if (GUILayout.Button("New Entry"))
        {
            m_EntryProperty.InsertArrayElementAtIndex(m_EntryProperty.arraySize);
        }

        int toDelete = -1;
        
        GUILayout.BeginHorizontal();
        GUILayout.Label("Name");
        GUILayout.Label("Prefab");
        GUILayout.Label("PoolSize");
        GUILayout.EndHorizontal();
        
        
        for (int i = 0; i < m_EntryProperty.arraySize; ++i)
        {
            GUILayout.BeginHorizontal();

            SerializedProperty entry = m_EntryProperty.GetArrayElementAtIndex(i);

            SerializedProperty nameProperty = entry.FindPropertyRelative(nameof(VFXDatabase.VFXDBEntry.Name));
            SerializedProperty objRef = entry.FindPropertyRelative(nameof(VFXDatabase.VFXDBEntry.Prefab));
            SerializedProperty poolSizeProp = entry.FindPropertyRelative(nameof(VFXDatabase.VFXDBEntry.PoolSize));

            EditorGUILayout.PropertyField(nameProperty, GUIContent.none);
            EditorGUILayout.PropertyField(objRef, GUIContent.none);
            EditorGUILayout.PropertyField(poolSizeProp, GUIContent.none);

            if (GUILayout.Button("-"))
            {
                toDelete = i;
            }
            
            GUILayout.EndHorizontal();
        }

        if (toDelete != -1)
        {
            m_EntryProperty.DeleteArrayElementAtIndex(toDelete);
        }

        if (serializedObject.hasModifiedProperties)
        {
            m_NeedGeneration = true;
            serializedObject.ApplyModifiedProperties();
        }
    }

    void RegenerateEnum()
    {
        serializedObject.Update();
        
        //first we clean all null entry, as we don't want to generate an identifier for it
        for (int i = 0; i < m_EntryProperty.arraySize; ++i)
        {
            SerializedProperty objRef = m_EntryProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(VFXDatabase.VFXDBEntry.Prefab));
            if (objRef.objectReferenceValue == null)
            {
                m_EntryProperty.DeleteArrayElementAtIndex(i);
            }
        }

        serializedObject.ApplyModifiedProperties();

        //then generate the script file
        string resultingEnum = "public enum VFXType\n{\n";
        for (int i = 0; i < m_EntryProperty.arraySize; ++i)
        {
            SerializedProperty nameProperty = m_EntryProperty.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(VFXDatabase.VFXDBEntry.Name));
            resultingEnum += $"\t{nameProperty.stringValue.Replace(' ', '_')}";

            if (i < m_EntryProperty.arraySize - 1)
                resultingEnum += ",\n";
        }

        resultingEnum += "\n}";

        string[] typeFile = AssetDatabase.FindAssets("t:Script VFXTypes");

        if (typeFile.Length != 1)
        {
            if(typeFile.Length == 0)
                Debug.LogError("You have no VFXTypes.cs file in the project!");
            else
                Debug.LogError("You have more than one VFXTypes.cs files in the project!");
        }
        else
        {
            string path = AssetDatabase.GUIDToAssetPath(typeFile[0]);
            File.WriteAllText(path.Replace("Assets", Application.dataPath), resultingEnum);
            
            AssetDatabase.Refresh();
        }
    }

    [MenuItem("Assets/Create/Beginner Code/VFXDatabase", priority = -800)]
    static void CreateAssetDB()
    {
        var existingDb = AssetDatabase.FindAssets("t:VFXDatabase");
        string selectionPath = "";
        
        if (existingDb.Length > 0)
        {
            Debug.LogError("A VFXDatabase already exists.");
            selectionPath = AssetDatabase.GUIDToAssetPath(existingDb[0]);
        }
        else
        {
            string path = AssetDatabase.GetAssetPath (Selection.activeObject);

            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
            }

            var newDb = CreateInstance<VFXDatabase>();
            var assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/VFXDatabase.asset");
            AssetDatabase.CreateAsset(newDb, assetPath);
            selectionPath = assetPath;
        }
        
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<VFXDatabase>(selectionPath);
    }
}
