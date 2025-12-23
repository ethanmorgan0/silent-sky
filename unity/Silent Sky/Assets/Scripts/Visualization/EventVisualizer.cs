using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Displays detected events and missed opportunities
    /// Creates its own UI if not provided
    /// </summary>
    public class EventVisualizer : MonoBehaviour
    {
        [Header("UI References (Optional - will create if not assigned)")]
        [SerializeField] private Transform eventListContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private Text discoveredCountText;
        [SerializeField] private Text missedCountText;
        
        [Header("Auto-Create Settings")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Vector2 panelPosition = new Vector2(-10, -10); // Top-right, closer to edge
        
        private List<GameObject> eventItems = new List<GameObject>();
        private EnvironmentState currentState;
        private Canvas parentCanvas;
        
        private void Awake()
        {
            Debug.Log($"EventVisualizer: Awake() called on GameObject '{gameObject.name}', enabled={enabled}, activeInHierarchy={gameObject.activeInHierarchy}");
        }
        
        private void Start()
        {
            Debug.Log($"EventVisualizer: Start() called on GameObject '{gameObject.name}'");
            
            if (!enabled)
            {
                Debug.LogWarning("EventVisualizer: Component is disabled!");
                return;
            }
            
            if (!gameObject.activeInHierarchy)
            {
                Debug.LogWarning("EventVisualizer: GameObject is inactive!");
                return;
            }
            
            // Find or create UI
            if (autoCreateUI)
            {
                Debug.Log("EventVisualizer: Auto-creating UI");
                CreateUI();
            }
            else
            {
                Debug.Log("EventVisualizer: Auto-create disabled, using assigned references");
            }
            
            // Subscribe to updates
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateEvents;
                Debug.Log("EventVisualizer: Subscribed to ZMQBridge updates");
            }
            else
            {
                Debug.LogWarning("EventVisualizer: ZMQBridge not found - events won't update");
            }
        }
        
        private void CreateUI()
        {
            Debug.Log("EventVisualizer: CreateUI() called");
            
            // Find Canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                parentCanvas = FindObjectOfType<Canvas>();
            }
            
            // Create Canvas if it doesn't exist
            if (parentCanvas == null)
            {
                Debug.LogWarning("EventVisualizer: No Canvas found. Creating one automatically.");
                GameObject canvasObj = new GameObject("Canvas");
                parentCanvas = canvasObj.AddComponent<Canvas>();
                parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Also create EventSystem if it doesn't exist (required for UI)
                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    GameObject eventSystemObj = new GameObject("EventSystem");
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    Debug.Log("EventVisualizer: Created EventSystem");
                }
                
                Debug.Log("EventVisualizer: Created Canvas");
            }
            else
            {
                Debug.Log($"EventVisualizer: Found Canvas '{parentCanvas.name}'");
            }
            
            // Create panel if we don't have a container
            if (eventListContainer == null)
            {
                Debug.Log("EventVisualizer: Creating panel");
                GameObject panel = new GameObject("EventVisualizerPanel");
                panel.transform.SetParent(parentCanvas.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                // Use center-top anchor for easier debugging
                panelRect.anchorMin = new Vector2(0.5f, 1f);
                panelRect.anchorMax = new Vector2(0.5f, 1f);
                panelRect.pivot = new Vector2(0.5f, 1f);
                panelRect.anchoredPosition = new Vector2(0f, -10f); // 10px from top, centered
                panelRect.sizeDelta = new Vector2(300f, 100f); // Bigger so it's obvious
                
                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.9f); // Very opaque black background
                
                eventListContainer = panel.transform;
                Debug.Log($"EventVisualizer: Panel created at position {panelPosition}, size {panelRect.sizeDelta}, color {panelImage.color}");
                Debug.Log($"EventVisualizer: Panel active={panel.activeSelf}, activeInHierarchy={panel.activeInHierarchy}");
            }
            else
            {
                Debug.Log("EventVisualizer: Using existing container");
            }
            
            // Create discovered count text if not assigned
            if (discoveredCountText == null)
            {
                Debug.Log("EventVisualizer: Creating discovered count text");
                GameObject discoveredObj = new GameObject("DiscoveredCountText");
                discoveredObj.transform.SetParent(eventListContainer, false);
                
                RectTransform rect = discoveredObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -15f);
                rect.sizeDelta = new Vector2(-20f, 40f); // Give it actual width
                
                discoveredCountText = discoveredObj.AddComponent<Text>();
                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont != null)
                {
                    discoveredCountText.font = defaultFont;
                }
                discoveredCountText.fontSize = 16;
                discoveredCountText.color = Color.white;
                discoveredCountText.alignment = TextAnchor.UpperLeft;
                discoveredCountText.text = "Discovered: 0";
                discoveredCountText.horizontalOverflow = HorizontalWrapMode.Overflow;
                discoveredCountText.verticalOverflow = VerticalWrapMode.Overflow;
                Debug.Log($"EventVisualizer: Discovered count text created, font={discoveredCountText.font != null}, text='{discoveredCountText.text}', color={discoveredCountText.color}");
            }
            else
            {
                Debug.Log("EventVisualizer: Using existing discovered count text");
            }
            
            // Create missed count text if not assigned
            if (missedCountText == null)
            {
                Debug.Log("EventVisualizer: Creating missed count text");
                GameObject missedObj = new GameObject("MissedCountText");
                missedObj.transform.SetParent(eventListContainer, false);
                
                RectTransform rect = missedObj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.anchoredPosition = new Vector2(0f, -55f);
                rect.sizeDelta = new Vector2(-20f, 40f); // Give it actual width
                
                missedCountText = missedObj.AddComponent<Text>();
                Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (defaultFont != null)
                {
                    missedCountText.font = defaultFont;
                }
                missedCountText.fontSize = 16;
                missedCountText.color = Color.red;
                missedCountText.alignment = TextAnchor.UpperLeft;
                missedCountText.text = "Missed: 0";
                missedCountText.horizontalOverflow = HorizontalWrapMode.Overflow;
                missedCountText.verticalOverflow = VerticalWrapMode.Overflow;
                Debug.Log($"EventVisualizer: Missed count text created, font={missedCountText.font != null}, text='{missedCountText.text}', color={missedCountText.color}");
            }
            else
            {
                Debug.Log("EventVisualizer: Using existing missed count text");
            }
            
            Debug.Log("EventVisualizer: UI creation complete");
        }
        
        private void UpdateEvents(EnvironmentState state)
        {
            currentState = state;
            
            if (state?.state == null) return;
            
            // Update counts
            if (discoveredCountText != null)
            {
                discoveredCountText.text = $"Discovered: {state.state.discovered_events?.Length ?? 0}";
            }
            
            if (missedCountText != null)
            {
                int missed = (state.state.events?.Length ?? 0) - (state.state.discovered_events?.Length ?? 0);
                missedCountText.text = $"Missed: {missed}";
            }
            
            // Update event list (simplified for Phase 1)
            // Full timeline visualization deferred
        }
        
        private void ClearEventList()
        {
            foreach (var item in eventItems)
            {
                Destroy(item);
            }
            eventItems.Clear();
        }
    }
}

