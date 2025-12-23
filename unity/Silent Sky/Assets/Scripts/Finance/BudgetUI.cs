using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Finance
{
    /// <summary>
    /// Budget and financial tracking UI
    /// Creates its own UI if not provided
    /// </summary>
    public class BudgetUI : MonoBehaviour
    {
        [Header("UI References (Optional - will create if not assigned)")]
        [SerializeField] private Text budgetText;
        [SerializeField] private Text earningsText;
        [SerializeField] private Text costsText;
        [SerializeField] private Text profitText;
        [SerializeField] private Slider budgetSlider;
        
        [Header("Auto-Create Settings")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Vector2 panelPosition = new Vector2(10f, -10f); // Bottom-left
        
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
                Debug.LogError("BudgetUI: No Canvas found. Create a Canvas in the scene.");
                return;
            }
            
            // Create panel container
            Transform panelContainer = null;
            if (budgetText == null || earningsText == null || costsText == null || profitText == null)
            {
                GameObject panel = new GameObject("BudgetUIPanel");
                panel.transform.SetParent(parentCanvas.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0f, 0f);
                panelRect.anchorMax = new Vector2(0f, 0f);
                panelRect.pivot = new Vector2(0f, 0f);
                panelRect.anchoredPosition = panelPosition;
                panelRect.sizeDelta = new Vector2(250f, 120f);
                
                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0f, 0f, 0f, 0.7f);
                
                panelContainer = panel.transform;
            }
            
            // Create budget text
            if (budgetText == null)
            {
                GameObject obj = new GameObject("BudgetText");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f, -5f);
                rect.sizeDelta = new Vector2(-20f, 20f);
                
                budgetText = obj.AddComponent<Text>();
                budgetText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                budgetText.fontSize = 14;
                budgetText.color = Color.white;
                budgetText.alignment = TextAnchor.UpperLeft;
                budgetText.text = "Budget: $0.00";
            }
            
            // Create earnings text
            if (earningsText == null)
            {
                GameObject obj = new GameObject("EarningsText");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f, -30f);
                rect.sizeDelta = new Vector2(-20f, 20f);
                
                earningsText = obj.AddComponent<Text>();
                earningsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                earningsText.fontSize = 14;
                earningsText.color = Color.green;
                earningsText.alignment = TextAnchor.UpperLeft;
                earningsText.text = "Earnings: $0.00";
            }
            
            // Create costs text
            if (costsText == null)
            {
                GameObject obj = new GameObject("CostsText");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f, -55f);
                rect.sizeDelta = new Vector2(-20f, 20f);
                
                costsText = obj.AddComponent<Text>();
                costsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                costsText.fontSize = 14;
                costsText.color = Color.red;
                costsText.alignment = TextAnchor.UpperLeft;
                costsText.text = "Costs: $0.00";
            }
            
            // Create profit text
            if (profitText == null)
            {
                GameObject obj = new GameObject("ProfitText");
                obj.transform.SetParent(panelContainer, false);
                
                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(10f, -80f);
                rect.sizeDelta = new Vector2(-20f, 20f);
                
                profitText = obj.AddComponent<Text>();
                profitText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                profitText.fontSize = 14;
                profitText.color = Color.yellow;
                profitText.alignment = TextAnchor.UpperLeft;
                profitText.text = "Profit: $0.00";
            }
            
            // Create budget slider (optional, since AgentStateUI already has one)
            if (budgetSlider == null)
            {
                // Skip slider - AgentStateUI already shows budget
                // Can be added later if needed
            }
        }
        
        private void UpdateUI(EnvironmentState state)
        {
            if (state?.state == null) return;
            
            if (budgetText != null)
            {
                budgetText.text = $"Budget: ${state.state.budget:F2}";
            }
            
            if (earningsText != null)
            {
                earningsText.text = $"Earnings: ${state.state.total_earnings:F2}";
            }
            
            if (costsText != null)
            {
                costsText.text = $"Costs: ${state.state.total_costs:F2}";
            }
            
            if (profitText != null)
            {
                float profit = state.state.total_earnings - state.state.total_costs;
                profitText.text = $"Profit: ${profit:F2}";
                profitText.color = profit >= 0 ? Color.green : Color.red;
            }
            
            if (budgetSlider != null)
            {
                // Normalize budget (assuming max 10000)
                float budgetNorm = Mathf.Clamp01(state.state.budget / 10000f);
                budgetSlider.value = budgetNorm;
            }
        }
    }
}

