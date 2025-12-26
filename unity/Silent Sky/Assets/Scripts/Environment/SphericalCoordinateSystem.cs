using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Utility class for spherical coordinate system conversions
    /// Theta (θ): Azimuth angle [0, 2π] (longitude)
    /// Phi (φ): Polar angle [0, π] (latitude)
    /// </summary>
    public static class SphericalCoordinateSystem
    {
        /// <summary>
        /// Converts spherical coordinates to 3D Cartesian coordinates
        /// </summary>
        /// <param name="theta">Azimuth angle [0, 2π]</param>
        /// <param name="phi">Polar angle [0, π]</param>
        /// <param name="radius">Distance from origin (default 1 for unit sphere)</param>
        /// <returns>Cartesian coordinates (x, y, z)</returns>
        public static Vector3 SphericalToCartesian(float theta, float phi, float radius = 1f)
        {
            float x = radius * Mathf.Sin(phi) * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(phi) * Mathf.Sin(theta);
            float z = radius * Mathf.Cos(phi);
            return new Vector3(x, y, z);
        }
        
        /// <summary>
        /// Converts 3D Cartesian coordinates to spherical coordinates
        /// </summary>
        /// <param name="cartesian">Cartesian coordinates (x, y, z)</param>
        /// <returns>Spherical coordinates (theta, phi, radius)</returns>
        public static (float theta, float phi, float radius) CartesianToSpherical(Vector3 cartesian)
        {
            float radius = cartesian.magnitude;
            if (radius < 0.0001f)
            {
                return (0f, 0f, 0f);
            }
            
            float theta = Mathf.Atan2(cartesian.y, cartesian.x);
            if (theta < 0f)
            {
                theta += 2f * Mathf.PI; // Normalize to [0, 2π]
            }
            
            float phi = Mathf.Acos(cartesian.z / radius);
            
            return (theta, phi, radius);
        }
        
        /// <summary>
        /// Calculates angular distance (great circle distance) between two points on a sphere
        /// </summary>
        /// <param name="theta1">Azimuth of first point</param>
        /// <param name="phi1">Polar angle of first point</param>
        /// <param name="theta2">Azimuth of second point</param>
        /// <param name="phi2">Polar angle of second point</param>
        /// <returns>Angular distance in radians</returns>
        public static float AngularDistance(float theta1, float phi1, float theta2, float phi2)
        {
            // Using spherical law of cosines
            float cosDelta = Mathf.Sin(phi1) * Mathf.Sin(phi2) * Mathf.Cos(theta1 - theta2) + 
                            Mathf.Cos(phi1) * Mathf.Cos(phi2);
            cosDelta = Mathf.Clamp(cosDelta, -1f, 1f); // Clamp to avoid numerical errors
            return Mathf.Acos(cosDelta);
        }
        
        /// <summary>
        /// Normalizes theta to [0, 2π] range
        /// </summary>
        public static float NormalizeTheta(float theta)
        {
            while (theta < 0f)
            {
                theta += 2f * Mathf.PI;
            }
            while (theta >= 2f * Mathf.PI)
            {
                theta -= 2f * Mathf.PI;
            }
            return theta;
        }
        
        /// <summary>
        /// Clamps phi to [0, π] range
        /// </summary>
        public static float ClampPhi(float phi)
        {
            return Mathf.Clamp(phi, 0f, Mathf.PI);
        }
    }
}

