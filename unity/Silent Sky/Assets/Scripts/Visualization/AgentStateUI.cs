using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Displays current agent state and action
    /// Creates its own UI if not provided
    /// </summary>
    public class AgentStateUI : MonoBehaviour
    {
        [Header("UI References (Optional - will create if not assigned)")]
        [SerializeField] private Text timestepText;
        [SerializeField] private Text timeRemainingText;
        [SerializeField] private Text currentActionText;
        [SerializeField] private Slider timeRemainingSlider;
        [SerializeField] private Slider budgetSlider;
        
        [Header("Auto-Create Settings")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Vector2 panelPosition = new Vector2(0f, -10f); // Bottom-center
        
        private Canvas parentCanvas;
        
        private void Start()
        {
            // Find or create UI
            if (autoCreateUI)
            {
                CreateUI();
            }
            
            // Subscribe to updates
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateUI;
            }
        }
        
        private void CreateUI()
        {
            // Find Canvas
            parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                parentCanvas = FindObjectOfType<Canvas>();
            }
            
            if (parentCanvas == null)
            {
                Debug.LogError("AgentStateUI: No Canvas found. Create a Canvas in the scene.");
                return;
            }
            
            // Create panel container
            Transform panelContainer = null;
            if (timestepText == null || timeRemainingText == null || timeRemainingSlider == null || budgetSlider == null)
            {
                GameObject panel = new GameObject("AgentStateUIPanel");
                panel.transform.SetParent(parentCanvas.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.5f, 0f);
                panelRect.anchorMax = new Vector2(0.5f, 0f);
                panelRect.pivot = new Vector2(0.5f, 0f);
                panelRect.anchoredPosition = panelPosition;
                panelRect.sizeDelta = new Vector2(400f, 120f);
                
                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.7f);
                
                panelContainer = panel.transform;
            }
            
            // Create timestep text
            if (timestepText == null)
            {
                GameObject obj = new GameObject("TimestepText");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f, -10f);
                rect.sizeDelta = new Vector2(0f, 30f);
                
                timestepText = obj.AddComponent<Text>();
                timestepText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                timestepText.fontSize = 14;
                timestepText.color = Color.white;
                timestepText.alignment = TextAnchor.UpperLeft;
                timestepText.text = "Step: 0";
            }
            
            // Create time remaining text
            if (timeRemainingText == null)
            {
                GameObject obj = new GameObject("TimeRemainingText");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-10f, -10f);
                rect.sizeDelta = new Vector2(0f, 30f);
                
                timeRemainingText = obj.AddComponent<Text>();
                timeRemainingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                timeRemainingText.fontSize = 14;
                timeRemainingText.color = Color.white;
                timeRemainingText.alignment = TextAnchor.UpperRight;
                timeRemainingText.text = "Time: 100%";
            }
            
            // Create time remaining slider
            if (timeRemainingSlider == null)
            {
                GameObject obj = new GameObject("TimeRemainingSlider");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, -20f);
                rect.sizeDelta = new Vector2(-20f, 20f);
                
                timeRemainingSlider = obj.AddComponent<Slider>();
                timeRemainingSlider.minValue = 0f;
                timeRemainingSlider.maxValue = 1f;
                timeRemainingSlider.value = 1f;
                
                // Create background
                GameObject bg = new GameObject("Background");
                bg.transform.SetParent(obj.transform, false);
                Image bgImage = bg.AddComponent<Image>();
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                RectTransform bgRect = bg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                timeRemainingSlider.targetGraphic = bgImage;
                
                // Create fill
                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(obj.transform, false);
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = Color.green;
                RectTransform fillRect = fill.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(1f, 1f);
                fillRect.sizeDelta = Vector2.zero;
                timeRemainingSlider.fillRect = fillRect;
            }
            
            // Create budget slider
            if (budgetSlider == null)
            {
                GameObject obj = new GameObject("BudgetSlider");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 10f);
                rect.sizeDelta = new Vector2(-20f, 20f);
                
                budgetSlider = obj.AddComponent<Slider>();
                budgetSlider.minValue = 0f;
                budgetSlider.maxValue = 1f;
                budgetSlider.value = 1f;
                
                // Create background
                GameObject bg = new GameObject("Background");
                bg.transform.SetParent(obj.transform, false);
                Image bgImage = bg.AddComponent<Image>();
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                RectTransform bgRect = bg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                budgetSlider.targetGraphic = bgImage;
                
                // Create fill
                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(obj.transform, false);
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = Color.blue;
                RectTransform fillRect = fill.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(1f, 1f);
                fillRect.sizeDelta = Vector2.zero;
                budgetSlider.fillRect = fillRect;
            }
        }
        
        private void UpdateUI(EnvironmentState state)
        {
            if (state == null) return;
            
            if (timestepText != null)
            {
                timestepText.text = $"Step: {state.timestep}";
            }
            
            if (timeRemainingText != null && state.state != null)
            {
                float timeRem = state.state.time_remaining;
                timeRemainingText.text = $"Time: {timeRem:P0}";
            }
            
            if (timeRemainingSlider != null && state.state != null)
            {
                timeRemainingSlider.value = state.state.time_remaining;
            }
            
            if (budgetSlider != null && state.state != null)
            {
                // Normalize budget (assuming max 10000)
                float budgetNorm = Mathf.Clamp01(state.state.budget / 10000f);
                budgetSlider.value = budgetNorm;
            }
            
            // Current action would need to be tracked separately
            // For Phase 1, this is stubbed
            if (currentActionText != null)
            {
                currentActionText.text = "Observing...";
            }
        }
    }
}

