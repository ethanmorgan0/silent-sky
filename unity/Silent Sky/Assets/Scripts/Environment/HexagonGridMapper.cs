using System.Collections.Generic;
using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Maps viewport coordinates (normalized 0-1) to hexagon indices.
    /// Uses point-in-hexagon tests to determine which hexagon contains a viewport point.
    /// </summary>
    public static class HexagonGridMapper
    {
        private const int NUM_HEXAGONS = 19;
        private static Vector2[] hexagonCenters; // Viewport positions (normalized 0-1)
        private static float hexSizeNormalized; // Hexagon size in normalized viewport space
        private static bool initialized = false;
        
        /// <summary>
        /// Initializes the mapper with hexagon positions and size.
        /// Must be called before using GetHexagonForViewportPoint.
        /// </summary>
        /// <param name="hexWorldPositions">World positions of hexagons (from SectorMap, relative to container center)</param>
        /// <param name="hexSize">Size of each hexagon in world units (radius from center to vertex)</param>
        /// <param name="viewportSize">Size of the viewport container (RectTransform sizeDelta)</param>
        public static void Initialize(Vector2[] hexWorldPositions, float hexSize, Vector2 viewportSize)
        {
            if (hexWorldPositions == null || hexWorldPositions.Length != NUM_HEXAGONS)
            {
                Debug.LogError($"HexagonGridMapper: Expected {NUM_HEXAGONS} hexagon positions, got {hexWorldPositions?.Length ?? 0}");
                return;
            }
            
            hexagonCenters = new Vector2[NUM_HEXAGONS];
            
            // Convert world positions to normalized viewport coordinates [0, 1]
            // World positions are relative to the container center (0,0)
            // Viewport coordinates: (0,0) = bottom-left, (1,1) = top-right
            // Container center in viewport = (0.5, 0.5)
            
            for (int i = 0; i < NUM_HEXAGONS; i++)
            {
                // Convert world position (relative to center) to viewport coordinates
                // World: center is (0,0), positive Y is up
                // Viewport: center is (0.5, 0.5), positive Y is up
                Vector2 worldPos = hexWorldPositions[i];
                
                // Normalize: add 0.5 to center, then scale by viewport size
                hexagonCenters[i] = new Vector2(
                    0.5f + (worldPos.x / viewportSize.x),
                    0.5f + (worldPos.y / viewportSize.y)
                );
            }
            
            // Normalize hexSize to viewport space
            // hexSize is the radius from center to vertex (in pixels)
            // We normalize by the viewport size to get radius in normalized [0,1] space
            // For square viewports, use average; for rectangular, this is an approximation
            // But since hexagons are roughly circular, using average should be fine
            float avgViewportSize = (viewportSize.x + viewportSize.y) * 0.5f;
            hexSizeNormalized = hexSize / avgViewportSize;
            
            Debug.Log($"HexagonGridMapper: Initialized with {NUM_HEXAGONS} hexagons, hexSize={hexSize}px, viewportSize=({viewportSize.x:F0}, {viewportSize.y:F0}), normalizedRadius={hexSizeNormalized:F4}");
            
            initialized = true;
        }
        
        /// <summary>
        /// Gets the hexagon index that contains the given viewport point.
        /// Returns -1 if no hexagon contains the point.
        /// </summary>
        /// <param name="viewportPos">Normalized viewport coordinates [0, 1]</param>
        /// <returns>Hexagon index [0-18] or -1 if not in any hexagon</returns>
        public static int GetHexagonForViewportPoint(Vector2 viewportPos)
        {
            if (!initialized)
            {
                Debug.LogWarning("HexagonGridMapper: Not initialized. Call Initialize() first.");
                return -1;
            }
            
            // Early rejection: points outside [0, 1] viewport bounds cannot be in any hexagon
            if (viewportPos.x < 0f || viewportPos.x > 1f || viewportPos.y < 0f || viewportPos.y > 1f)
            {
                return -1;
            }
            
            // Check each hexagon to see if the point is inside it
            for (int i = 0; i < NUM_HEXAGONS; i++)
            {
                if (IsPointInHexagon(viewportPos, hexagonCenters[i], hexSizeNormalized))
                {
                    return i;
                }
            }
            
            return -1; // Point not in any hexagon
        }
        
        /// <summary>
        /// Checks if a point is inside a hexagon using proper geometric test.
        /// Uses the same hexagon geometry as HexagonSpriteGenerator (flat-top hexagons).
        /// </summary>
        private static bool IsPointInHexagon(Vector2 point, Vector2 center, float sizeNormalized)
        {
            // For flat-top hexagons, check if point is inside using edge tests
            // Hexagon vertices (relative to center in normalized viewport space):
            // Top: (0, sizeNormalized)
            // Top-right: (sizeNormalized * sqrt(3)/2, sizeNormalized/2)
            // Bottom-right: (sizeNormalized * sqrt(3)/2, -sizeNormalized/2)
            // Bottom: (0, -sizeNormalized)
            // Bottom-left: (-sizeNormalized * sqrt(3)/2, -sizeNormalized/2)
            // Top-left: (-sizeNormalized * sqrt(3)/2, sizeNormalized/2)
            
            Vector2 delta = point - center;
            float sqrt3 = Mathf.Sqrt(3f);
            
            // Use a proper point-in-polygon test: check if point is on the correct side of all edges
            // For each edge, check if the point is to the left (inside) of the edge
            // Edges are defined by consecutive vertices going clockwise
            
            Vector2[] vertices = new Vector2[6];
            vertices[0] = new Vector2(0f, sizeNormalized); // Top
            vertices[1] = new Vector2(sizeNormalized * sqrt3 / 2f, sizeNormalized / 2f); // Top-right
            vertices[2] = new Vector2(sizeNormalized * sqrt3 / 2f, -sizeNormalized / 2f); // Bottom-right
            vertices[3] = new Vector2(0f, -sizeNormalized); // Bottom
            vertices[4] = new Vector2(-sizeNormalized * sqrt3 / 2f, -sizeNormalized / 2f); // Bottom-left
            vertices[5] = new Vector2(-sizeNormalized * sqrt3 / 2f, sizeNormalized / 2f); // Top-left
            
            // Check if point is inside by testing against each edge
            // For a convex polygon, point is inside if it's on the same side of all edges
            // We'll use cross product to determine which side
            for (int i = 0; i < 6; i++)
            {
                Vector2 v1 = vertices[i];
                Vector2 v2 = vertices[(i + 1) % 6];
                
                // Edge vector
                Vector2 edge = v2 - v1;
                // Vector from v1 to point
                Vector2 toPoint = delta - v1;
                
                // Cross product: edge.x * toPoint.y - edge.y * toPoint.x
                // For Unity UI coordinate system (left-handed, Y-up), inside means cross < 0
                float cross = edge.x * toPoint.y - edge.y * toPoint.x;
                
                if (cross > 0f)
                {
                    return false; // Point is outside this edge (cross > 0 means outside in Unity UI)
                }
            }
            
            return true; // Point is inside all edges
        }
        
        /// <summary>
        /// Gets the viewport position of a hexagon center (for debugging).
        /// </summary>
        public static Vector2 GetHexagonCenterViewportPos(int hexagonIndex)
        {
            if (!initialized || hexagonIndex < 0 || hexagonIndex >= NUM_HEXAGONS)
            {
                return Vector2.zero;
            }
            return hexagonCenters[hexagonIndex];
        }
        
        /// <summary>
        /// Checks if the mapper is initialized.
        /// </summary>
        public static bool IsInitialized()
        {
            return initialized;
        }
        
        /// <summary>
        /// Gets the normalized hex size (for debugging).
        /// </summary>
        public static float GetHexSizeNormalized()
        {
            return hexSizeNormalized;
        }
    }
}

