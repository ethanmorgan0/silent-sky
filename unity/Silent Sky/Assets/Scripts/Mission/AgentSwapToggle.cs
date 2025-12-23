using UnityEngine;
using UnityEngine.UI;

namespace SilentSky.Unity.Mission
{
    /// <summary>
    /// Toggle between dummy and PPO agents for debugging
    /// </summary>
    public class AgentSwapToggle : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Toggle agentToggle;
        [SerializeField] private Text agentLabel;
        
        private bool useDummyAgent = true;
        
        private void Start()
        {
            if (agentToggle != null)
            {
                agentToggle.isOn = useDummyAgent;
                agentToggle.onValueChanged.AddListener(OnToggleChanged);
            }
            
            UpdateLabel();
        }
        
        private void OnToggleChanged(bool isDummy)
        {
            useDummyAgent = isDummy;
            UpdateLabel();
            
            // Send directive to Python to swap agent
            // This would need to be implemented via ZMQBridge
            Debug.Log($"Agent swapped to: {(useDummyAgent ? "Dummy" : "PPO")}");
        }
        
        private void UpdateLabel()
        {
            if (agentLabel != null)
            {
                agentLabel.text = $"Agent: {(useDummyAgent ? "Dummy" : "PPO")}";
            }
        }
    }
}

