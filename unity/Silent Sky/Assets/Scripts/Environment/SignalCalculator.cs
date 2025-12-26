using System.Collections.Generic;
using UnityEngine;
using SilentSky.Unity.Visualization;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Calculates signal values for each segment by summing event values
    /// </summary>
    public class SignalCalculator : MonoBehaviour
    {
        [SerializeField] private FakeDataGenerator dataGenerator;
        [SerializeField] private SectorMap sectorMap; // Reference to SectorMap for hexagon positions
        [SerializeField] private ViewportRotationController rotationController; // Viewport rotation controller (auto-finds if not set)
        
        private float[] segmentSignals; // Signal value for each segment (0-18)
        public float currentTime = 0f; // Made public for EventVisualizer
        
        public float[] SegmentSignals => segmentSignals;
        public int SegmentCount => segmentSignals != null ? segmentSignals.Length : 0;
        
        private void Awake()
        {
            segmentSignals = new float[19]; // 19 hexagons
        }
        
        private void Start()
        {
            if (dataGenerator == null)
            {
                dataGenerator = FindObjectOfType<FakeDataGenerator>();
            }
            
            if (dataGenerator == null)
            {
                Debug.LogError("SignalCalculator: No FakeDataGenerator found!");
            }
            
            // Find SectorMap if not assigned
            if (sectorMap == null)
            {
                sectorMap = FindObjectOfType<SectorMap>();
            }
            
            // Initialize HexagonGridMapper with hexagon positions
            InitializeHexagonMapper();
            
            // Find rotation controller if not assigned (rotation is handled automatically via ViewportProjection)
            if (rotationController == null)
            {
                rotationController = FindObjectOfType<ViewportRotationController>();
            }
        }
        
        /// <summary>
        /// Initializes HexagonGridMapper with hexagon positions from SectorMap
        /// </summary>
        private void InitializeHexagonMapper()
        {
            if (sectorMap == null)
            {
                Debug.LogWarning("SignalCalculator: SectorMap not found. HexagonGridMapper will not be initialized.");
                return;
            }
            
            // Get hexagon positions using the same logic as SectorMap
            Vector2Int[] hexPositions = GetJWSTHexPositions();
            Vector2[] worldPositions = new Vector2[19];
            
            // Get hexSize from SectorMap (we'll need to access it via reflection or make it public)
            // For now, use a default or try to get it
            float hexSize = 80f; // Default
            var sectorMapType = typeof(SectorMap);
            var hexSizeField = sectorMapType.GetField("hexSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (hexSizeField != null)
            {
                hexSize = (float)hexSizeField.GetValue(sectorMap);
            }
            
            // Convert hex positions to world positions
            for (int i = 0; i < hexPositions.Length; i++)
            {
                worldPositions[i] = HexToWorldPosition(hexPositions[i].x, hexPositions[i].y, hexSize);
            }
            
            // Get viewport size from SectorMap's container
            // Try to get sectorContainer via reflection
            RectTransform container = null;
            var sectorContainerField = sectorMapType.GetField("sectorContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (sectorContainerField != null)
            {
                Transform sectorContainerTransform = sectorContainerField.GetValue(sectorMap) as Transform;
                if (sectorContainerTransform != null)
                {
                    container = sectorContainerTransform.GetComponent<RectTransform>();
                }
            }
            
            // Fallback: try to find by name
            if (container == null)
            {
                Transform found = sectorMap.transform.Find("SectorContainer");
                if (found != null)
                {
                    container = found.GetComponent<RectTransform>();
                }
            }
            
            // Last resort: use SectorMap's own RectTransform or default size
            if (container == null)
            {
                container = sectorMap.GetComponent<RectTransform>();
            }
            
            // Get actual container size - use rect if available, otherwise sizeDelta
            Vector2 viewportSize;
            if (container != null)
            {
                // Force canvas update to ensure rect is accurate
                Canvas.ForceUpdateCanvases();
                
                // Get actual rect size (accounts for anchors and parent sizing)
                float rectWidth = container.rect.width;
                float rectHeight = container.rect.height;
                
                if (rectWidth > 0f && rectHeight > 0f)
                {
                    viewportSize = new Vector2(rectWidth, rectHeight);
                }
                else
                {
                    // Fallback to sizeDelta if rect is not available yet
                    viewportSize = container.sizeDelta;
                    if (viewportSize.x == 0f || viewportSize.y == 0f)
                    {
                        viewportSize = new Vector2(800f, 800f); // Default fallback
                        Debug.LogWarning($"SignalCalculator: Container size not available, using default {viewportSize}");
                    }
                }
            }
            else
            {
                viewportSize = new Vector2(800f, 800f); // Default if container not found
                Debug.LogWarning($"SignalCalculator: Container not found, using default size {viewportSize}");
            }
            
            // Initialize the mapper
            HexagonGridMapper.Initialize(worldPositions, hexSize, viewportSize);
            
            Debug.Log($"SignalCalculator: Initialized with {worldPositions.Length} hexagons, hexSize={hexSize}px, viewport={viewportSize.x:F0}x{viewportSize.y:F0}");
        }
        
        /// <summary>
        /// Gets JWST hexagon positions (same as SectorMap)
        /// </summary>
        private Vector2Int[] GetJWSTHexPositions()
        {
            List<Vector2Int> positions = new List<Vector2Int>();
            
            // Center hexagon
            positions.Add(new Vector2Int(0, 0));
            
            // Ring 1: 6 hexagons
            positions.Add(new Vector2Int(1, 0));
            positions.Add(new Vector2Int(0, 1));
            positions.Add(new Vector2Int(-1, 1));
            positions.Add(new Vector2Int(-1, 0));
            positions.Add(new Vector2Int(0, -1));
            positions.Add(new Vector2Int(1, -1));
            
            // Ring 2: 12 hexagons at distance 2
            List<Vector2Int> ring2Positions = new List<Vector2Int>();
            for (int q = -3; q <= 3; q++)
            {
                for (int r = -3; r <= 3; r++)
                {
                    int dist = (Mathf.Abs(q) + Mathf.Abs(r) + Mathf.Abs(q + r)) / 2;
                    if (dist == 2)
                    {
                        ring2Positions.Add(new Vector2Int(q, r));
                    }
                }
            }
            
            ring2Positions.Sort((a, b) => {
                float angleA = Mathf.Atan2(a.y, a.x);
                float angleB = Mathf.Atan2(b.y, b.x);
                return angleB.CompareTo(angleA);
            });
            
            positions.AddRange(ring2Positions);
            return positions.ToArray();
        }
        
        /// <summary>
        /// Converts hexagonal axial coordinates to world position (same as SectorMap)
        /// </summary>
        private Vector2 HexToWorldPosition(int q, int r, float size)
        {
            float sqrt3 = Mathf.Sqrt(3f);
            float x = size * sqrt3 * (q + r * 0.5f);
            float y = size * 1.5f * r;
            return new Vector2(x, y);
        }
        
        private void Update()
        {
            // Update time (for testing - in real game this would come from environment)
            currentTime += Time.deltaTime;
            CalculateSignals();
        }
        
        /// <summary>
        /// Calculates signal values for all segments based on active events
        /// </summary>
        public void CalculateSignals()
        {
            // Reset all signals
            for (int i = 0; i < segmentSignals.Length; i++)
            {
                segmentSignals[i] = 0f;
            }
            
            if (dataGenerator == null)
            {
                return;
            }
            
            // Ensure mapper is initialized
            if (!HexagonGridMapper.IsInitialized())
            {
                InitializeHexagonMapper();
                if (!HexagonGridMapper.IsInitialized())
                {
                    Debug.LogWarning("SignalCalculator: HexagonGridMapper not initialized. Cannot calculate signals.");
                    return;
                }
            }
            
            // Get all active events
            List<SpaceEvent> activeEvents = dataGenerator.GetActiveEvents(currentTime);
            
            // Sum event values for each hexagon using unified mapping
            int eventsMapped = 0;
            int eventsUnmapped = 0;
            int eventsOutsideFOV = 0;
            foreach (var evt in activeEvents)
            {
                // Skip events outside the viewport FOV - they can't be in any hexagon
                if (!ViewportProjection.IsInViewport(evt.theta, evt.phi))
                {
                    eventsOutsideFOV++;
                    continue;
                }
                
                Vector2 viewportPos = ViewportProjection.ProjectToViewport(evt.theta, evt.phi);
                int hexagonIndex = SphereToHexagonMapper.GetHexagonForEvent(evt.theta, evt.phi);
                
                if (hexagonIndex >= 0 && hexagonIndex < segmentSignals.Length)
                {
                    segmentSignals[hexagonIndex] += evt.value;
                    eventsMapped++;
                }
                else if (hexagonIndex == -1)
                {
                    // Event is within FOV but outside hexagon coverage area
                    eventsUnmapped++;
                }
                else
                {
                    Debug.LogWarning($"SignalCalculator: Event at (θ={evt.theta:F2}, φ={evt.phi:F2}) mapped to invalid hexagon index {hexagonIndex}!");
                }
            }
            
            // Debug: Log signal distribution (only occasionally to avoid spam)
            if (Time.frameCount % 600 == 0) // Every ~10 seconds at 60fps
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder("Signals: ");
                for (int i = 0; i < segmentSignals.Length; i++)
                {
                    if (segmentSignals[i] > 0f)
                    {
                        sb.Append($"S{i}:{segmentSignals[i]:F1} ");
                    }
                }
                Debug.Log(sb.ToString());
            }
        }
        
        /// <summary>
        /// Gets signal value for a specific segment
        /// </summary>
        public float GetSignal(int segmentIndex)
        {
            if (segmentIndex >= 0 && segmentIndex < segmentSignals.Length)
            {
                return segmentSignals[segmentIndex];
            }
            return 0f;
        }
        
        /// <summary>
        /// Sets the current time (for external time control)
        /// </summary>
        public void SetTime(float time)
        {
            currentTime = time;
            CalculateSignals();
        }
    }
}

