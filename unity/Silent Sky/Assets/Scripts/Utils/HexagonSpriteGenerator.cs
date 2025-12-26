using System.Collections.Generic;
using UnityEngine;

namespace SilentSky.Unity.Utils
{
    /// <summary>
    /// Utility class for generating hexagon sprites programmatically
    /// </summary>
    public static class HexagonSpriteGenerator
    {
        /// <summary>
        /// Creates a hexagon sprite with the specified size
        /// </summary>
        /// <param name="size">Size of the hexagon (radius from center to vertex)</param>
        /// <param name="pixelsPerUnit">Pixels per Unity unit</param>
        /// <returns>A sprite with a hexagon shape</returns>
        public static Sprite CreateHexagonSprite(float size = 100f, int pixelsPerUnit = 100)
        {
            // For flat-top hexagon with radius 'size':
            // Width (flat edge to flat edge) = size * sqrt(3)
            // Height (point to point) = size * 2
            // We need texture large enough to contain the hexagon with some padding
            float sqrt3 = Mathf.Sqrt(3f);
            float hexWidth = size * sqrt3;
            float hexHeight = size * 2f;
            int textureWidth = Mathf.CeilToInt(hexWidth * 1.2f); // 20% padding
            int textureHeight = Mathf.CeilToInt(hexHeight * 1.2f); // 20% padding
            int textureSize = Mathf.Max(textureWidth, textureHeight); // Use square texture
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            
            // Fill with transparent
            Color[] pixels = new Color[textureSize * textureSize];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.clear;
            }
            
            Vector2 center = new Vector2(textureSize / 2f, textureSize / 2f);
            float radius = size;
            
            // Draw hexagon using 6-sided polygon fill
            // For flat-top hexagon (pointy-top), vertices are at specific angles
            Vector2[] vertices = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                // Start at top (90°) and go clockwise
                float angle = (90f - i * 60f) * Mathf.Deg2Rad;
                vertices[i] = center + new Vector2(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius
                );
            }
            
            // Fill hexagon using scanline algorithm
            for (int y = 0; y < textureSize; y++)
            {
                // Find intersections with hexagon edges for this scanline
                List<float> intersections = new List<float>();
                for (int i = 0; i < 6; i++)
                {
                    Vector2 v1 = vertices[i];
                    Vector2 v2 = vertices[(i + 1) % 6];
                    
                    // Check if scanline intersects this edge
                    if ((v1.y <= y && v2.y > y) || (v2.y <= y && v1.y > y))
                    {
                        if (Mathf.Abs(v2.y - v1.y) > 0.001f)
                        {
                            float x = v1.x + (v2.x - v1.x) * (y - v1.y) / (v2.y - v1.y);
                            intersections.Add(x);
                        }
                    }
                }
                
                // Sort intersections
                intersections.Sort();
                
                // Fill between pairs of intersections
                for (int i = 0; i < intersections.Count - 1; i += 2)
                {
                    int xStart = Mathf.Max(0, Mathf.FloorToInt(intersections[i]));
                    int xEnd = Mathf.Min(textureSize - 1, Mathf.CeilToInt(intersections[i + 1]));
                    
                    for (int x = xStart; x <= xEnd; x++)
                    {
                        pixels[y * textureSize + x] = Color.white;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            
            // Calculate exact hexagon bounds in texture space
            int hexWidthPixels = Mathf.CeilToInt(hexWidth);
            int hexHeightPixels = Mathf.CeilToInt(hexHeight);
            
            // Center the hexagon rect in the texture
            float rectX = (textureSize - hexWidthPixels) * 0.5f;
            float rectY = (textureSize - hexHeightPixels) * 0.5f;
            
            // Create sprite from texture
            // The sprite rect should match the actual hexagon dimensions exactly
            // This ensures 1:1 pixel mapping when used in UI
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(rectX, rectY, hexWidthPixels, hexHeightPixels), // Exact hexagon size
                new Vector2(0.5f, 0.5f), // Pivot at center
                1f // Use 1 pixel per unit for UI to get 1:1 pixel mapping
            );
            
            return sprite;
        }
        
    }
}

