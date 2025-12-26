using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Visualization; // For SectorMap

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Visualizes space events as bright transient stars on the starfield
    /// </summary>
    public class EventVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SignalCalculator signalCalculator;
        [SerializeField] private FakeDataGenerator dataGenerator;
        [SerializeField] private GameObject starfieldBackground; // StarfieldBackground GameObject (auto-finds if not set)
        [SerializeField] private RectTransform sectorContainer; // SectorContainer - must match hexagon container (auto-finds if not set)
        [SerializeField] private ViewportRotationController rotationController; // Viewport rotation controller (auto-finds if not set)
        private RectTransform starfieldContainer; // Internal: RectTransform from starfieldBackground
        
        [Header("Visual Settings")]
        [SerializeField] private float eventStarSize = 0.5f; // Larger than regular stars
        [SerializeField] private Color eventStarColor = Color.yellow; // Bright yellow
        [SerializeField] private float eventStarBrightness = 1.5f; // Extra bright
        
        private GameObject eventContainer;
        private Dictionary<SpaceEvent, GameObject> eventToStarMap = new Dictionary<SpaceEvent, GameObject>();
        private Dictionary<SpaceEvent, Vector2> eventOffsets = new Dictionary<SpaceEvent, Vector2>(); // Store random offsets to prevent wobbling
        private HashSet<SpaceEvent> loggedEvents = new HashSet<SpaceEvent>(); // Track which events we've already logged
        private Sprite eventStarSprite;
        private float currentTime = 0f;
        
        private void Start()
        {
            if (signalCalculator == null)
            {
                signalCalculator = FindObjectOfType<SignalCalculator>();
            }
            
            if (dataGenerator == null)
            {
                dataGenerator = FindObjectOfType<FakeDataGenerator>();
            }
            
            // Find rotation controller if not assigned
            if (rotationController == null)
            {
                rotationController = FindObjectOfType<ViewportRotationController>();
            }
            
            // Subscribe to rotation changes to update event positions
            if (rotationController != null)
            {
                rotationController.OnRotationChanged += OnViewportRotationChanged;
            }
            
            // Find SectorContainer - this MUST be the same container as hexagons for alignment
            if (sectorContainer == null)
            {
                // Try to find by name
                GameObject containerObj = GameObject.Find("SectorContainer");
                if (containerObj != null)
                {
                    sectorContainer = containerObj.GetComponent<RectTransform>();
                }
                
                // Fallback: try to get from SectorMap via reflection
                if (sectorContainer == null)
                {
                    var sectorMap = FindObjectOfType<SilentSky.Unity.Visualization.SectorMap>();
                    if (sectorMap != null)
                    {
                        var sectorContainerField = typeof(SilentSky.Unity.Visualization.SectorMap).GetField("sectorContainer", 
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        if (sectorContainerField != null)
                        {
                            Transform containerTransform = sectorContainerField.GetValue(sectorMap) as Transform;
                            if (containerTransform != null)
                            {
                                sectorContainer = containerTransform.GetComponent<RectTransform>();
                            }
                        }
                    }
                }
            }
            
            if (sectorContainer == null)
            {
                Debug.LogError("EventVisualizer: Cannot find SectorContainer! Events will not align with hexagons. " +
                    "Please ensure SectorContainer exists in the scene or assign it in the Inspector.");
            }
            else
            {
                // SectorContainer found and ready for event positioning
            }
            
            // Get RectTransform from starfieldBackground GameObject (for rendering order, not positioning)
            if (starfieldBackground == null)
            {
                starfieldBackground = GameObject.Find("StarfieldBackground");
                if (starfieldBackground == null)
                {
                    StarfieldBackground starfieldComponent = FindObjectOfType<StarfieldBackground>();
                    if (starfieldComponent != null)
                    {
                        starfieldBackground = starfieldComponent.gameObject;
                    }
                }
            }
            
            if (starfieldBackground != null)
            {
                starfieldContainer = starfieldBackground.GetComponent<RectTransform>();
            }
            
            // Create event star sprite first (doesn't depend on container)
            eventStarSprite = CreateEventStarSprite();
            
            // Create event container directly as child of SectorContainer (same as hexagons)
            CreateEventContainer();
        }
        
        /// <summary>
        /// Creates EventContainer as a child of SectorContainer (same as hexagons) for perfect alignment
        /// </summary>
        private void CreateEventContainer()
        {
            if (sectorContainer == null)
            {
                Debug.LogError("EventVisualizer: Cannot create EventContainer - SectorContainer is null!");
                return;
            }
            
            // Create event container as child of SectorContainer (same container as hexagons)
            eventContainer = new GameObject("EventContainer");
            eventContainer.transform.SetParent(sectorContainer.transform, false);
            
            RectTransform eventContainerRect = eventContainer.AddComponent<RectTransform>();
            eventContainerRect.anchorMin = Vector2.zero;
            eventContainerRect.anchorMax = Vector2.one;
            eventContainerRect.sizeDelta = Vector2.zero;
            eventContainerRect.anchoredPosition = Vector2.zero;
            
            // Force layout update to ensure size is calculated
            Canvas.ForceUpdateCanvases();
            
            // Set sibling index to render above hexagons
            eventContainer.transform.SetAsLastSibling();
            
            // EventContainer created successfully
        }
        
        private void Update()
        {
            if (dataGenerator == null || signalCalculator == null)
            {
                // Try to find them if not set
                if (dataGenerator == null)
                {
                    dataGenerator = FindObjectOfType<FakeDataGenerator>();
                }
                if (signalCalculator == null)
                {
                    signalCalculator = FindObjectOfType<SignalCalculator>();
                }
                if (dataGenerator == null || signalCalculator == null)
                {
                    return;
                }
            }
            
            // Re-check starfieldContainer if it's still null (in case it wasn't ready at Start)
            if (starfieldContainer == null)
            {
                if (starfieldBackground == null)
                {
                    starfieldBackground = GameObject.Find("StarfieldBackground");
                    if (starfieldBackground == null)
                    {
                        StarfieldBackground starfieldComponent = FindObjectOfType<StarfieldBackground>();
                        if (starfieldComponent != null)
                        {
                            starfieldBackground = starfieldComponent.gameObject;
                        }
                    }
                }
                
                if (starfieldBackground != null)
                {
                    starfieldContainer = starfieldBackground.GetComponent<RectTransform>();
                }
                
                // If still null, try to use the eventContainer's parent
                if (starfieldContainer == null && eventContainer != null && eventContainer.transform.parent != null)
                {
                    starfieldContainer = eventContainer.transform.parent.GetComponent<RectTransform>();
                    if (starfieldContainer != null)
                    {
                        Debug.Log($"EventVisualizer: Found starfieldContainer from eventContainer parent: {starfieldContainer.name}");
                    }
                }
            }
            
            // Get current time from SignalCalculator
            currentTime = signalCalculator.currentTime;
            
            // Get all active events
            List<SpaceEvent> activeEvents = dataGenerator.GetActiveEvents(currentTime);
            
            // Debug logging (occasionally)
            if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
            {
                // Event visualization update (use HexagonMappingDebugger for detailed visualization)
            }
            
            // Update visualization
            UpdateEventVisualization(activeEvents);
        }
        
        /// <summary>
        /// Updates the visual representation of events
        /// </summary>
        private void UpdateEventVisualization(List<SpaceEvent> activeEvents)
        {
            // Create a set of currently active events for quick lookup
            HashSet<SpaceEvent> activeEventSet = new HashSet<SpaceEvent>(activeEvents);
            
            // Remove stars for events that are no longer active
            List<SpaceEvent> toRemove = new List<SpaceEvent>();
            foreach (var kvp in eventToStarMap)
            {
                if (!activeEventSet.Contains(kvp.Key))
                {
                    Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var evt in toRemove)
            {
                eventToStarMap.Remove(evt);
                eventOffsets.Remove(evt); // Also remove stored offset to prevent memory leak
                loggedEvents.Remove(evt); // Also remove from logged events
            }
            
            // Add/update stars for active events (only if within viewport FOV)
            foreach (var evt in activeEvents)
            {
                // Only visualize events that are within the viewport FOV
                if (!ViewportProjection.IsInViewport(evt.theta, evt.phi))
                {
                    // Event is outside viewport - remove if it exists, don't create new one
                    if (eventToStarMap.ContainsKey(evt))
                    {
                        Destroy(eventToStarMap[evt]);
                        eventToStarMap.Remove(evt);
                        eventOffsets.Remove(evt);
                        loggedEvents.Remove(evt);
                    }
                    continue;
                }
                
                if (!eventToStarMap.ContainsKey(evt))
                {
                    CreateEventStar(evt);
                    // Event star created (use HexagonMappingDebugger for visualization)
                }
                else
                {
                    // Update position (in case event moved, though they're static for now)
                    UpdateEventStarPosition(evt, eventToStarMap[evt]);
                }
            }
            
            // Debug if we have events but no stars
            if (activeEvents.Count > 0 && eventToStarMap.Count == 0)
            {
                Debug.LogWarning($"EventVisualizer: Have {activeEvents.Count} active events but no stars created! " +
                    $"eventContainer={eventContainer != null}, starfieldContainer={starfieldContainer != null}, " +
                    $"eventStarSprite={eventStarSprite != null}");
            }
            
            // Debug star count mismatch
            if (activeEvents.Count != eventToStarMap.Count)
            {
                Debug.LogWarning($"EventVisualizer: Mismatch - {activeEvents.Count} active events but {eventToStarMap.Count} stars in map!");
            }
        }
        
        /// <summary>
        /// Creates a visual star for an event
        /// </summary>
        private void CreateEventStar(SpaceEvent evt)
        {
            if (eventContainer == null)
            {
                Debug.LogError("EventVisualizer: Cannot create event star - eventContainer is null!");
                return;
            }
            
            if (eventStarSprite == null)
            {
                Debug.LogError("EventVisualizer: eventStarSprite is null! Cannot render event.");
                return;
            }
            
            GameObject starObj = new GameObject($"Event_{evt.GetHashCode()}");
            starObj.transform.SetParent(eventContainer.transform, false);
            
            Image starImage = starObj.AddComponent<Image>();
            starImage.sprite = eventStarSprite;
            // Multiply color by brightness, but clamp to valid range (0-1)
            Color finalColor = new Color(
                Mathf.Clamp01(eventStarColor.r * eventStarBrightness),
                Mathf.Clamp01(eventStarColor.g * eventStarBrightness),
                Mathf.Clamp01(eventStarColor.b * eventStarBrightness),
                1f // Fully opaque
            );
            starImage.color = finalColor;
            starImage.raycastTarget = false;
            
            RectTransform starRect = starObj.GetComponent<RectTransform>();
            starRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Size the star - make it larger and more visible
            // Use the eventContainer's parent (StarContainer) for size calculation
            RectTransform sizeContainer = eventContainer.transform.parent != null ? 
                eventContainer.transform.parent.GetComponent<RectTransform>() : starfieldContainer;
            
            float containerSize = sizeContainer != null ? 
                Mathf.Max(sizeContainer.rect.width, sizeContainer.rect.height) : 1000f;
            if (containerSize == 0f) containerSize = 1000f;
            
            // Make event stars much larger - at least 20 pixels, or 2% of container size
            float starPixelSize = Mathf.Max(20f, eventStarSize * containerSize / 100f);
            starRect.sizeDelta = Vector2.one * starPixelSize;
            
            // Event star GameObject created (use HexagonMappingDebugger for visualization)
            
            // Position based on spherical coordinates
            UpdateEventStarPosition(evt, starObj);
            
            eventToStarMap[evt] = starObj;
        }
        
        /// <summary>
        /// Updates the UI position of an event star based on its spherical coordinates
        /// </summary>
        private void UpdateEventStarPosition(SpaceEvent evt, GameObject starObj)
        {
            if (starObj == null)
            {
                Debug.LogWarning("EventVisualizer: Cannot position event - starObj is null");
                return;
            }
            
            RectTransform starRect = starObj.GetComponent<RectTransform>();
            if (starRect == null) return;
            
            // Use SectorContainer for positioning - this is the same container as hexagons
            // This ensures events align perfectly with hexagons since they use the same coordinate system
            if (sectorContainer == null)
            {
                Debug.LogWarning("EventVisualizer: Cannot position event - SectorContainer is null!");
                return;
            }
            
            RectTransform containerToUse = sectorContainer;
            
            // Position events using the same viewport projection as signal calculation
            // This ensures visual consistency: events visible in a hexagon will have signals counted there
            Vector2 viewportPos = ViewportProjection.ProjectToViewport(evt.theta, evt.phi);
            float normalizedX = viewportPos.x;
            float normalizedY = viewportPos.y;
            
            // Note: viewportPos should already be in [0, 1] range since we only render events within FOV
            // But we'll clamp anyway to be safe
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
            
            // Add a small random offset to prevent events from clustering at exact same position
            // Store the offset so it doesn't change every frame (prevents wobbling)
            Vector2 offset;
            if (!eventOffsets.ContainsKey(evt))
            {
                // Generate a small random offset once when event is first positioned
                float offsetRange = 0.01f; // 1% of container size - small enough to not move events far
                offset = new Vector2(
                    Random.Range(-offsetRange, offsetRange),
                    Random.Range(-offsetRange, offsetRange)
                );
                eventOffsets[evt] = offset;
            }
            else
            {
                // Use stored offset to prevent wobbling
                offset = eventOffsets[evt];
            }
            
            normalizedX += offset.x;
            normalizedY += offset.y;
            
            // Clamp again after adding offset
            normalizedX = Mathf.Clamp01(normalizedX);
            normalizedY = Mathf.Clamp01(normalizedY);
            
            // Ensure parent is eventContainer
            if (starObj.transform.parent != eventContainer.transform)
            {
                starObj.transform.SetParent(eventContainer.transform, false);
            }
            
            // Position using anchors (same approach as regular stars in StarfieldBackground)
            // The anchor determines position relative to the parent (eventContainer)
            starRect.anchorMin = new Vector2(normalizedX, normalizedY);
            starRect.anchorMax = new Vector2(normalizedX, normalizedY);
            starRect.pivot = new Vector2(0.5f, 0.5f);
            starRect.anchoredPosition = Vector2.zero;
            
            // Ensure the Image component is properly configured
            Image img = starObj.GetComponent<Image>();
            if (img != null)
            {
                img.enabled = true;
                img.raycastTarget = false;
                // Make sure the sprite is set
                if (img.sprite == null && eventStarSprite != null)
                {
                    img.sprite = eventStarSprite;
                }
            }
            
            // Force update the rect transform to ensure positioning is applied
            Canvas.ForceUpdateCanvases();
            
            // Verify the star is actually positioned correctly
            Vector3 worldPos = starRect.position;
            Rect containerRect = containerToUse.rect;
            
            // Event positioned successfully (no logging needed - use HexagonMappingDebugger for visualization)
        }
        
        /// <summary>
        /// Creates a bright star sprite for events
        /// </summary>
        private Sprite CreateEventStarSprite()
        {
            int size = 16; // Larger than regular stars for visibility
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 1f;
            
            // Create a bright circular star with soft edges
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = 1f - Mathf.Clamp01((dist - radius * 0.7f) / (radius * 0.3f));
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
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
            
            // Ring 2: 12 hexagons
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
        
        /// <summary>
        /// Called when viewport rotation changes - updates all event positions
        /// </summary>
        private void OnViewportRotationChanged(Vector2 rotation)
        {
            // Update positions of all existing events when rotation changes
            foreach (var kvp in eventToStarMap)
            {
                if (kvp.Value != null)
                {
                    UpdateEventStarPosition(kvp.Key, kvp.Value);
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from rotation changes
            if (rotationController != null)
            {
                rotationController.OnRotationChanged -= OnViewportRotationChanged;
            }
        }
    }
}

