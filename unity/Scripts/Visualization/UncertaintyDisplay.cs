using UnityEngine;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Visual representation of uncertainty per sector
    /// </summary>
    public class UncertaintyDisplay : MonoBehaviour
    {
        [Header("Visual Effects")]
        [SerializeField] private Material fogMaterial;
        [SerializeField] private float maxFogOpacity = 0.8f;
        
        private SectorMap sectorMap;
        
        private void Start()
        {
            sectorMap = FindObjectOfType<SectorMap>();
            
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateUncertainty;
            }
        }
        
        private void UpdateUncertainty(EnvironmentState state)
        {
            if (state?.state?.sectors == null) return;
            
            // Update fog/opacity effects based on uncertainty
            // This is a placeholder - actual implementation would modify
            // visual effects on sector displays
            for (int i = 0; i < state.state.sectors.Length; i++)
            {
                float uncertainty = 1f - state.state.sectors[i].sensor_confidence;
                // Apply visual effect (stubbed for Phase 1)
            }
        }
    }
}

