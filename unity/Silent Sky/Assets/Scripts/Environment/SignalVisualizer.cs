using UnityEngine;
using SilentSky.Unity.Visualization;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Visualizes signals by mapping them to hexagon colors/intensity
    /// </summary>
    public class SignalVisualizer : MonoBehaviour
    {
        [SerializeField] private SignalCalculator signalCalculator;
        [SerializeField] private SectorMap sectorMap;
        
        [Header("Visualization Settings")]
        [SerializeField] private float minSignal = 0f;
        [SerializeField] private float maxSignal = 500f;
        [SerializeField] private Color minSignalColor = new Color(0.1f, 0.1f, 0.1f); // Dark
        [SerializeField] private Color maxSignalColor = new Color(1f, 1f, 0f); // Bright yellow
        [SerializeField] [Range(0f, 1f)] private float hexagonOpacity = 0.7f; // Transparency (0 = fully transparent, 1 = fully opaque)
        
        private void Start()
        {
            if (signalCalculator == null)
            {
                signalCalculator = FindObjectOfType<SignalCalculator>();
            }
            
            if (sectorMap == null)
            {
                sectorMap = FindObjectOfType<SectorMap>();
            }
        }
        
        private void Update()
        {
            UpdateVisualization();
        }
        
        /// <summary>
        /// Updates hexagon colors based on signal values
        /// </summary>
        private void UpdateVisualization()
        {
            if (signalCalculator == null || sectorMap == null)
            {
                return;
            }
            
            float[] signals = signalCalculator.SegmentSignals;
            if (signals == null || signals.Length != 19)
            {
                return;
            }
            
            // Update each hexagon based on its signal
            // Apply opacity to colors so stars show through
            Color minColorWithOpacity = minSignalColor;
            minColorWithOpacity.a = hexagonOpacity;
            Color maxColorWithOpacity = maxSignalColor;
            maxColorWithOpacity.a = hexagonOpacity;
            
            sectorMap.UpdateSignals(signals, minSignal, maxSignal, minColorWithOpacity, maxColorWithOpacity);
        }
    }
}

