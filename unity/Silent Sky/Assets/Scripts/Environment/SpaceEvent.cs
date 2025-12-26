using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Represents an event in space with position, value, and timing
    /// </summary>
    [System.Serializable]
    public class SpaceEvent
    {
        public string eventType; // For now: just "Event", types come later
        public float value; // Event magnitude/importance
        public float theta; // Azimuth angle [0, 2π]
        public float phi; // Polar angle [0, π]
        public float timestamp; // When event occurs
        public float duration; // How long event lasts (static for now)
        
        public SpaceEvent(string type, float val, float t, float p, float time, float dur)
        {
            eventType = type;
            value = val;
            theta = SphericalCoordinateSystem.NormalizeTheta(t);
            phi = SphericalCoordinateSystem.ClampPhi(p);
            timestamp = time;
            duration = dur;
        }
        
        /// <summary>
        /// Checks if event is active at given time
        /// </summary>
        public bool IsActive(float currentTime)
        {
            return currentTime >= timestamp && currentTime <= timestamp + duration;
        }
        
        /// <summary>
        /// Gets 3D position of event on unit sphere
        /// </summary>
        public Vector3 GetPosition()
        {
            return SphericalCoordinateSystem.SphericalToCartesian(theta, phi, 1f);
        }
    }
}

