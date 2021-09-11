using UnityEngine;

namespace CreatorKitCode 
{
    /// <summary>
    /// Helper class containing diverse functions that avoid redoing common things.
    /// </summary>
    public class Helpers
    {
        public static int WrapAngle(int angle)
        {
            while (angle < 0)
                angle += 360;

            while (angle > 360)
                angle -= 360;

            return angle;
        }

        public static void RecursiveLayerChange(Transform root, int layer)
        {
            root.gameObject.layer = layer;
        
            foreach(Transform t in root)
                RecursiveLayerChange(t, layer);
        }
    }
}