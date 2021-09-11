using System.Collections;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreatorKitCode {
    /// <summary>
    /// Describe an usable item. A usable item is an item that can be used in the inventory by double clicking on it.
    /// When it is used, all the stored UsageEffects will be run, allowing to specify what that item does.
    /// (e.g. a AddHealth effect will give health point back to the user)
    /// </summary>
    [CreateAssetMenu(fileName = "UsableItem", menuName = "Beginner Code/Usable Item", order = -999)]
    public class UsableItem : Item
    {
        public abstract class UsageEffect : ScriptableObject
        {
            public string Description;
            //return true if could be used, false otherwise.
            public abstract bool Use(CharacterData user);
        }

        public List<UsageEffect> UsageEffects;

        public override bool UsedBy(CharacterData user)
        {
            bool wasUsed = false;
            foreach (var effect in UsageEffects)
            {
                wasUsed |= effect.Use(user);
            }
        
            return wasUsed;
        }

        public override string GetDescription()
        {
            string description = base.GetDescription();
        
            if(!string.IsNullOrWhiteSpace(description))
                description += "\n";
            else
                description = "";

        
            foreach (var effect in UsageEffects)
            {
                description += effect.Description + "\n";
            }

            return description;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UsableItem))]
public class UsableItemEditor : Editor
{
    UsableItem m_Target;
    
    ItemEditor m_ItemEditor;

    List<string> m_AvailableUsageType;
    SerializedProperty m_UsageEffectListProperty;
    
    void OnEnable()
    {
        m_Target = target as UsableItem;
        m_UsageEffectListProperty = serializedObject.FindProperty(nameof(UsableItem.UsageEffects));
        
        m_ItemEditor = new ItemEditor();
        m_ItemEditor.Init(serializedObject);

        var lookup = typeof(UsableItem.UsageEffect);
        m_AvailableUsageType = System.AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(lookup))
            .Select(type => type.Name)
            .ToList();
    }

    public override void OnInspectorGUI()
    {
        m_ItemEditor.GUI();
        
        int choice = EditorGUILayout.Popup("Add new Effect", -1, m_AvailableUsageType.ToArray());

        if (choice != -1)
        {
            var newInstance = ScriptableObject.CreateInstance(m_AvailableUsageType[choice]);
            
            AssetDatabase.AddObjectToAsset(newInstance, target);
            
            m_UsageEffectListProperty.InsertArrayElementAtIndex(m_UsageEffectListProperty.arraySize);
            m_UsageEffectListProperty.GetArrayElementAtIndex(m_UsageEffectListProperty.arraySize - 1).objectReferenceValue = newInstance;
        }

        Editor ed = null;
        int toDelete = -1;
        for (int i = 0; i < m_UsageEffectListProperty.arraySize; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            var item = m_UsageEffectListProperty.GetArrayElementAtIndex(i);           
            SerializedObject obj = new SerializedObject(item.objectReferenceValue);

            Editor.CreateCachedEditor(item.objectReferenceValue, null, ref ed);
            
            ed.OnInspectorGUI();
            EditorGUILayout.EndVertical();

            if (GUILayout.Button("-", GUILayout.Width(32)))
            {
                toDelete = i;
            }
            EditorGUILayout.EndHorizontal();
        }

        if (toDelete != -1)
        {
            var item = m_UsageEffectListProperty.GetArrayElementAtIndex(toDelete).objectReferenceValue;
            DestroyImmediate(item, true);
            
            //need to do it twice, first time just nullify the entry, second actually remove it.
            m_UsageEffectListProperty.DeleteArrayElementAtIndex(toDelete);
            m_UsageEffectListProperty.DeleteArrayElementAtIndex(toDelete);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
