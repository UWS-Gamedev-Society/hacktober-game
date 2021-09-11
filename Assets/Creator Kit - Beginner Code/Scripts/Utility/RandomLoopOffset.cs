using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CreatorKitCodeInternal
{
    /// <summary>
    /// Will pick a random time in an animation loop to offset an animator, allowing to avoid lots of object playing the
    /// same animation loop to look synchronised
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class RandomLoopOffset : MonoBehaviour
    {
        void Start()
        {
            var animator = GetComponent<Animator>();
            
            animator.SetFloat("CycleOffset", Random.value);
        }
    }

}