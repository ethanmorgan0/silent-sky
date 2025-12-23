using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Mission
{
    /// <summary>
    /// UI for setting player preferences (mission directives)
    /// Creates its own UI if not provided
    /// </summary>
    public class MissionDirectives : MonoBehaviour
    {
        [Header("UI Sliders (Optional - will create if not assigned)")]
        [SerializeField] private Slider riskToleranceSlider;
        [SerializeField] private Slider explorationBiasSlider;
        [SerializeField] private Slider efficiencyFocusSlider;
        [SerializeField] private Slider budgetLimitSlider;
        
        [Header("Priority Markers")]
        [SerializeField] private Toggle[] sectorPriorityToggles;
        
        [Header("Auto-Create Settings")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Vector2 panelPosition = new Vector2(-10f, -10f); // Bottom-right
        
        private ZMQBridge bridge;
        private Canvas parentCanvas;
        
        private void Start()
        {
            // Find or create UI
            if (autoCreateUI)
            {
                CreateUI();
            }
            
            bridge = FindObjectOfType<ZMQBridge>();
            
            // Setup slider handlers
            if (riskToleranceSlider != null)
                riskToleranceSlider.onValueChanged.AddListener(_ => SendDirectives());
            if (explorationBiasSlider != null)
                explorationBiasSlider.onValueChanged.AddListener(_ => SendDirectives());
            if (efficiencyFocusSlider != null)
                efficiencyFocusSlider.onValueChanged.AddListener(_ => SendDirectives());
            if (budgetLimitSlider != null)
                budgetLimitSlider.onValueChanged.AddListener(_ => SendDirectives());
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
                Debug.LogError("MissionDirectives: No Canvas found. Create a Canvas in the scene.");
                return;
            }
            
            // Create panel container
            Transform panelContainer = null;
            if (riskToleranceSlider == null || explorationBiasSlider == null || efficiencyFocusSlider == null || budgetLimitSlider == null)
            {
                GameObject panel = new GameObject("MissionDirectivesPanel");
                panel.transform.SetParent(parentCanvas.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(1f, 0f);
                panelRect.anchorMax = new Vector2(1f, 0f);
                panelRect.pivot = new Vector2(1f, 0f);
                panelRect.anchoredPosition = panelPosition;
                panelRect.sizeDelta = new Vector2(300f, 200f);
                
                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.7f);
                
                panelContainer = panel.transform;
            }
            
            // Helper to create slider with label
            Slider CreateSlider(string name, float yPos, float defaultValue)
            {
                // Create label
                GameObject labelObj = new GameObject($"{name}Label");
                labelObj.transform.SetParent(panelContainer, false);
                RectTransform labelRect = labelObj.AddComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0f, 1f);
                labelRect.anchorMax = new Vector2(1f, 1f);
                labelRect.pivot = new Vector2(0f, 1f);
                labelRect.anchoredPosition = new Vector2(10f, yPos);
                labelRect.sizeDelta = new Vector2(-20f, 20f);
                Text labelText = labelObj.AddComponent<Text>();
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                labelText.fontSize = 12;
                labelText.color = Color.white;
                labelText.alignment = TextAnchor.UpperLeft;
                labelText.text = name.Replace("Slider", "");
                
                // Create slider
                GameObject sliderObj = new GameObject(name);
                sliderObj.transform.SetParent(panelContainer, false);
                RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
                sliderRect.anchorMin = new Vector2(0f, 1f);
                sliderRect.anchorMax = new Vector2(1f, 1f);
                sliderRect.pivot = new Vector2(0f, 1f);
                sliderRect.anchoredPosition = new Vector2(10f, yPos - 25f);
                sliderRect.sizeDelta = new Vector2(-20f, 20f);
                
                Slider slider = sliderObj.AddComponent<Slider>();
                slider.minValue = 0f;
                slider.maxValue = 1f;
                slider.value = defaultValue;
                
                // Background
                GameObject bg = new GameObject("Background");
                bg.transform.SetParent(sliderObj.transform, false);
                Image bgImage = bg.AddComponent<Image>();
                bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                RectTransform bgRect = bg.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.sizeDelta = Vector2.zero;
                slider.targetGraphic = bgImage;
                
                // Fill
                GameObject fill = new GameObject("Fill");
                fill.transform.SetParent(sliderObj.transform, false);
                Image fillImage = fill.AddComponent<Image>();
                fillImage.color = Color.cyan;
                RectTransform fillRect = fill.GetComponent<RectTransform>();
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(1f, 1f);
                fillRect.sizeDelta = Vector2.zero;
                slider.fillRect = fillRect;
                
                return slider;
            }
            
            // Create sliders
            if (riskToleranceSlider == null)
                riskToleranceSlider = CreateSlider("RiskTolerance", -5f, 0.5f);
            if (explorationBiasSlider == null)
                explorationBiasSlider = CreateSlider("ExplorationBias", -50f, 0.0f);
            if (efficiencyFocusSlider == null)
                efficiencyFocusSlider = CreateSlider("EfficiencyFocus", -95f, 0.5f);
            if (budgetLimitSlider == null)
                budgetLimitSlider = CreateSlider("BudgetLimit", -140f, 1.0f);
        }
        
        public void SendDirectives()
        {
            if (bridge == null) return;
            
            var directive = new Directive
            {
                reward_weights = new Dictionary<string, float>
                {
                    { "discovery_value", riskToleranceSlider != null ? riskToleranceSlider.value : 1.0f },
                    { "operational_cost", efficiencyFocusSlider != null ? efficiencyFocusSlider.value : 1.0f },
                    { "exploration_bias", explorationBiasSlider != null ? explorationBiasSlider.value : 0.0f }
                }
            };
            
            bridge.SendDirective(directive);
        }
        
        public void SetSectorPriority(int sector, bool priority)
        {
            if (sectorPriorityToggles != null && sector >= 0 && sector < sectorPriorityToggles.Length)
            {
                sectorPriorityToggles[sector].isOn = priority;
                SendDirectives();
            }
        }
    }
}

