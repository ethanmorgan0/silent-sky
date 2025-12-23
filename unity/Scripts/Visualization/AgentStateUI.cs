using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Displays current agent state and action
    /// </summary>
    public class AgentStateUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Text timestepText;
        [SerializeField] private Text timeRemainingText;
        [SerializeField] private Text currentActionText;
        [SerializeField] private Slider timeRemainingSlider;
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

