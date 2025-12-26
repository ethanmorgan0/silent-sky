using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SilentSky.Unity.Visualization;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Visual debugging tool to show hexagon viewport boundaries and coordinate mappings
    /// Helps diagnose mismatches between calculated and rendered positions
    /// </summary>
    public class HexagonMappingDebugger : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showHexagonBounds = true;
        [SerializeField] private bool showHexagonCenters = true;
        [SerializeField] private bool showEventPositions = true;
        [SerializeField] private bool showCoordinateGrid = false;
        [SerializeField] private Color hexagonBoundColor = new Color(1f, 0f, 0f, 0.5f); // Red, semi-transparent
        [SerializeField] private Color hexagonCenterColor = Color.yellow;
        [SerializeField] private Color eventColor = Color.cyan;
        [SerializeField] private float lineWidth = 2f;
        
        [Header("References")]
        [SerializeField] private RectTransform sectorContainer;
        [SerializeField] private SignalCalculator signalCalculator;
        [SerializeField] private FakeDataGenerator dataGenerator;
        
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
            
            if (signalCalculator == null)
            {
                signalCalculator = FindObjectOfType<SignalCalculator>();
            }
            
            if (dataGenerator == null)
            {
                dataGenerator = FindObjectOfType<FakeDataGenerator>();
            }
            
            CreateDebugContainer();
        }
        
        private void Update()
        {
            if (sectorContainer == null || !HexagonGridMapper.IsInitialized())
            {
                return;
            }
            
            ClearDebugObjects();
            
            if (showHexagonBounds)
            {
                DrawHexagonBounds();
            }
            
            if (showHexagonCenters)
            {
                DrawHexagonCenters();
            }
            
            if (showEventPositions)
            {
                DrawEventPositions();
            }
            
            if (showCoordinateGrid)
            {
                DrawCoordinateGrid();
            }
        }
        
        private void CreateDebugContainer()
        {
            if (sectorContainer == null) return;
            
            debugContainer = new GameObject("HexagonMappingDebugger");
            debugContainer.transform.SetParent(sectorContainer.transform, false);
            
            RectTransform rect = debugContainer.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
            
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
        
        private void DrawHexagonBounds()
        {
            if (!HexagonGridMapper.IsInitialized()) return;
            
            // Get viewport size from SectorContainer
            Canvas.ForceUpdateCanvases();
            Vector2 viewportSize = new Vector2(sectorContainer.rect.width, sectorContainer.rect.height);
            
            // Get hexSize (try to get from SectorMap)
            float hexSize = 80f;
            var sectorMap = FindObjectOfType<SectorMap>();
            if (sectorMap != null)
            {
                var hexSizeField = typeof(SectorMap).GetField("hexSize", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (hexSizeField != null)
                {
                    hexSize = (float)hexSizeField.GetValue(sectorMap);
                }
            }
            
            float avgViewportSize = (viewportSize.x + viewportSize.y) * 0.5f;
            float hexSizeNormalized = hexSize / avgViewportSize;
            float sqrt3 = Mathf.Sqrt(3f);
            
            // Draw bounds for each hexagon
            for (int i = 0; i < 19; i++)
            {
                Vector2 hexCenterViewport = HexagonGridMapper.GetHexagonCenterViewportPos(i);
                
                // Convert viewport center to UI position
                Vector2 hexCenterUI = new Vector2(
                    hexCenterViewport.x * viewportSize.x,
                    hexCenterViewport.y * viewportSize.y
                );
                
                // Calculate hexagon vertices in viewport space
                Vector2[] vertices = new Vector2[6];
                vertices[0] = new Vector2(0f, hexSizeNormalized);
                vertices[1] = new Vector2(hexSizeNormalized * sqrt3 / 2f, hexSizeNormalized / 2f);
                vertices[2] = new Vector2(hexSizeNormalized * sqrt3 / 2f, -hexSizeNormalized / 2f);
                vertices[3] = new Vector2(0f, -hexSizeNormalized);
                vertices[4] = new Vector2(-hexSizeNormalized * sqrt3 / 2f, -hexSizeNormalized / 2f);
                vertices[5] = new Vector2(-hexSizeNormalized * sqrt3 / 2f, hexSizeNormalized / 2f);
                
                // Convert vertices to UI space and draw
                for (int j = 0; j < 6; j++)
                {
                    Vector2 v1Viewport = hexCenterViewport + vertices[j];
                    Vector2 v2Viewport = hexCenterViewport + vertices[(j + 1) % 6];
                    
                    Vector2 v1UI = new Vector2(
                        v1Viewport.x * viewportSize.x,
                        v1Viewport.y * viewportSize.y
                    );
                    Vector2 v2UI = new Vector2(
                        v2Viewport.x * viewportSize.x,
                        v2Viewport.y * viewportSize.y
                    );
                    
                    DrawLine(v1UI, v2UI, hexagonBoundColor, $"Hex{i}_Edge{j}");
                }
            }
        }
        
        private void DrawHexagonCenters()
        {
            if (!HexagonGridMapper.IsInitialized()) return;
            
            Canvas.ForceUpdateCanvases();
            Vector2 viewportSize = new Vector2(sectorContainer.rect.width, sectorContainer.rect.height);
            
            for (int i = 0; i < 19; i++)
            {
                Vector2 hexCenterViewport = HexagonGridMapper.GetHexagonCenterViewportPos(i);
                Vector2 hexCenterUI = new Vector2(
                    hexCenterViewport.x * viewportSize.x,
                    hexCenterViewport.y * viewportSize.y
                );
                
                // Draw a small circle at the center
                DrawCircle(hexCenterUI, 5f, hexagonCenterColor, $"Hex{i}_Center");
                
                // Also draw a label
                DrawText(hexCenterUI + new Vector2(0, 15), $"H{i}\n({hexCenterViewport.x:F3},{hexCenterViewport.y:F3})", 
                    hexagonCenterColor, $"Hex{i}_Label");
            }
        }
        
        private void DrawEventPositions()
        {
            if (dataGenerator == null || signalCalculator == null) return;
            
            Canvas.ForceUpdateCanvases();
            Vector2 viewportSize = new Vector2(sectorContainer.rect.width, sectorContainer.rect.height);
            
            List<SpaceEvent> activeEvents = dataGenerator.GetActiveEvents(signalCalculator.currentTime);
            
            foreach (var evt in activeEvents)
            {
                if (!ViewportProjection.IsInViewport(evt.theta, evt.phi))
                    continue;
                
                Vector2 eventViewport = ViewportProjection.ProjectToViewport(evt.theta, evt.phi);
                Vector2 eventUI = new Vector2(
                    eventViewport.x * viewportSize.x,
                    eventViewport.y * viewportSize.y
                );
                
                // Draw event position
                DrawCircle(eventUI, 8f, eventColor, $"Event_{evt.GetHashCode()}");
                
                // Check which hexagon the mapper thinks this event is in
                int mappedHex = SphereToHexagonMapper.GetHexagonForEvent(evt.theta, evt.phi);
                
                // Find closest hexagon center
                int closestHex = -1;
                float minDist = float.MaxValue;
                for (int h = 0; h < 19; h++)
                {
                    Vector2 hexCenter = HexagonGridMapper.GetHexagonCenterViewportPos(h);
                    float dist = Vector2.Distance(eventViewport, hexCenter);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestHex = h;
                    }
                }
                
                // Draw line to closest hexagon center
                if (closestHex >= 0)
                {
                    Vector2 hexCenterViewport = HexagonGridMapper.GetHexagonCenterViewportPos(closestHex);
                    Vector2 hexCenterUI = new Vector2(
                        hexCenterViewport.x * viewportSize.x,
                        hexCenterViewport.y * viewportSize.y
                    );
                    
                    // Use different color based on whether mapping succeeded
                    Color lineColor = (mappedHex == closestHex) ? 
                        new Color(0f, 1f, 0f, 0.5f) : // Green if mapped correctly
                        new Color(1f, 0f, 0f, 0.5f);   // Red if mapping failed
                    
                    DrawLine(eventUI, hexCenterUI, lineColor, 
                        $"EventToHex_{closestHex}_{evt.GetHashCode()}");
                    
                    // Draw label showing mapping result
                    string label = $"E→H{closestHex}\n";
                    if (mappedHex >= 0)
                        label += $"Mapped:{mappedHex}";
                    else
                        label += "No map";
                    
                    DrawText(eventUI + new Vector2(0, -20), label, 
                        mappedHex == closestHex ? Color.green : Color.red, 
                        $"EventLabel_{evt.GetHashCode()}");
                }
            }
        }
        
        private void DrawCoordinateGrid()
        {
            Canvas.ForceUpdateCanvases();
            Vector2 viewportSize = new Vector2(sectorContainer.rect.width, sectorContainer.rect.height);
            
            // Draw grid lines at 0.1, 0.2, ..., 0.9 viewport coordinates
            Color gridColor = new Color(1f, 1f, 1f, 0.2f);
            
            for (float x = 0.1f; x < 1f; x += 0.1f)
            {
                Vector2 start = new Vector2(x * viewportSize.x, 0);
                Vector2 end = new Vector2(x * viewportSize.x, viewportSize.y);
                DrawLine(start, end, gridColor, $"Grid_V_{x}");
            }
            
            for (float y = 0.1f; y < 1f; y += 0.1f)
            {
                Vector2 start = new Vector2(0, y * viewportSize.y);
                Vector2 end = new Vector2(viewportSize.x, y * viewportSize.y);
                DrawLine(start, end, gridColor, $"Grid_H_{y}");
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
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            
            rect.sizeDelta = new Vector2(length, lineWidth);
            rect.anchoredPosition = start;
            rect.localRotation = Quaternion.Euler(0, 0, angle);
            
            Image img = lineObj.AddComponent<Image>();
            img.color = color;
            
            debugObjects.Add(lineObj);
        }
        
        private void DrawCircle(Vector2 center, float radius, Color color, string name)
        {
            GameObject circleObj = new GameObject(name);
            circleObj.transform.SetParent(debugContainer.transform, false);
            
            RectTransform rect = circleObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(radius * 2f, radius * 2f);
            rect.anchoredPosition = center;
            
            Image img = circleObj.AddComponent<Image>();
            img.color = color;
            // Create a simple circle sprite (or use a default UI sprite)
            // For now, we'll use a white sprite and tint it
            img.sprite = CreateCircleSprite(radius);
            
            debugObjects.Add(circleObj);
        }
        
        private void DrawText(Vector2 position, string text, Color color, string name)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(debugContainer.transform, false);
            
            RectTransform rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100f, 30f);
            rect.anchoredPosition = position;
            
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.color = color;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 10;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            debugObjects.Add(textObj);
        }
        
        private Sprite CreateCircleSprite(float radius)
        {
            int size = Mathf.CeilToInt(radius * 2f);
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radiusSq = radius * radius;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distSq = (new Vector2(x, y) - center).sqrMagnitude;
                    pixels[y * size + x] = distSq <= radiusSq ? Color.white : Color.clear;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}

