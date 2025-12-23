using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Finance
{
    /// <summary>
    /// Budget and financial tracking UI
    /// </summary>
    public class BudgetUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text budgetText;
        [SerializeField] private Text earningsText;
        [SerializeField] private Text costsText;
        [SerializeField] private Text profitText;
        [SerializeField] private Slider budgetSlider;
        
        private void Start()
        {
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateUI;
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

