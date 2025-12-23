using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Mission
{
    /// <summary>
    /// Upgrade purchase UI - simplified binary upgrades for Phase 1
    /// </summary>
    public class UpgradeShop : MonoBehaviour
    {
        [Header("Upgrade Buttons")]
        [SerializeField] private Button sensorQualityButton;
        [SerializeField] private Button fieldOfViewButton;
        [SerializeField] private Button reactionSpeedButton;
        [SerializeField] private Button predictionHintsButton;
        
        [Header("Display")]
        [SerializeField] private Text budgetText;
        [SerializeField] private Text[] upgradeStatusTexts;
        
        private ZMQBridge bridge;
        private float currentBudget = 1000f;
        
        // Upgrade costs (placeholders)
        private readonly float[] upgradeCosts = { 500f, 500f, 500f, 1000f };
        private bool[] upgradesPurchased = new bool[4];
        
        private void Start()
        {
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

