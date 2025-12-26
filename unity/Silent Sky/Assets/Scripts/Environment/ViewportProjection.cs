using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Projects spherical coordinates (theta, phi) to viewport coordinates using equirectangular projection.
    /// Viewport represents a Field of View (FOV) that can be rotated around the sphere.
    /// </summary>
    public static class ViewportProjection
    {
        // FOV: 180° horizontal × 120° vertical (human-like FOV)
        private const float FOV_HORIZONTAL = 180f * Mathf.Deg2Rad; // π radians
        private const float FOV_VERTICAL = 120f * Mathf.Deg2Rad;   // 2π/3 radians
        
        // Default viewport center: equator (phi = π/2), forward direction (theta = 0)
        private const float DEFAULT_CENTER_THETA = 0f;
        private const float DEFAULT_CENTER_PHI = Mathf.PI / 2f;
        
        // Current viewport rotation (rotation offset from default center)
        private static float thetaOffset = 0f;
        private static float phiOffset = 0f;
        
        /// <summary>
        /// Projects a point on the sphere (theta, phi) to normalized viewport coordinates [0, 1].
        /// Uses equirectangular projection with current viewport rotation.
        /// 
        /// Returns coordinates outside [0, 1] if the point is outside the viewport FOV.
        /// </summary>
        /// <param name="theta">Azimuth angle [0, 2π)</param>
        /// <param name="phi">Polar angle [0, π]</param>
        /// <returns>Normalized viewport coordinates (x, y) where (0,0) is bottom-left, (1,1) is top-right</returns>
        public static Vector2 ProjectToViewport(float theta, float phi)
        {
            return ProjectToViewport(theta, phi, thetaOffset, phiOffset);
        }
        
        /// <summary>
        /// Projects a point on the sphere (theta, phi) to normalized viewport coordinates [0, 1].
        /// Uses equirectangular projection with specified rotation offset.
        /// </summary>
        /// <param name="theta">Azimuth angle [0, 2π)</param>
        /// <param name="phi">Polar angle [0, π]</param>
        /// <param name="thetaOffset">Rotation offset for theta (viewport orientation)</param>
        /// <param name="phiOffset">Rotation offset for phi (viewport orientation)</param>
        /// <returns>Normalized viewport coordinates (x, y) where (0,0) is bottom-left, (1,1) is top-right</returns>
        public static Vector2 ProjectToViewport(float theta, float phi, float thetaOffset, float phiOffset)
        {
            // Get centerTheta (normalized from unbounded thetaOffset)
            float normalizedThetaOffset = SphericalCoordinateSystem.NormalizeTheta(thetaOffset);
            float centerTheta = SphericalCoordinateSystem.NormalizeTheta(DEFAULT_CENTER_THETA + normalizedThetaOffset);
            
            // Get raw centerPhi (unbounded, like thetaOffset)
            float centerPhiRaw = DEFAULT_CENTER_PHI + phiOffset;
            
            // Clamp centerPhiRaw to valid range [0, π] to prevent scrolling past poles
            float clampedCenterPhi = Mathf.Clamp(centerPhiRaw, 0f, Mathf.PI);
            
            // Calculate deltaTheta - adjust for pole crossings
            // When crossing a pole, the view is rotated 180 degrees, so theta needs adjustment
            float rawDelta = theta - centerTheta;
            float deltaTheta = rawDelta;
            if (deltaTheta > Mathf.PI)
                deltaTheta -= 2f * Mathf.PI;
            else if (deltaTheta < -Mathf.PI)
                deltaTheta += 2f * Mathf.PI;
            
            // Calculate deltaPhi - simple approach with clamped center
            float deltaPhi = phi - clampedCenterPhi;
            
            // Convert angular offsets to normalized viewport coordinates
            float x = 0.5f + (deltaTheta / FOV_HORIZONTAL);
            float y = 0.5f + (deltaPhi / FOV_VERTICAL);
            
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// Checks if a point (theta, phi) is within the viewport FOV.
        /// </summary>
        public static bool IsInViewport(float theta, float phi)
        {
            Vector2 viewportPos = ProjectToViewport(theta, phi);
            return viewportPos.x >= 0f && viewportPos.x <= 1f && 
                   viewportPos.y >= 0f && viewportPos.y <= 1f;
        }
        
        /// <summary>
        /// Gets the FOV bounds in radians.
        /// </summary>
        public static Vector2 GetFOV()
        {
            return new Vector2(FOV_HORIZONTAL, FOV_VERTICAL);
        }
        
        /// <summary>
        /// Sets the viewport rotation offset.
        /// </summary>
        /// <param name="thetaOffset">Rotation offset for theta (azimuth) in radians</param>
        /// <param name="phiOffset">Rotation offset for phi (polar) in radians</param>
        public static void SetViewportRotation(float thetaOffset, float phiOffset)
        {
            ViewportProjection.thetaOffset = thetaOffset;
            // Allow phi offset to be unbounded (same strategy as theta)
            ViewportProjection.phiOffset = phiOffset;
        }
        
        /// <summary>
        /// Gets the current viewport rotation offset.
        /// </summary>
        /// <returns>Rotation offset (thetaOffset, phiOffset) in radians</returns>
        public static Vector2 GetViewportRotation()
        {
            return new Vector2(thetaOffset, phiOffset);
        }
        
        /// <summary>
        /// Gets the current viewport center position on the sphere.
        /// Uses same strategy as theta: keep phiOffset unbounded, wrap only when needed.
        /// </summary>
        /// <returns>Viewport center (theta, phi) in radians</returns>
        public static Vector2 GetViewportCenter()
        {
            // Normalize thetaOffset here (not in controller) to compute centerTheta
            // This preserves the unbounded state in the controller, preventing jumps
            float normalizedThetaOffset = SphericalCoordinateSystem.NormalizeTheta(thetaOffset);
            float centerTheta = SphericalCoordinateSystem.NormalizeTheta(DEFAULT_CENTER_THETA + normalizedThetaOffset);
            
            // Compute centerPhi from offset - allow unbounded phiOffset (same as theta)
            float centerPhi = DEFAULT_CENTER_PHI + phiOffset;
            
            // Wrap phi to [0, π] range, tracking how many times we've wrapped
            // Use modulo-like wrapping: if centerPhi = π + ε, wrap to ε
            // This allows continuous rotation past poles
            while (centerPhi < 0f)
            {
                centerPhi += Mathf.PI;
            }
            while (centerPhi > Mathf.PI)
            {
                centerPhi -= Mathf.PI;
            }
            
            // Ensure valid range [0, π]
            centerPhi = Mathf.Clamp(centerPhi, 0f, Mathf.PI);
            
            return new Vector2(centerTheta, centerPhi);
        }
    }
}

