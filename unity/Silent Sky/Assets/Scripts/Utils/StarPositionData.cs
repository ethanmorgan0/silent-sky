using UnityEngine;

namespace SilentSky.Unity.Utils
{
    /// <summary>
    /// Helper component to store star position data
    /// Stores spherical coordinates so stars can update when viewport rotates
    /// </summary>
    public class StarPositionData : MonoBehaviour
    {
        // Spherical coordinates (source of truth)
        public float theta; // Azimuth [0, 2π)
        public float phi;   // Polar [0, π]
        
        // Cached viewport position (recalculated when rotation changes)
        public float normalizedX;
        public float normalizedY;
    }
}

