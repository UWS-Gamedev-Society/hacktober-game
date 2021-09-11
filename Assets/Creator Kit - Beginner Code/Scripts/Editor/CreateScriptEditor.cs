using System.IO;
using UnityEngine;
using UnityEditor;
using System.CodeDom.Compiler;

public class CreateScriptEditor
{
    [MenuItem("Beginner Code/Create Item Effect")]
    static void CreateItemEffect()
    {
        var win = ScriptableObject.CreateInstance<NameWindow>();

        win.OnValidate = s =>
        {
            string[] asset = AssetDatabase.FindAssets("SampleItemEffect");

            if (asset.Length > 0)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(asset[0]));

                string result = textAsset.text.Replace("{EFFECTNAME}", s);

                string targetPath = Application.dataPath + "/Scripts/ItemEffect/";
                Directory.CreateDirectory(targetPath);

                targetPath += s + ".cs";
                File.WriteAllText(targetPath, result);
                AssetDatabase.Refresh();

                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(targetPath.Replace(Application.dataPath, "Assets"));
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor"));
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("Couldn't find the sample item effect script file");
            }
        }; 

        win.Display();
    }

    [MenuItem("Beginner Code/Create Weapon Attack Effect")]
    static void CreateWeaponEffect()
    {
        var win = ScriptableObject.CreateInstance<NameWindow>();

        win.OnValidate = s =>
        {
            string[] asset = AssetDatabase.FindAssets("SampleWeaponEffect");

            if (asset.Length > 0)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(asset[0]));

                string result = textAsset.text.Replace("{EFFECTNAME}", s);

                string targetPath = Application.dataPath + "/Scripts/WeaponEffect/";
                Directory.CreateDirectory(targetPath);

                targetPath += s + ".cs";
                File.WriteAllText(targetPath, result);
                AssetDatabase.Refresh();
                
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(targetPath.Replace(Application.dataPath, "Assets"));
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor"));
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("Couldn't find the sample weapon effect script file");
            }
        };
        
        win.Display();
    }
    
    [MenuItem("Beginner Code/Create Equipped Effect")]
    static void CreateEquippedEffect()
    {
        var win = ScriptableObject.CreateInstance<NameWindow>();
        win.OnValidate = s =>
        {
            string[] asset = AssetDatabase.FindAssets("SampleEquipmentEffect");

            if (asset.Length > 0)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(asset[0]));

                string result = textAsset.text.Replace("{EFFECTNAME}", s);

                string targetPath = Application.dataPath + "/Scripts/EquippedEffect/";
                Directory.CreateDirectory(targetPath);

                targetPath += s + ".cs";
                File.WriteAllText(targetPath, result);
                AssetDatabase.Refresh();
                
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(targetPath.Replace(Application.dataPath, "Assets"));
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor"));
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("Couldn't find the sample equipped effect script file");
            }
        };
        
        win.Display();
    }
    
    [MenuItem("Beginner Code/Create Interactable Object")]
    static void CreateInteractableObject()
    {
        var win = ScriptableObject.CreateInstance<NameWindow>();
       
        win.OnValidate = s =>
        {
            string[] asset = AssetDatabase.FindAssets("SampleInteractableObject");

            if (asset.Length > 0)
            {
                var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(asset[0]));

                string result = textAsset.text.Replace("{EFFECTNAME}", s);

                string targetPath = Application.dataPath + "/Scripts/InteractableObjects/";
                Directory.CreateDirectory(targetPath);

                targetPath += s + ".cs";
                File.WriteAllText(targetPath, result);
                AssetDatabase.Refresh();
                
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(targetPath.Replace(Application.dataPath, "Assets"));
                EditorWindow.GetWindow(System.Type.GetType("UnityEditor.ProjectBrowser, UnityEditor"));
                EditorGUIUtility.PingObject(Selection.activeObject);
            }
            else
            {
                Debug.LogError("Couldn't find the sample interactable script file");
            }
        };
        
        win.Display();
    }
}

public class NameWindow : EditorWindow
{
    public System.Action<string> OnValidate;

    string m_EffectName;

    CodeDomProvider _provider;
    
    public void Display()
    {
        var pos = position;
        pos.size = new Vector2(400, 300);
        position = pos;

        m_EffectName = "";

        if (_provider == null)
            _provider = CodeDomProvider.CreateProvider("CSharp");
        
        ShowModalUtility();
    }

    void OnGUI()
    {
        m_EffectName = EditorGUILayout.TextField("Effect Name", m_EffectName);

        bool validName = _provider.IsValidIdentifier(m_EffectName);
        
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = validName;
        if (GUILayout.Button(validName ? "Create" : "Invalid Name"))
        {
            OnValidate(m_EffectName);
            Close();
        }

        GUI.enabled = true;
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        
        EditorGUILayout.EndHorizontal();

        if (!validName)
        {
            EditorGUILayout.HelpBox("The name is not valid. It shouldn't contains space, start with a number or contains special character like ; or .", MessageType.Error);
        }
    }
}