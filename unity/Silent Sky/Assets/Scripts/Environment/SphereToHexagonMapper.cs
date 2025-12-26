using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Unified mapper that combines sphere-to-viewport projection and viewport-to-hexagon mapping.
    /// Single source of truth for mapping spherical coordinates to hexagon indices.
    /// </summary>
    public static class SphereToHexagonMapper
    {
        /// <summary>
        /// Maps a point on the sphere (theta, phi) to a hexagon index.
        /// Uses two-step process: Sphere → Viewport → Hexagon
        /// </summary>
        /// <param name="theta">Azimuth angle [0, 2π)</param>
        /// <param name="phi">Polar angle [0, π]</param>
        /// <returns>Hexagon index [0-18] or -1 if not in any hexagon</returns>
        public static int GetHexagonForEvent(float theta, float phi)
        {
            // Step 1: Project sphere coordinates to viewport
            Vector2 viewportPos = ViewportProjection.ProjectToViewport(theta, phi);
            
            // Step 2: Map viewport coordinates to hexagon index
            int hexagonIndex = HexagonGridMapper.GetHexagonForViewportPoint(viewportPos);
            
            return hexagonIndex;
        }
        
        /// <summary>
        /// Checks if a point on the sphere is within the viewport FOV.
        /// </summary>
        public static bool IsInViewport(float theta, float phi)
        {
            return ViewportProjection.IsInViewport(theta, phi);
        }
    }
}

