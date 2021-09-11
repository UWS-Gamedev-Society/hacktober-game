using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CreatorKitCodeInternal 
{
    /// <summary>
    /// Handle all the UI related to damage number appearing above object/character when they get damaged.
    /// Manage the pool of UI text and activating, placing and fading them out across time.
    /// </summary>
    public class DamageUI : MonoBehaviour
    {
        public static DamageUI Instance { get; private set; }
    
        public class ActiveText
        {
            public Text UIText;
            public float MaxTime;
            public float Timer;
            public Vector3 WorldPositionStart;

            public void PlaceText(Camera cam, Canvas canvas)
            {
                float ratio = 1.0f - (Timer / MaxTime);
                Vector3 pos = WorldPositionStart + new Vector3(ratio, Mathf.Sin(ratio * Mathf.PI), 0);
                pos = cam.WorldToScreenPoint(pos);
                //pos *= canvas.scaleFactor;
                pos.z = 0.0f;
            
                UIText.transform.position = pos;
            }
        }
    
        public Text DamageTextPrefab;

        Canvas m_Canvas;
        Queue<Text> m_TextPool = new Queue<Text>();
        List<ActiveText> m_ActiveTexts = new List<ActiveText>();

        Camera m_MainCamera;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            m_Canvas = GetComponent<Canvas>();
        
            const int POOL_SIZE = 64;
            for (int i = 0; i < POOL_SIZE; ++i)
            {
                var t = Instantiate(DamageTextPrefab, m_Canvas.transform);
                t.gameObject.SetActive(false);
                m_TextPool.Enqueue(t);
            }

            m_MainCamera = Camera.main;
        }

        void Update()
        {
            for (int i = 0; i < m_ActiveTexts.Count; ++i)
            {
                var at = m_ActiveTexts[i];
                at.Timer -= Time.deltaTime;

                if (at.Timer <= 0.0f)
                {
                    at.UIText.gameObject.SetActive(false);
                    m_TextPool.Enqueue(at.UIText);
                    m_ActiveTexts.RemoveAt(i);
                    i--;
                }
                else
                {
                    var color = at.UIText.color;
                    color.a = at.Timer / at.MaxTime;
                    at.UIText.color = color;
                    at.PlaceText(m_MainCamera, m_Canvas);
                }
            }
        }

        /// <summary>
        /// Called by the CharacterData system when a new damage is made. This will take care of grabbing a text from
        /// the pool and place it properly, then register it as an active text so its position and opacity is updated by
        /// the system.
        /// </summary>
        /// <param name="amount">The amount of damage to display</param>
        /// <param name="worldPos">The position is the world where the damage text should appear (e.g. character head)</param>
        public void NewDamage(int amount, Vector3 worldPos)
        {       
            var t = m_TextPool.Dequeue();

            t.text = amount.ToString();
            t.gameObject.SetActive(true);
        
            ActiveText at = new ActiveText();
            at.MaxTime = 1.0f;
            at.Timer = at.MaxTime;
            at.UIText = t;
            at.WorldPositionStart = worldPos + Vector3.up;
            at.PlaceText(m_MainCamera, m_Canvas);
        
            m_ActiveTexts.Add(at);
        }
    }
}