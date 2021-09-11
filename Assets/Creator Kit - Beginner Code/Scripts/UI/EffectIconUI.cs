using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CreatorKitCodeInternal {
    /// <summary>
    /// Simple Monobehaviour used to get fast reference to the Image and Slider used by current active effect icone on UI
    /// </summary>
    public class EffectIconUI : MonoBehaviour
    {
        public Image BackgroundImage;
        public Slider TimeSlider;
    }
}