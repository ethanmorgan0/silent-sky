using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Visual debugging tool to show spherical coordinate grid (theta/phi) on the viewport
    /// Helps visualize how spherical coordinates map to viewport positions
    /// </summary>
    public class SphericalCoordinateDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showThetaLines = true;
        [SerializeField] private bool showPhiLines = true;
        [SerializeField] private bool showLabels = true;
        [SerializeField] private int thetaDivisions = 8; // Number of theta lines (0 to 2π)
        [SerializeField] private int phiDivisions = 6; // Number of phi lines (0 to π)
        [SerializeField] private Color thetaLineColor = new Color(0f, 1f, 1f, 0.4f); // Cyan
        [SerializeField] private Color phiLineColor = new Color(1f, 0f, 1f, 0.4f); // Magenta
        [SerializeField] private Color labelColor = Color.white;
        [SerializeField] private float lineWidth = 1f;
        [SerializeField] private int fontSize = 10;
        
        [Header("References")]
        [SerializeField] private RectTransform sectorContainer;
        [SerializeField] private ViewportRotationController rotationController;
        
        private GameObject debugContainer;
        private List<GameObject> debugObjects = new List<GameObject>();
        
        private void Start()
        {
            // Auto-find references if not set
            if (sectorContainer == null)
            {
                GameObject containerObj = GameObject.Find("SectorContainer");
                if (containerObj != null)
                {
                    sectorContainer = containerObj.GetComponent<RectTransform>();
                }
            }
            
            if (rotationController == null)
            {
                rotationController = FindObjectOfType<ViewportRotationController>();
            }
            
            CreateDebugContainer();
        }
        
        private void Update()
        {
            if (sectorContainer == null)
            {
                return;
            }
            
            ClearDebugObjects();
            
            if (showThetaLines)
            {
                DrawThetaLines();
            }
            
            if (showPhiLines)
            {
                DrawPhiLines();
            }
            
            if (showLabels)
            {
                DrawLabels();
            }
        }
        
        private void CreateDebugContainer()
        {
            if (sectorContainer == null) return;
            
            debugContainer = new GameObject("SphericalCoordinateDebugger");
            debugContainer.transform.SetParent(sectorContainer.transform, false);
            
            RectTransform rect = debugContainer.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            
            // Force update to ensure container is properly sized
            Canvas.ForceUpdateCanvases();
            
            // Set to render on top
            debugContainer.transform.SetAsLastSibling();
        }
        
        private void ClearDebugObjects()
        {
            foreach (var obj in debugObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            debugObjects.Clear();
        }
        
        private void DrawThetaLines()
        {
            Canvas.ForceUpdateCanvases();
            // Use the debug container's size if available, otherwise fall back to sector container
            RectTransform container = debugContainer != null ? debugContainer.GetComponent<RectTransform>() : sectorContainer;
            Vector2 viewportSize = new Vector2(container.rect.width, container.rect.height);
            
            // Draw theta lines (constant theta, varying phi)
            // These are vertical lines in equirectangular projection
            // Note: theta = 0 and theta = 2π are the same, so we draw 0 to 2π but skip the duplicate
            for (int i = 0; i < thetaDivisions; i++) // Changed to < instead of <= to avoid duplicate 0/2π
            {
                float theta = (i / (float)thetaDivisions) * 2f * Mathf.PI;
                
                // Sample points along this theta line (varying phi)
                List<Vector2> linePoints = new List<Vector2>();
                int phiSamples = 50;
                
                for (int j = 0; j <= phiSamples; j++)
                {
                    float phi = (j / (float)phiSamples) * Mathf.PI;
                    
                    // Project to viewport
                    Vector2 viewportPos = ViewportProjection.ProjectToViewport(theta, phi);
                    
                    // Include points that are within or near viewport bounds (extend slightly beyond)
                    // This ensures we draw lines that cross the viewport edges
                    float margin = 0.1f; // Extend 10% beyond viewport
                    if (viewportPos.x >= -margin && viewportPos.x <= 1f + margin && 
                        viewportPos.y >= -margin && viewportPos.y <= 1f + margin)
                    {
                        // Clamp to viewport bounds for UI positioning
                        float clampedX = Mathf.Clamp(viewportPos.x, 0f, 1f);
                        float clampedY = Mathf.Clamp(viewportPos.y, 0f, 1f);
                        
                        Vector2 uiPos = new Vector2(
                            clampedX * viewportSize.x,
                            clampedY * viewportSize.y
                        );
                        linePoints.Add(uiPos);
                    }
                }
                
                // Draw line segments connecting consecutive points
                for (int k = 0; k < linePoints.Count - 1; k++)
                {
                    DrawLine(linePoints[k], linePoints[k + 1], thetaLineColor, 
                        $"ThetaLine_{i}_{k}");
                }
            }
        }
        
        private void DrawPhiLines()
        {
            Canvas.ForceUpdateCanvases();
            // Use the debug container's size if available, otherwise fall back to sector container
            RectTransform container = debugContainer != null ? debugContainer.GetComponent<RectTransform>() : sectorContainer;
            Vector2 viewportSize = new Vector2(container.rect.width, container.rect.height);
            
            // Draw phi lines (constant phi, varying theta)
            // These are horizontal lines in equirectangular projection
            for (int i = 0; i <= phiDivisions; i++)
            {
                float phi = (i / (float)phiDivisions) * Mathf.PI;
                
                // Sample points along this phi line (varying theta)
                List<Vector2> linePoints = new List<Vector2>();
                int thetaSamples = 100; // More samples for smoother curves
                
                for (int j = 0; j <= thetaSamples; j++)
                {
                    float theta = (j / (float)thetaSamples) * 2f * Mathf.PI;
                    
                    // Project to viewport
                    Vector2 viewportPos = ViewportProjection.ProjectToViewport(theta, phi);
                    
                    // Include points that are within or near viewport bounds (extend slightly beyond)
                    // This ensures we draw lines that cross the viewport edges
                    float margin = 0.1f; // Extend 10% beyond viewport
                    if (viewportPos.x >= -margin && viewportPos.x <= 1f + margin && 
                        viewportPos.y >= -margin && viewportPos.y <= 1f + margin)
                    {
                        // Clamp to viewport bounds for UI positioning
                        float clampedX = Mathf.Clamp(viewportPos.x, 0f, 1f);
                        float clampedY = Mathf.Clamp(viewportPos.y, 0f, 1f);
                        
                        Vector2 uiPos = new Vector2(
                            clampedX * viewportSize.x,
                            clampedY * viewportSize.y
                        );
                        linePoints.Add(uiPos);
                    }
                }
                
                // Draw line segments connecting consecutive points
                for (int k = 0; k < linePoints.Count - 1; k++)
                {
                    DrawLine(linePoints[k], linePoints[k + 1], phiLineColor, 
                        $"PhiLine_{i}_{k}");
                }
            }
        }
        
        private void DrawLabels()
        {
            Canvas.ForceUpdateCanvases();
            // Use the debug container's size if available, otherwise fall back to sector container
            RectTransform container = debugContainer != null ? debugContainer.GetComponent<RectTransform>() : sectorContainer;
            Vector2 viewportSize = new Vector2(container.rect.width, container.rect.height);
            
            // Draw theta labels along top edge
            // Skip theta = 2π since it's the same as theta = 0 (would overlap)
            for (int i = 0; i < thetaDivisions; i++) // Changed to < instead of <=
            {
                float theta = (i / (float)thetaDivisions) * 2f * Mathf.PI;
                float phi = Mathf.PI / 2f; // Equator (middle of phi range)
                
                Vector2 viewportPos = ViewportProjection.ProjectToViewport(theta, phi);
                
                if (viewportPos.x >= 0f && viewportPos.x <= 1f && 
                    viewportPos.y >= 0f && viewportPos.y <= 1f)
                {
                    Vector2 uiPos = new Vector2(
                        viewportPos.x * viewportSize.x,
                        viewportPos.y * viewportSize.y
                    );
                    
                    string label = $"θ={theta * Mathf.Rad2Deg:F0}°";
                    DrawText(uiPos + new Vector2(0, 15), label, labelColor, 
                        $"ThetaLabel_{i}");
                }
            }
            
            // Draw phi labels along left edge
            for (int i = 0; i <= phiDivisions; i++)
            {
                float phi = (i / (float)phiDivisions) * Mathf.PI;
                float theta = 0f; // Left edge of viewport
                
                Vector2 viewportPos = ViewportProjection.ProjectToViewport(theta, phi);
                
                if (viewportPos.x >= 0f && viewportPos.x <= 1f && 
                    viewportPos.y >= 0f && viewportPos.y <= 1f)
                {
                    Vector2 uiPos = new Vector2(
                        viewportPos.x * viewportSize.x,
                        viewportPos.y * viewportSize.y
                    );
                    
                    string label = $"φ={phi * Mathf.Rad2Deg:F0}°";
                    DrawText(uiPos + new Vector2(-40, 0), label, labelColor, 
                        $"PhiLabel_{i}");
                }
            }
            
            // Draw viewport center label
            Vector2 center = ViewportProjection.GetViewportCenter();
            Vector2 centerViewport = ViewportProjection.ProjectToViewport(center.x, center.y);
            if (centerViewport.x >= 0f && centerViewport.x <= 1f && 
                centerViewport.y >= 0f && centerViewport.y <= 1f)
            {
                Vector2 centerUI = new Vector2(
                    centerViewport.x * viewportSize.x,
                    centerViewport.y * viewportSize.y
                );
                
                string centerLabel = $"Center\nθ={center.x * Mathf.Rad2Deg:F1}°\nφ={center.y * Mathf.Rad2Deg:F1}°";
                DrawText(centerUI, centerLabel, Color.yellow, "ViewportCenterLabel");
            }
        }
        
        private void DrawLine(Vector2 start, Vector2 end, Color color, string name)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(debugContainer.transform, false);
            
            RectTransform rect = lineObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0f, 0.5f);
            
            Vector2 delta = end - start;
            float length = delta.magnitude;
            if (length < 0.1f) return; // Skip very short lines
            
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            
            rect.sizeDelta = new Vector2(length, lineWidth);
            rect.anchoredPosition = start;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            
            Image img = lineObj.AddComponent<Image>();
            img.color = color;
            
            debugObjects.Add(lineObj);
        }
        
        private void DrawText(Vector2 position, string text, Color color, string name)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(debugContainer.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(80f, 40f);
            rect.anchoredPosition = position;
            
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = fontSize;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            debugObjects.Add(textObj);
        }
    }
}

