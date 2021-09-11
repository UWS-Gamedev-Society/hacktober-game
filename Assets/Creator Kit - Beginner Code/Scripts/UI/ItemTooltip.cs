using UnityEngine;
using UnityEngine.UI;

namespace CreatorKitCodeInternal 
{
    public class ItemTooltip : MonoBehaviour
    {
        public Text Name;
        public Text DescriptionText;

        RectTransform m_RectTransform;
        CanvasScaler m_CanvasScaler;
        Canvas m_Canvas;
    
        void Awake()
        {
            m_RectTransform = GetComponent<RectTransform>();
            m_CanvasScaler = GetComponentInParent<CanvasScaler>();
            m_Canvas = GetComponentInParent<Canvas>();
        }

        void OnEnable()
        {
            UpdatePosition();
        }

        void Update()
        {
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            Vector3 mousePosition = Input.mousePosition;
        
            Vector3[] corners = new Vector3[4];    
            m_RectTransform.GetWorldCorners(corners);

            float width = corners[3].x - corners[0].x;
            float height = corners[1].y - corners[0].y;

            if (width + 16 < Screen.width - mousePosition.x)
            {
                m_RectTransform.transform.position = mousePosition + Vector3.right * 16;
            }
            else
            {
                m_RectTransform.transform.position = mousePosition + Vector3.left * (width + 16);
            }
        }
    }
}