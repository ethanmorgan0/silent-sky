using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Visualizes 8 sky sectors in a circular or grid layout
    /// </summary>
    public class SectorMap : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private bool useCircularLayout = true;
        [SerializeField] private float radius = 200f;
        [SerializeField] private Transform sectorContainer;
        [SerializeField] private GameObject sectorPrefab;
        
        [Header("Colors")]
        [SerializeField] private Color lowUncertaintyColor = Color.green;
        [SerializeField] private Color highUncertaintyColor = Color.red;
        [SerializeField] private Color defaultColor = Color.white;
        
        private List<SectorDisplay> sectors = new List<SectorDisplay>();
        private EnvironmentState currentState;
        
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
            
            for (int i = 0; i < 8; i++)
            {
                GameObject sectorObj = Instantiate(sectorPrefab, sectorContainer);
                SectorDisplay display = sectorObj.GetComponent<SectorDisplay>();
                if (display == null)
                {
                    display = sectorObj.AddComponent<SectorDisplay>();
                }
                
                display.Initialize(i);
                sectors.Add(display);
                
                // Position sectors
                if (useCircularLayout)
                {
                    float angle = (i / 8f) * 2f * Mathf.PI;
                    sectorObj.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    );
                }
                else
                {
                    // Grid layout
                    int row = i / 4;
                    int col = i % 4;
                    sectorObj.transform.localPosition = new Vector3(
                        (col - 1.5f) * 100f,
                        (row - 0.5f) * 100f,
                        0f
                    );
                }
            }
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
                    
                    // Create 4 border edges
                    Color borderColor = new Color(0f, 1f, 1f, 0.9f);
                    float borderWidth = 3f;
                    
                    // Top edge
                    CreateBorderEdgeHelper(borderContainer, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), 
                        new Vector2(0f, 0f), new Vector2(0f, borderWidth), borderColor);
                    
                    // Bottom edge
                    CreateBorderEdgeHelper(borderContainer, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f),
                        new Vector2(0f, -borderWidth), new Vector2(0f, borderWidth), borderColor);
                    
                    // Left edge
                    CreateBorderEdgeHelper(borderContainer, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f),
                        new Vector2(-borderWidth, 0f), new Vector2(borderWidth, 0f), borderColor);
                    
                    // Right edge
                    CreateBorderEdgeHelper(borderContainer, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f),
                        new Vector2(0f, 0f), new Vector2(borderWidth, 0f), borderColor);
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

