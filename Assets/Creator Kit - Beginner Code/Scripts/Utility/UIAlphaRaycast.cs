using UnityEngine;
using UnityEngine.UI;

namespace CreatorKitCodeInternal 
{
    /// <summary>
    /// Allow to define the alpha threshold that will let a raycast pass in the UI. By default for performance reason that
    /// threshold is set to 0 (all part of the image, even the one with = 0, stop the raycast). 
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIAlphaRaycast : MonoBehaviour
    {
        [Range(0.0f, 1.0f)]
        public float AlphaLimit = 0.5f;
    
        Image UIImage;

        void Awake()
        {
            UIImage = GetComponent<Image>();
            if (!UIImage.sprite.texture.isReadable)
            {
                Debug.LogError("The texture of the sprite assign to a UIAlphaRaycast should be readable!");
                Destroy(this);
                return;
            }

            UIImage.alphaHitTestMinimumThreshold = AlphaLimit;
        }
    }
}