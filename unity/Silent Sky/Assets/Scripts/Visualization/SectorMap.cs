using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;
using SilentSky.Unity.Utils;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Visualizes 19 sky sectors in a JWST-style hexagonal honeycomb layout
    /// </summary>
    public class SectorMap : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private bool useHexagonalLayout = true;
        [SerializeField] private float hexSize = 80f; // Size of each hexagon
        [SerializeField] private Transform sectorContainer;
        [SerializeField] private GameObject sectorPrefab;
        
        private const int NUM_SECTORS = 19;
        
        [Header("Colors")]
        [SerializeField] private Color lowUncertaintyColor = Color.green;
        [SerializeField] private Color highUncertaintyColor = Color.red;
        [SerializeField] private Color defaultColor = Color.white;
        
        private List<SectorDisplay> sectors = new List<SectorDisplay>();
        private EnvironmentState currentState;
        private static Sprite cachedHexagonSprite;
        
        private void Start()
        {
            InitializeSectors();
            
            // Subscribe to state updates
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateSectors;
            }
        }
        
        private void InitializeSectors()
        {
            if (sectorPrefab == null || sectorContainer == null)
            {
                Debug.LogWarning("SectorMap: Missing prefab or container");
                return;
            }
            
            // Generate or reuse hexagon sprite (cache it to avoid regenerating)
            if (cachedHexagonSprite == null)
            {
                cachedHexagonSprite = HexagonSpriteGenerator.CreateHexagonSprite(hexSize);
            }
            Sprite hexagonSprite = cachedHexagonSprite;
            
            // JWST pattern: 18 hexagons in honeycomb layout
            // Center: (0, 0)
            // Ring 1: 6 hexagons around center
            // Ring 2: 12 hexagons in outer ring
            Vector2Int[] hexPositions = GetJWSTHexPositions();
            
            for (int i = 0; i < NUM_SECTORS; i++)
            {
                GameObject sectorObj = Instantiate(sectorPrefab, sectorContainer);
                SectorDisplay display = sectorObj.GetComponent<SectorDisplay>();
                if (display == null)
                {
                    display = sectorObj.AddComponent<SectorDisplay>();
                }
                
                // Set hexagon sprite if Image component exists
                Image img = sectorObj.GetComponent<Image>();
                if (img != null && hexagonSprite != null)
                {
                    img.sprite = hexagonSprite;
                    img.type = Image.Type.Simple;
                    img.preserveAspect = false; // Disable to ensure exact size matching
                    
                    // Ensure RectTransform size matches hexagon size
                    // For flat-top hexagon with radius hexSize:
                    // Width (flat edge to flat edge) = hexSize * sqrt(3)
                    // Height (point to point) = hexSize * 2
                    RectTransform rect = sectorObj.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        float sqrt3 = Mathf.Sqrt(3f);
                        float hexWidth = hexSize * sqrt3; // Width of flat-top hexagon (flat edge to flat edge)
                        float hexHeight = hexSize * 2f; // Height of flat-top hexagon (point to point)
                        rect.sizeDelta = new Vector2(hexWidth, hexHeight);
                    }
                }
                
                display.Initialize(i);
                sectors.Add(display);
                
                // Position sectors using hexagonal grid
                if (useHexagonalLayout && i < hexPositions.Length)
                {
                    Vector2Int hexCoord = hexPositions[i];
                    Vector2 worldPos = HexToWorldPosition(hexCoord.x, hexCoord.y, hexSize);
                    sectorObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0f);
                }
                else
                {
                    // Fallback: circular layout
                    float angle = (i / (float)NUM_SECTORS) * 2f * Mathf.PI;
                    float radius = hexSize * 2.5f;
                    sectorObj.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    );
                }
            }
        }
        
        /// <summary>
        /// Gets the JWST-style hexagonal positions for 19 sectors
        /// Returns axial coordinates (q, r) for each hexagon
        /// JWST pattern: 1 center + 6 in ring 1 + 12 in ring 2 = 19 total
        /// </summary>
        private Vector2Int[] GetJWSTHexPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            // Center hexagon (starting point for tutorial/progression)
            positions.Add(new Vector2Int(0, 0));
            
            // Ring 1: 6 hexagons at distance 1 from center (all positions where |q|+|r|+|q+r| = 2)
            // These form a complete ring around the center
            positions.Add(new Vector2Int(1, 0));   // Right
            positions.Add(new Vector2Int(0, 1));   // Top
            positions.Add(new Vector2Int(-1, 1));  // Top-left
            positions.Add(new Vector2Int(-1, 0));  // Left
            positions.Add(new Vector2Int(0, -1));  // Bottom
            positions.Add(new Vector2Int(1, -1));  // Bottom-right
            
            // Ring 2: 12 hexagons at distance 2 from center
            // Generate all positions at distance 2 programmatically to ensure we have all 12
            List<Vector2Int> ring2Positions = new List<Vector2Int>();
            for (int q = -3; q <= 3; q++)
            {
                for (int r = -3; r <= 3; r++)
                {
                    // Calculate distance using axial coordinate formula
                    int dist = (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(q + r)) / 2;
                    if (dist == 2)
                    {
                        ring2Positions.Add(new Vector2Int(q, r));
                    }
                }
            }
            
            // Sort by angle to arrange clockwise from top
            ring2Positions.Sort((a, b) => {
                float angleA = Mathf.Atan2(a.y, a.x);
                float angleB = Mathf.Atan2(b.y, b.x);
                return angleB.CompareTo(angleA); // Sort descending (top first)
            });
            
            positions.AddRange(ring2Positions);
            
            // Verify we have exactly 19 positions (1 center + 6 ring 1 + 12 ring 2)
            if (positions.Count != NUM_SECTORS)
            {
                Debug.LogError($"Expected {NUM_SECTORS} hexagon positions, got {positions.Count}");
            }
            
            return positions.ToArray();
        }
        
        /// <summary>
        /// Converts hexagonal axial coordinates (q, r) to world position
        /// For flat-top hexagons (pointy-top), where size is the radius from center to vertex
        /// </summary>
        private Vector2 HexToWorldPosition(int q, int r, float size)
        {
            // For flat-top hexagons, the spacing is:
            // Horizontal spacing = size * sqrt(3)
            // Vertical spacing = size * 1.5
            float sqrt3 = Mathf.Sqrt(3f);
            float x = size * sqrt3 * (q + r * 0.5f);
            float y = size * 1.5f * r;
            return new Vector2(x, y);
        }
        
        private void UpdateSectors(EnvironmentState state)
        {
            currentState = state;
            
            if (state?.state?.sectors == null)
            {
                Debug.LogWarning("SectorMap: UpdateSectors called with null state or sectors");
                return;
            }
            
            Debug.Log($"SectorMap: Updating {state.state.sectors.Length} sectors, have {sectors.Count} SectorDisplay components");
            
            for (int i = 0; i < sectors.Count && i < state.state.sectors.Length; i++)
            {
                var sectorData = state.state.sectors[i];
                sectors[i].UpdateDisplay(sectorData);
            }
        }
        
        private Color GetSectorColor(float confidence)
        {
            // Interpolate between colors based on confidence
            float uncertainty = 1f - confidence;
            return Color.Lerp(lowUncertaintyColor, highUncertaintyColor, uncertainty);
        }
        
        /// <summary>
        /// Updates hexagon colors based on signal values
        /// Called by SignalVisualizer to display signals
        /// </summary>
        public void UpdateSignals(float[] signals, float minSignal, float maxSignal, 
            Color minColor, Color maxColor)
        {
            if (signals == null || signals.Length != NUM_SECTORS)
            {
                Debug.LogWarning($"SectorMap: Invalid signals array (expected {NUM_SECTORS}, got {signals?.Length ?? 0})");
                return;
            }
            
            for (int i = 0; i < sectors.Count && i < signals.Length; i++)
            {
                float signal = signals[i];
                float normalizedSignal = Mathf.Clamp01((signal - minSignal) / (maxSignal - minSignal));
                Color signalColor = Color.Lerp(minColor, maxColor, normalizedSignal);
                
                // Update hexagon color directly
                if (sectors[i] != null)
                {
                    sectors[i].UpdateSignalColor(signalColor, signal);
                }
            }
        }
    }
    
    /// <summary>
    /// Individual sector display component
    /// </summary>
    public class SectorDisplay : MonoBehaviour
    {
        [SerializeField] private Text sectorLabel;
        [SerializeField] private Image sectorImage;
        [SerializeField] private Text readingText;
        
        private int sectorId;
        
        public void Initialize(int id)
        {
            sectorId = id;
            
            // Auto-find components if not assigned in Inspector
            if (sectorImage == null)
                sectorImage = GetComponent<Image>();
            
            if (sectorLabel == null || readingText == null)
            {
                var texts = GetComponentsInChildren<Text>(true);
                Debug.Log($"SectorDisplay {id}: Found {texts.Length} Text component(s)");
                
                if (texts.Length > 0)
                {
                    // If only one text, use it for readings (more important than label)
                    if (texts.Length == 1)
                    {
                        readingText = texts[0];
                        // Label is optional - we'll skip it if only one text exists
                        Debug.Log($"SectorDisplay {id}: Using single Text component for readings");
                    }
                    else
                    {
                        // Multiple texts: first for label, second for reading
                        if (sectorLabel == null)
                            sectorLabel = texts[0];
                        if (readingText == null)
                            readingText = texts[1];
                    }
                }
            }
            
            // Set label only if we have a separate label component
            if (sectorLabel != null && sectorLabel != readingText)
            {
                sectorLabel.text = $"Sector {id}";
                
                // Auto-position label at top-center
                var labelRect = sectorLabel.GetComponent<RectTransform>();
                if (labelRect != null)
                {
                    labelRect.anchorMin = new Vector2(0.5f, 1f);
                    labelRect.anchorMax = new Vector2(0.5f, 1f);
                    labelRect.pivot = new Vector2(0.5f, 1f);
                    labelRect.anchoredPosition = new Vector2(0f, -15f);
                    labelRect.sizeDelta = new Vector2(100f, 20f); // Ensure it has size
                }
            }
            
            // Auto-position reading text in center
            if (readingText != null)
            {
                var readingRect = readingText.GetComponent<RectTransform>();
                if (readingRect != null)
                {
                    readingRect.anchorMin = new Vector2(0.5f, 0.5f);
                    readingRect.anchorMax = new Vector2(0.5f, 0.5f);
                    readingRect.pivot = new Vector2(0.5f, 0.5f);
                    readingRect.anchoredPosition = Vector2.zero; // Center
                    readingRect.sizeDelta = new Vector2(100f, 20f); // Ensure it has size
                }
            }
            
            if (sectorImage == null)
            {
                Debug.LogWarning($"SectorDisplay {id}: No Image component found - add Image component to prefab");
            }
            
            if (readingText == null)
            {
                Debug.LogWarning($"SectorDisplay {id}: No readingText found - create a Text component for readings");
            }
            else
            {
                Debug.Log($"SectorDisplay {id}: readingText assigned to {readingText.name}");
                // Ensure text is visible
                readingText.color = Color.white;
                readingText.enabled = true;
                readingText.gameObject.SetActive(true);
                // Set initial placeholder text so we can see it
                readingText.text = "---";
                Debug.Log($"SectorDisplay {id}: Text component enabled, color={readingText.color}, active={readingText.gameObject.activeSelf}");
            }
        }
        
        public void UpdateDisplay(SectorData data)
        {
            if (sectorImage != null)
            {
                // Color based on confidence with ambiguity palette
                float confidence = data.sensor_confidence;
                
                Color sectorColor;
                if (confidence > 0.7f)
                {
                    // High confidence = green (likely signal)
                    sectorColor = Color.Lerp(new Color(0.2f, 0.8f, 0.2f), new Color(0f, 1f, 0f), (confidence - 0.7f) / 0.3f);
                }
                else if (confidence > 0.4f)
                {
                    // Medium confidence = yellow (ambiguous)
                    float t = (confidence - 0.4f) / 0.3f;
                    sectorColor = Color.Lerp(new Color(0.6f, 0.6f, 0.2f), new Color(0.8f, 0.8f, 0.2f), t);
                }
                else
                {
                    // Low confidence = dark/muted (low info)
                    float t = confidence / 0.4f;
                    sectorColor = Color.Lerp(new Color(0.2f, 0.2f, 0.2f), new Color(0.4f, 0.4f, 0.3f), t);
                }
                
                // Visual indicator: if agent is observing this sector, show cyan border outline
                if (data.is_observing)
                {
                    // Add subtle cyan tint to the sector itself
                    sectorColor = Color.Lerp(sectorColor, new Color(0.3f, 1f, 1f, 1f), 0.2f);
                }
                
                // Always find or create border container, then show/hide based on is_observing
                Transform borderContainer = transform.Find("BorderContainer");
                if (borderContainer == null && data.is_observing)
                {
                    // Create container for border pieces only when needed
                    GameObject container = new GameObject("BorderContainer");
                    container.transform.SetParent(transform, false);
                    RectTransform containerRect = container.AddComponent<RectTransform>();
                    containerRect.anchorMin = Vector2.zero;
                    containerRect.anchorMax = Vector2.one;
                    containerRect.sizeDelta = Vector2.zero;
                    containerRect.anchoredPosition = Vector2.zero;
                    borderContainer = container.transform;
                    
                    // Create 6 border edges for hexagon
                    CreateHexagonBorder(borderContainer);
                }
                
                // Show or hide border container based on is_observing
                if (borderContainer != null)
                {
                    borderContainer.gameObject.SetActive(data.is_observing);
                }
                
                // Always set the sector color (with or without cyan tint based on is_observing)
                sectorImage.color = sectorColor;
                
                // Ensure sector image is always visible
                sectorImage.enabled = true;
                sectorImage.gameObject.SetActive(true);
            }
            
            if (readingText != null)
            {
                // Calculate likelihood of sighting an event
                // Combines sensor reading (signal strength) with confidence
                // Higher reading + higher confidence = higher likelihood
                float likelihood = data.sensor_reading * data.sensor_confidence;
                readingText.text = $"{likelihood:P0}"; // Display as percentage (e.g., "45%")
                
                // Ensure text stays visible
                if (readingText.color.a < 0.1f)
                    readingText.color = Color.white;
                
                // Debug log updates for first sector
                if (sectorId == 0)
                {
                    Debug.Log($"SectorDisplay {sectorId}: Updated likelihood to '{readingText.text}' on {readingText.name}, visible={readingText.gameObject.activeInHierarchy}");
                }
            }
            else
            {
                if (sectorId == 0) // Only log for first sector to avoid spam
                {
                    Debug.LogWarning($"SectorDisplay {sectorId}: No readingText found - create a Text component for readings");
                }
            }
        }
        
        /// <summary>
        /// Updates hexagon color and text based on signal value
        /// Called by SectorMap.UpdateSignals()
        /// </summary>
        public void UpdateSignalColor(Color signalColor, float signalValue)
        {
            if (sectorImage != null)
            {
                sectorImage.color = signalColor;
                sectorImage.enabled = true;
                sectorImage.gameObject.SetActive(true);
            }
            
            if (readingText != null)
            {
                readingText.text = $"{signalValue:F0}";
                readingText.color = Color.white;
            }
        }
        
        /// <summary>
        /// Creates a hexagonal border with 6 edges
        /// </summary>
        private void CreateHexagonBorder(Transform parent)
        {
            Color borderColor = new Color(0f, 1f, 1f, 0.9f); // Cyan
            float borderWidth = 3f;
            
            // Get the RectTransform of the parent to determine size
            RectTransform parentRect = parent.parent.GetComponent<RectTransform>();
            if (parentRect == null) return;
            
            float width = parentRect.rect.width;
            float height = parentRect.rect.height;
            float radius = Mathf.Min(width, height) * 0.5f;
            
            // Hexagon has 6 edges at 60° intervals
            // Start at top (90°) and go clockwise
            for (int i = 0; i < 6; i++)
            {
                float angle1 = (90f - i * 60f) * Mathf.Deg2Rad;
                float angle2 = (90f - (i + 1) * 60f) * Mathf.Deg2Rad;
                
                Vector2 start = new Vector2(
                    Mathf.Cos(angle1) * radius,
                    Mathf.Sin(angle1) * radius
                );
                Vector2 end = new Vector2(
                    Mathf.Cos(angle2) * radius,
                    Mathf.Sin(angle2) * radius
                );
                
                // Create edge as a line from start to end
                CreateHexagonBorderEdge(parent, $"Edge{i}", start, end, borderWidth, borderColor);
            }
        }
        
        /// <summary>
        /// Creates a single border edge for a hexagon
        /// </summary>
        private void CreateHexagonBorderEdge(Transform parent, string name, Vector2 start, Vector2 end, float width, Color color)
        {
            GameObject edge = new GameObject(name);
            edge.transform.SetParent(parent, false);
            Image edgeImage = edge.AddComponent<Image>();
            RectTransform edgeRect = edge.GetComponent<RectTransform>();
            
            // Calculate position (midpoint) and rotation
            Vector2 midpoint = (start + end) * 0.5f;
            Vector2 direction = (end - start).normalized;
            float length = Vector2.Distance(start, end);
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            
            // Set up RectTransform
            edgeRect.anchorMin = new Vector2(0.5f, 0.5f);
            edgeRect.anchorMax = new Vector2(0.5f, 0.5f);
            edgeRect.pivot = new Vector2(0.5f, 0.5f);
            edgeRect.anchoredPosition = midpoint;
            edgeRect.sizeDelta = new Vector2(length, width);
            edgeRect.localRotation = Quaternion.Euler(0, 0, angle);
            
            edgeImage.color = color;
            edgeImage.raycastTarget = false;
        }
        
        private void CreateBorderEdgeHelper(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, 
            Vector2 offsetMin, Vector2 sizeDelta, Color color)
        {
            GameObject edge = new GameObject(name);
            edge.transform.SetParent(parent, false);
            Image edgeImage = edge.AddComponent<Image>();
            RectTransform edgeRect = edge.GetComponent<RectTransform>();
            edgeRect.anchorMin = anchorMin;
            edgeRect.anchorMax = anchorMax;
            edgeRect.anchoredPosition = offsetMin;
            edgeRect.sizeDelta = sizeDelta;
            edgeImage.color = color;
            edgeImage.raycastTarget = false;
        }
    }
}

