using System;
using System.Collections.Generic;
using CreatorKitCode;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CreatorKitCode 
{
    /// <summary>
    /// Class that handle all the SFX. Through its functions you can play a SFX of a given type at a given position.
    /// It use pooling to pre-create all the source and recycle them for efficiency reason.
    /// </summary>
    public class SFXManager : MonoBehaviour
    {
        //one use for now
        public enum Use
        {
            Player,
            Enemies,
            WorldSound,
            Sound2D
        }

        /// <summary>
        /// Store all data used to play a sound. The pitch will be picked randomly between PitchMin and PitchMax.
        /// </summary>
        public class PlayData
        {
            public AudioClip Clip;
            public Vector3 Position = Vector3.zero;

            public float PitchMin = 1.0f;
            public float PitchMax = 1.0f;

            public float Volume = 1.0f;
        }
    
        static SFXManager Instance { get; set; }

        public AudioListener Listener;
        public Transform ListenerTarget;

        [Header("Defaults")]
        public AudioClip[] DefaultSwingSound;
        public AudioClip[] DefaultHitSound;
        public AudioClip DefaultItemUsedSound;
        public AudioClip DefaultItemEquipedSound;
        public AudioClip DefaultPickupSound;
    
        public static AudioClip ItemUsedSound => Instance.DefaultItemUsedSound;
        public static AudioClip ItemEquippedSound => Instance.DefaultItemEquipedSound;
        public static AudioClip PickupSound => Instance.DefaultPickupSound;
    
        [SerializeField]
        AudioSource[] m_Prefabs;
        [SerializeField]
        int[] m_PoolAmount;
    
        Queue<AudioSource>[] m_Instances;

        void Awake()
        {
            Instance = this;
            m_Instances = new Queue<AudioSource>[m_Prefabs.Length];
            for (int i = 0; i < m_Prefabs.Length; ++i)
            {
                m_Instances[i] = new Queue<AudioSource>();

                for (int k = 0; k < m_PoolAmount[i]; ++k)
                {
                    var audioSource = Instantiate(m_Prefabs[i]);

                    m_Instances[i].Enqueue(audioSource);
                }
            }
        }

        void Reset()
        {
            m_Prefabs = new AudioSource[Enum.GetValues(typeof(Use)).Length];
            m_PoolAmount = new int[m_Prefabs.Length];
        }

        void Update()
        {
            Listener.transform.position = ListenerTarget.transform.position;
        }

        /// <summary>
        /// Get a source of the given type. You will rarely call this directly and instead use PlaySound.
        /// </summary>
        /// <param name="useType">The type of sound (map to a specific mixer)</param>
        /// <returns>The AudioSource at the front of the current pool queue for the given type</returns>
        public static AudioSource GetSource(Use useType)
        {
            var s = Instance.m_Instances[(int)useType].Dequeue();
            Instance.m_Instances[(int)useType].Enqueue(s);

            return s;
        }

        /// <summary>
        /// Play a sound of the given type using the info in the given PlayData. This will take care of retrieving an
        /// AudioSource of the given type
        /// </summary>
        /// <param name="useType">The type of sound (map to a specific mixer)</param>
        /// <param name="data">The PlayData that contains all the data of the sound to play (clip, volume, position etc.)</param>
        public static void PlaySound(Use useType, PlayData data)
        {
            var source = GetSource(useType);

            source.clip = data.Clip;
            source.gameObject.transform.position = data.Position;
            source.pitch = Random.Range(data.PitchMin, data.PitchMax);
            source.volume = data.Volume;
        
            source.Play();
        }

        public static AudioClip GetDefaultSwingSound()
        {
            var clipArray = Instance.DefaultSwingSound;

            return clipArray[Random.Range(0, clipArray.Length)];
        }
    
        public static AudioClip GetDefaultHit()
        {
            var clipArray = Instance.DefaultHitSound;

            return clipArray[Random.Range(0, clipArray.Length)];
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SFXManager))]
public class SFXManagerEditor : Editor
{
    SerializedProperty m_PrefabsArrayProp;
    SerializedProperty m_PoolAmountProp;
    SerializedProperty m_ListenerProp;
    SerializedProperty m_ListenerTargetProp;
    SerializedProperty m_DefaultSwingSoundProp;
    SerializedProperty m_DefaultHitSoundProp;
    SerializedProperty m_DefaultItemUsedSound;
    SerializedProperty m_DefaultItemEquippedSound;
    SerializedProperty m_DefaultPickupSoundProp;
    
    void OnEnable()
    {
        m_PrefabsArrayProp = serializedObject.FindProperty("m_Prefabs");
        m_PoolAmountProp = serializedObject.FindProperty("m_PoolAmount");

        m_ListenerProp = serializedObject.FindProperty(nameof(SFXManager.Listener));
        m_ListenerTargetProp = serializedObject.FindProperty(nameof(SFXManager.ListenerTarget));

        m_DefaultSwingSoundProp = serializedObject.FindProperty(nameof(SFXManager.DefaultSwingSound));
        m_DefaultHitSoundProp = serializedObject.FindProperty(nameof(SFXManager.DefaultHitSound));
        m_DefaultItemUsedSound = serializedObject.FindProperty(nameof(SFXManager.DefaultItemUsedSound));
        m_DefaultItemEquippedSound = serializedObject.FindProperty(nameof(SFXManager.DefaultItemEquipedSound));
        m_DefaultPickupSoundProp = serializedObject.FindProperty(nameof(SFXManager.DefaultPickupSound));
        
        int useSize = Enum.GetValues(typeof(SFXManager.Use)).Length;
        if (m_PrefabsArrayProp.arraySize != useSize)
            m_PrefabsArrayProp.arraySize = useSize;
        if (m_PoolAmountProp.arraySize != useSize)
            m_PoolAmountProp.arraySize = useSize;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.LabelField("Listener Info");
        EditorGUILayout.PropertyField(m_ListenerProp);
        EditorGUILayout.PropertyField(m_ListenerTargetProp);

        EditorGUILayout.PropertyField(m_DefaultSwingSoundProp, true);
        EditorGUILayout.PropertyField(m_DefaultHitSoundProp, true);
        EditorGUILayout.PropertyField(m_DefaultItemUsedSound);
        EditorGUILayout.PropertyField(m_DefaultItemEquippedSound);
        EditorGUILayout.PropertyField(m_DefaultPickupSoundProp);
        
        EditorGUILayout.LabelField("Prefab Per Use");

        float saveWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 128.0f;
        for (int i = 0; i < m_PrefabsArrayProp.arraySize; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(m_PrefabsArrayProp.GetArrayElementAtIndex(i), new GUIContent(((SFXManager.Use)i).ToString()));
            EditorGUILayout.PropertyField(m_PoolAmountProp.GetArrayElementAtIndex(i), new GUIContent("Pool Size"));
            EditorGUILayout.EndHorizontal();
        }
        EditorGUIUtility.labelWidth = saveWidth;
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif
