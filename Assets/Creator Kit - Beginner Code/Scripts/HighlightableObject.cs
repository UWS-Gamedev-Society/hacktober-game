using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CreatorKitCode 
{
    /// <summary>
    /// All object that can be highlighted (enemies, interactable object etc.) derive from this class, which takes care
    /// of setting the material parameters for it when it gets highlighted.
    /// If the object use another material, it will just ignore all the changes.
    /// </summary>
    public class HighlightableObject : MonoBehaviour
    {
        protected Renderer[] m_Renderers;

        int m_RimColorID;
        int m_RimPowID;
        int m_RimIntensityID;


        Color[] m_OriginalRimColor;  
        float[] m_SavedRimIntensity;
    
        MaterialPropertyBlock m_PropertyBlock;
    
        protected virtual void Start()
        {
            m_Renderers = GetComponentsInChildren<Renderer>();

            m_RimColorID = Shader.PropertyToID("_RimColor");
            m_RimPowID = Shader.PropertyToID("_RimPower");
            m_RimIntensityID = Shader.PropertyToID("_RimIntensity");
        
            m_PropertyBlock = new MaterialPropertyBlock();

            m_OriginalRimColor = new Color[m_Renderers.Length];
            m_SavedRimIntensity = new float[m_Renderers.Length];
        
            for(int i = 0; i < m_Renderers.Length; ++i)
            {
                var rend = m_Renderers[i];
                var mat = rend.sharedMaterial;

                //it's not a rim colored material, just set that entry to null to skip it on setting
                if (!mat.HasProperty(m_RimColorID))
                {
                    m_Renderers[i] = null;
                    continue;
                }

                m_OriginalRimColor[i] = mat.GetColor(m_RimColorID);

                //"init" the property block with the property we want to retrieve
                m_PropertyBlock.SetColor(m_RimColorID, mat.GetColor(m_RimColorID));
                m_PropertyBlock.SetFloat(m_RimPowID, mat.GetFloat(m_RimPowID));
                m_PropertyBlock.SetFloat(m_RimIntensityID, mat.GetFloat(m_RimIntensityID));

                rend.SetPropertyBlock(m_PropertyBlock);
            }
        }
    
        /// <summary>
        /// Call this when the object is highlighted, this will switch all the material parameters to make it glow
        /// </summary>
        public void Highlight()
        {
            for (int i = 0; i < m_Renderers.Length; ++i)
            {
                var rend = m_Renderers[i];
            
                if(rend == null)
                    continue;
            
                rend.GetPropertyBlock(m_PropertyBlock);

                m_PropertyBlock.SetColor(m_RimColorID, m_OriginalRimColor[i]);
                m_PropertyBlock.SetFloat(m_RimPowID, m_PropertyBlock.GetFloat(m_RimPowID) * 0.25f);

                m_SavedRimIntensity[i] = m_PropertyBlock.GetFloat(m_RimIntensityID);
                m_PropertyBlock.SetFloat(m_RimIntensityID, 1.0f);

                rend.SetPropertyBlock(m_PropertyBlock);
            }
        }
        
        /// <summary>
        /// Call this when the object is done being highlighted, this will switch all the material parameters to make it back to normal
        /// </summary>
        public void Dehighlight()
        {
            for (int i = 0; i < m_Renderers.Length; ++i)
            {
                var rend = m_Renderers[i];
            
                if(rend == null)
                    continue;
            
                m_PropertyBlock.SetColor(m_RimColorID, Color.white);
                m_PropertyBlock.SetFloat(m_RimPowID, m_PropertyBlock.GetFloat(m_RimPowID) * 4.0f);
                m_PropertyBlock.SetFloat(m_RimIntensityID, m_SavedRimIntensity[i]);

                rend.SetPropertyBlock(m_PropertyBlock);
            }
        }
    }
}