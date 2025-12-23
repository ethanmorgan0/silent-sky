using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Visualization
{
    /// <summary>
    /// Visualizes 8 sky sectors in a circular or grid layout
    /// </summary>
    public class SectorMap : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField] private bool useCircularLayout = true;
        [SerializeField] private float radius = 200f;
        [SerializeField] private Transform sectorContainer;
        [SerializeField] private GameObject sectorPrefab;
        
        [Header("Colors")]
        [SerializeField] private Color lowUncertaintyColor = Color.green;
        [SerializeField] private Color highUncertaintyColor = Color.red;
        [SerializeField] private Color defaultColor = Color.white;
        
        private List<SectorDisplay> sectors = new List<SectorDisplay>();
        private EnvironmentState currentState;
        
        private void Start()
        {
            InitializeSectors();
            
            // Subscribe to state updates
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                bridge.OnStateUpdate += UpdateSectors;
            }
        }
        
        private void InitializeSectors()
        {
            if (sectorPrefab == null || sectorContainer == null)
            {
                Debug.LogWarning("SectorMap: Missing prefab or container");
                return;
            }
            
            for (int i = 0; i < 8; i++)
            {
                GameObject sectorObj = Instantiate(sectorPrefab, sectorContainer);
                SectorDisplay display = sectorObj.GetComponent<SectorDisplay>();
                if (display == null)
                {
                    display = sectorObj.AddComponent<SectorDisplay>();
                }
                
                display.Initialize(i);
                sectors.Add(display);
                
                // Position sectors
                if (useCircularLayout)
                {
                    float angle = (i / 8f) * 2f * Mathf.PI;
                    sectorObj.transform.localPosition = new Vector3(
                        Mathf.Cos(angle) * radius,
                        Mathf.Sin(angle) * radius,
                        0f
                    );
                }
                else
                {
                    // Grid layout
                    int row = i / 4;
                    int col = i % 4;
                    sectorObj.transform.localPosition = new Vector3(
                        (col - 1.5f) * 100f,
                        (row - 0.5f) * 100f,
                        0f
                    );
                }
            }
        }
        
        private void UpdateSectors(EnvironmentState state)
        {
            currentState = state;
            
            if (state?.state?.sectors == null) return;
            
            for (int i = 0; i < sectors.Count && i < state.state.sectors.Length; i++)
            {
                var sectorData = state.state.sectors[i];
                sectors[i].UpdateDisplay(sectorData);
            }
        }
        
        private Color GetSectorColor(float confidence)
        {
            // Interpolate between colors based on confidence
            float uncertainty = 1f - confidence;
            return Color.Lerp(lowUncertaintyColor, highUncertaintyColor, uncertainty);
        }
    }
    
    /// <summary>
    /// Individual sector display component
    /// </summary>
    public class SectorDisplay : MonoBehaviour
    {
        [SerializeField] private Text sectorLabel;
        [SerializeField] private Image sectorImage;
        [SerializeField] private Text readingText;
        
        private int sectorId;
        
        public void Initialize(int id)
        {
            sectorId = id;
            if (sectorLabel != null)
            {
                sectorLabel.text = $"Sector {id}";
            }
        }
        
        public void UpdateDisplay(SectorData data)
        {
            if (sectorImage != null)
            {
                // Color based on confidence
                float uncertainty = 1f - data.sensor_confidence;
                sectorImage.color = Color.Lerp(Color.green, Color.red, uncertainty);
            }
            
            if (readingText != null)
            {
                readingText.text = $"{data.sensor_reading:F2}";
            }
        }
    }
}

