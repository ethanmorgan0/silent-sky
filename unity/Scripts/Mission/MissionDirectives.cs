using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Mission
{
    /// <summary>
    /// UI for setting player preferences (mission directives)
    /// </summary>
    public class MissionDirectives : MonoBehaviour
    {
        [Header("UI Sliders")]
        [SerializeField] private Slider riskToleranceSlider;
        [SerializeField] private Slider explorationBiasSlider;
        [SerializeField] private Slider efficiencyFocusSlider;
        [SerializeField] private Slider budgetLimitSlider;
        
        [Header("Priority Markers")]
        [SerializeField] private Toggle[] sectorPriorityToggles;
        
        private ZMQBridge bridge;
        
        private void Start()
        {
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

