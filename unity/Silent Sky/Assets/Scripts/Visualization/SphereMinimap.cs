using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Environment;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Minimap component that shows the current viewport orientation on the full sphere.
    /// Displays the viewport FOV region and hexagon centers.
    /// </summary>
    public class SphereMinimap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ViewportRotationController rotationController;
        [SerializeField] private RectTransform minimapContainer; // Container for minimap UI (auto-created if not set)
        
        [Header("Visual Settings")]
        [SerializeField] private Vector2 minimapSize = new Vector2(200f, 150f);
        [SerializeField] private Vector2 minimapPosition = new Vector2(0f, 0f); // Bottom-right corner offset (at corner, adjust in inspector if needed)
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
        [SerializeField] private Color viewportFOVColor = new Color(1f, 1f, 0f, 0.3f); // Yellow highlight
        [SerializeField] private Color viewportFOVBorderColor = new Color(1f, 1f, 0f, 0.8f);
        [SerializeField] private Color hexagonCenterColor = new Color(0f, 1f, 1f, 0.6f); // Cyan
        [SerializeField] private float hexagonCenterSize = 3f;
        [SerializeField] private bool showGrid = true;
        [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.2f);
        
        private Image minimapBackground;
        private Image viewportFOVHighlight;
        private RectTransform minimapRect;
        private Canvas parentCanvas;
        
        private void Start()
        {
            // Find rotation controller if not assigned
            if (rotationController == null)
            {
                rotationController = FindObjectOfType<ViewportRotationController>();
            }
            
            if (rotationController == null)
            {
                Debug.LogWarning("SphereMinimap: ViewportRotationController not found. Minimap will not update.");
                return;
            }
            
            // Find or create canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                parentCanvas = FindObjectOfType<Canvas>();
            }
            
            if (parentCanvas == null)
            {
                Debug.LogError("SphereMinimap: No Canvas found. Create a Canvas in the scene.");
                return;
            }
            
            CreateMinimapUI();
        }
        
        private void CreateMinimapUI()
        {
            // Create minimap container if not assigned
            if (minimapContainer == null)
            {
                GameObject containerObj = new GameObject("MinimapContainer");
                containerObj.transform.SetParent(parentCanvas.transform, false);
                minimapContainer = containerObj.AddComponent<RectTransform>();
                
                // Position in bottom-right corner
                minimapContainer.anchorMin = new Vector2(1f, 0f);
                minimapContainer.anchorMax = new Vector2(1f, 0f);
                minimapContainer.pivot = new Vector2(1f, 0f);
                // Use negative offset to position from corner (anchoredPosition is relative to anchor)
                minimapContainer.anchoredPosition = new Vector2(-minimapPosition.x, minimapPosition.y);
                minimapContainer.sizeDelta = minimapSize;
            }
            
            minimapRect = minimapContainer;
            
            // Create background
            GameObject bgObj = new GameObject("MinimapBackground");
            bgObj.transform.SetParent(minimapContainer, false);
            minimapBackground = bgObj.AddComponent<Image>();
            minimapBackground.color = backgroundColor;
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            // Create viewport FOV highlight
            GameObject fovObj = new GameObject("ViewportFOV");
            fovObj.transform.SetParent(minimapContainer, false);
            viewportFOVHighlight = fovObj.AddComponent<Image>();
            viewportFOVHighlight.color = viewportFOVColor;
            
            RectTransform fovRect = fovObj.GetComponent<RectTransform>();
            fovRect.anchorMin = Vector2.zero;
            fovRect.anchorMax = Vector2.zero;
            fovRect.pivot = new Vector2(0.5f, 0.5f);
        }
        
        private void Update()
        {
            if (rotationController == null || minimapContainer == null)
                return;
            
            UpdateViewportFOVHighlight();
            UpdateHexagonCenters();
        }
        
        private void UpdateViewportFOVHighlight()
        {
            if (viewportFOVHighlight == null)
                return;
            
            // Get current viewport center and FOV
            Vector2 viewportCenter = ViewportProjection.GetViewportCenter();
            Vector2 fov = ViewportProjection.GetFOV();
            
            // Convert viewport center to minimap coordinates
            // Minimap shows full sphere (0-2π for theta, 0-π for phi)
            // Map to minimap size
            float minimapWidth = minimapRect.rect.width;
            float minimapHeight = minimapRect.rect.height;
            
            // Convert sphere coordinates to minimap pixel coordinates
            // Theta: 0-2π maps to 0-width
            // Phi: 0-π maps to height-0 (inverted, so 0 is at top)
            float x = (viewportCenter.x / (2f * Mathf.PI)) * minimapWidth;
            float y = (1f - (viewportCenter.y / Mathf.PI)) * minimapHeight;
            
            // Calculate FOV size in minimap pixels
            float fovWidth = (fov.x / (2f * Mathf.PI)) * minimapWidth;
            float fovHeight = (fov.y / Mathf.PI) * minimapHeight;
            
            // Position and size the FOV highlight
            RectTransform fovRect = viewportFOVHighlight.GetComponent<RectTransform>();
            fovRect.anchoredPosition = new Vector2(x, y);
            fovRect.sizeDelta = new Vector2(fovWidth, fovHeight);
        }
        
        private void UpdateHexagonCenters()
        {
            if (!HexagonGridMapper.IsInitialized())
                return;
            
            // Remove old hexagon center markers
            for (int i = minimapContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = minimapContainer.GetChild(i);
                if (child.name.StartsWith("HexCenter_"))
                {
                    Destroy(child.gameObject);
                }
            }
            
            // Get hexagon centers in viewport space and convert to sphere coordinates
            // This is a simplified approach - we'll show hexagon centers based on their viewport positions
            // For a full implementation, we'd need to store hexagon sphere positions
            
            // For now, we'll skip showing hexagon centers on the minimap
            // This would require storing hexagon sphere positions (theta, phi) which we don't currently have
            // TODO: Add hexagon sphere position storage for full minimap support
        }
        
        /// <summary>
        /// Draws grid lines on the minimap (optional)
        /// </summary>
        private void DrawGrid()
        {
            if (!showGrid || minimapContainer == null)
                return;
            
            // Remove old grid lines
            for (int i = minimapContainer.childCount - 1; i >= 0; i--)
            {
                Transform child = minimapContainer.GetChild(i);
                if (child.name.StartsWith("GridLine_"))
                {
                    Destroy(child.gameObject);
                }
            }
            
            float minimapWidth = minimapRect.rect.width;
            float minimapHeight = minimapRect.rect.height;
            
            // Draw vertical lines (theta divisions)
            int thetaDivisions = 8;
            for (int i = 0; i <= thetaDivisions; i++)
            {
                float x = (i / (float)thetaDivisions) * minimapWidth;
                DrawLine(new Vector2(x, 0f), new Vector2(x, minimapHeight), gridColor, $"GridLine_V_{i}");
            }
            
            // Draw horizontal lines (phi divisions)
            int phiDivisions = 6;
            for (int i = 0; i <= phiDivisions; i++)
            {
                float y = (i / (float)phiDivisions) * minimapHeight;
                DrawLine(new Vector2(0f, y), new Vector2(minimapWidth, y), gridColor, $"GridLine_H_{i}");
            }
        }
        
        private void DrawLine(Vector2 start, Vector2 end, Color color, string name)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(minimapContainer, false);
            
            RectTransform rect = lineObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 0.5f);
            
            Vector2 delta = end - start;
            float length = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            
            rect.sizeDelta = new Vector2(length, 1f);
            rect.anchoredPosition = start;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            
            Image img = lineObj.AddComponent<Image>();
            img.color = color;
        }
        
        private void LateUpdate()
        {
            if (showGrid)
            {
                DrawGrid();
            }
        }
    }
}

