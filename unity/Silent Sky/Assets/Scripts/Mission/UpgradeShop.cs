using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Mission
{
    /// <summary>
    /// Upgrade purchase UI - simplified binary upgrades for Phase 1
    /// Creates its own UI if not provided
    /// </summary>
    public class UpgradeShop : MonoBehaviour
    {
        [Header("Upgrade Buttons (Optional - will create if not assigned)")]
        [SerializeField] private Button sensorQualityButton;
        [SerializeField] private Button fieldOfViewButton;
        [SerializeField] private Button reactionSpeedButton;
        [SerializeField] private Button predictionHintsButton;
        
        [Header("Display")]
        [SerializeField] private Text budgetText;
        [SerializeField] private Text[] upgradeStatusTexts;
        
        [Header("Auto-Create Settings")]
        [SerializeField] private bool autoCreateUI = true;
        [SerializeField] private Vector2 panelPosition = new Vector2(10f, 200f); // Bottom-left
        
        private ZMQBridge bridge;
        private float currentBudget = 1000f;
        private Canvas parentCanvas;
        
        // Upgrade costs (placeholders)
        private readonly float[] upgradeCosts = { 500f, 500f, 500f, 1000f };
        private readonly string[] upgradeNames = { "Sensor Quality", "Field of View", "Reaction Speed", "Prediction Hints" };
        private bool[] upgradesPurchased = new bool[4];
        
        private void Start()
        {
            // Find or create UI
            if (autoCreateUI)
            {
                CreateUI();
            }
            
            bridge = FindObjectOfType<ZMQBridge>();
            
            // Setup button handlers
            if (sensorQualityButton != null)
                sensorQualityButton.onClick.AddListener(() => PurchaseUpgrade(0, "sensor_quality"));
            if (fieldOfViewButton != null)
                fieldOfViewButton.onClick.AddListener(() => PurchaseUpgrade(1, "field_of_view"));
            if (reactionSpeedButton != null)
                reactionSpeedButton.onClick.AddListener(() => PurchaseUpgrade(2, "reaction_speed"));
            if (predictionHintsButton != null)
                predictionHintsButton.onClick.AddListener(() => PurchaseUpgrade(3, "prediction_hints"));
            
            // Subscribe to state updates for budget
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateBudget;
            }
            
            UpdateUI();
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
                Debug.LogError("UpgradeShop: No Canvas found. Create a Canvas in the scene.");
                return;
            }
            
            // Create panel container
            Transform panelContainer = null;
            if (sensorQualityButton == null || fieldOfViewButton == null || reactionSpeedButton == null || predictionHintsButton == null || budgetText == null)
            {
                GameObject panel = new GameObject("UpgradeShopPanel");
                panel.transform.SetParent(parentCanvas.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                // Anchor to bottom-left for easier positioning
                panelRect.anchorMin = new Vector2(0f, 0f);
                panelRect.anchorMax = new Vector2(0f, 0f);
                panelRect.pivot = new Vector2(0f, 0f);
                // Position: X from left, Y from bottom (positive = up from bottom)
                panelRect.anchoredPosition = panelPosition;
                panelRect.sizeDelta = new Vector2(250f, 250f);
                
                Debug.Log($"UpgradeShop: Panel created at anchoredPosition={panelRect.anchoredPosition}");
                
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
                rect.anchoredPosition = new Vector2(10f, -10f);
                rect.sizeDelta = new Vector2(-20f, 25f);
                
                budgetText = obj.AddComponent<Text>();
                budgetText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                budgetText.fontSize = 14;
                budgetText.color = Color.white;
                budgetText.alignment = TextAnchor.UpperLeft;
                budgetText.text = "Budget: $0.00";
            }
            
            // Create upgrade buttons
            Button[] buttons = { sensorQualityButton, fieldOfViewButton, reactionSpeedButton, predictionHintsButton };
            upgradeStatusTexts = new Text[4];
            
            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == null)
                {
                    float yPos = -50f - (i * 45f);
                    
                    // Create button
                    GameObject btnObj = new GameObject($"{upgradeNames[i]}Button");
                    btnObj.transform.SetParent(panelContainer, false);
                    
                    RectTransform btnRect = btnObj.AddComponent<RectTransform>();
                    btnRect.anchorMin = new Vector2(0f, 1f);
                    btnRect.anchorMax = new Vector2(1f, 1f);
                    btnRect.pivot = new Vector2(0f, 1f);
                    btnRect.anchoredPosition = new Vector2(10f, yPos);
                    btnRect.sizeDelta = new Vector2(-20f, 35f);
                    
                    Button btn = btnObj.AddComponent<Button>();
                    Image btnImage = btnObj.AddComponent<Image>();
                    btnImage.color = new Color(0.3f, 0.3f, 0.5f, 1f);
                    btn.targetGraphic = btnImage;
                    
                    // Create button text
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(btnObj.transform, false);
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.sizeDelta = Vector2.zero;
                    Text btnText = textObj.AddComponent<Text>();
                    btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    btnText.fontSize = 12;
                    btnText.color = Color.white;
                    btnText.alignment = TextAnchor.MiddleLeft;
                    btnText.text = upgradeNames[i];
                    
                    // Create status text (price/purchased)
                    GameObject statusObj = new GameObject("StatusText");
                    statusObj.transform.SetParent(btnObj.transform, false);
                    RectTransform statusRect = statusObj.AddComponent<RectTransform>();
                    statusRect.anchorMin = new Vector2(0.7f, 0f);
                    statusRect.anchorMax = new Vector2(1f, 1f);
                    statusRect.sizeDelta = Vector2.zero;
                    Text statusText = statusObj.AddComponent<Text>();
                    statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    statusText.fontSize = 12;
                    statusText.color = Color.yellow;
                    statusText.alignment = TextAnchor.MiddleRight;
                    statusText.text = $"${upgradeCosts[i]:F0}";
                    
                    buttons[i] = btn;
                    upgradeStatusTexts[i] = statusText;
                }
            }
            
            sensorQualityButton = buttons[0];
            fieldOfViewButton = buttons[1];
            reactionSpeedButton = buttons[2];
            predictionHintsButton = buttons[3];
        }
        
        private void PurchaseUpgrade(int index, string upgradeName)
        {
            if (upgradesPurchased[index])
            {
                Debug.Log($"Upgrade {upgradeName} already purchased");
                return;
            }
            
            if (currentBudget < upgradeCosts[index])
            {
                Debug.Log($"Insufficient budget for {upgradeName}");
                return;
            }
            
            // Send directive to Python
            if (bridge != null)
            {
                var directive = new Directive { upgrade = upgradeName };
                bridge.SendDirective(directive);
            }
            
            upgradesPurchased[index] = true;
            currentBudget -= upgradeCosts[index];
            UpdateUI();
        }
        
        private void UpdateBudget(EnvironmentState state)
        {
            if (state?.state != null)
            {
                currentBudget = state.state.budget;
                UpdateUI();
            }
        }
        
        private void UpdateUI()
        {
            if (budgetText != null)
            {
                budgetText.text = $"Budget: ${currentBudget:F2}";
            }
            
            // Update upgrade button states
            Button[] buttons = { sensorQualityButton, fieldOfViewButton, reactionSpeedButton, predictionHintsButton };
            for (int i = 0; i < buttons.Length && i < upgradeStatusTexts.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].interactable = !upgradesPurchased[i] && currentBudget >= upgradeCosts[i];
                }
                
                if (upgradeStatusTexts[i] != null)
                {
                    upgradeStatusTexts[i].text = upgradesPurchased[i] 
                        ? "Purchased" 
                        : $"${upgradeCosts[i]:F0}";
                }
            }
        }
    }
}

