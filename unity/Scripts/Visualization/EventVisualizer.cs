using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Displays detected events and missed opportunities
    /// </summary>
    public class EventVisualizer : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform eventListContainer;
        [SerializeField] private GameObject eventItemPrefab;
        [SerializeField] private Text discoveredCountText;
        [SerializeField] private Text missedCountText;
        
        private List<GameObject> eventItems = new List<GameObject>();
        private EnvironmentState currentState;
        
        private void Start()
        {
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateEvents;
            }
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

