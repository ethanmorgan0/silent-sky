using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
// NetMQ and Newtonsoft.Json removed - using mock data mode for Phase 1
// Phase 2: Add back when packages can be installed safely

namespace SilentSky.Unity.Bridge
{
    /// <summary>
    /// ZeroMQ bridge for Unity-Python communication
    /// Subscribes to state updates and sends directives
    /// </summary>
    public class ZMQBridge : MonoBehaviour
    {
        [Header("Connection Settings")]
        [SerializeField] private bool useMockData = true; // Default to true - no packages needed
        [SerializeField] private string pubAddress = "tcp://localhost:5555"; // For Phase 2
        [SerializeField] private string repAddress = "tcp://localhost:5556"; // For Phase 2
        
        [Header("Mock Data Settings")]
        [SerializeField] private float mockUpdateInterval = 0.1f;
        
        // NetMQ components - will be used in Phase 2
        // private SubscriberSocket subscriber;
        // private RequestSocket requester;
        // private NetMQPoller poller;
        private bool isConnected = false;
        
        // Events
        public event Action<EnvironmentState> OnStateUpdate;
        
        private void Start()
        {
            if (useMockData)
            {
                StartCoroutine(GenerateMockData());
            }
            else
            {
                Connect();
            }
        }
        
        private void OnDestroy()
        {
            Disconnect();
        }
        
        public void Connect()
        {
            // Phase 2: Implement NetMQ connection here
            if (!useMockData)
            {
                Debug.LogWarning("Real ZMQ connection not implemented yet. Using mock data.");
                useMockData = true;
            }
            isConnected = true;
        }
        
        public void Disconnect()
        {
            isConnected = false;
            // Phase 2: Clean up NetMQ resources here
        }
        
        public void SendDirective(Directive directive)
        {
            if (useMockData)
            {
                Debug.Log($"Mock: Directive sent - {directive.upgrade ?? "reward_weights"}");
                return;
            }
            
            // Phase 2: Send via NetMQ here
            Debug.LogWarning("SendDirective: Real connection not implemented yet.");
        }
        
        private IEnumerator GenerateMockData()
        {
            int step = 0;
            while (true)
            {
                var mockState = MockDataGenerator.GenerateMockState(step);
                OnStateUpdate?.Invoke(mockState);
                step++;
                yield return new WaitForSeconds(mockUpdateInterval);
            }
        }
    }
    
    // Data structures matching Python schema
    [Serializable]
    public class EnvironmentState
    {
        public int schema_version;
        public int timestep;
        public StateData state;
        public ObservationData observation;
        public InfoData info;
    }
    
    [Serializable]
    public class StateData
    {
        public SectorData[] sectors;
        public EventData[] events;
        public EventData[] discovered_events;
        public float budget;
        public float total_earnings;
        public float total_costs;
        public UpgradeData upgrades;
        public float time_remaining;
    }
    
    [Serializable]
    public class SectorData
    {
        public int sector_id;
        public float sensor_reading;
        public float sensor_confidence;
        public float activity_rate;
    }
    
    [Serializable]
    public class EventData
    {
        public string event_type;
        public int sector;
        public int timestep;
        public float value;
        public bool discovered;
    }
    
    [Serializable]
    public class UpgradeData
    {
        public bool sensor_quality;
        public bool field_of_view;
        public bool reaction_speed;
        public bool prediction_hints;
    }
    
    [Serializable]
    public class ObservationData
    {
        public float[] sensor_readings;
        public float[] sensor_confidence;
        public float[] time_remaining;
        public float[] budget_remaining;
    }
    
    [Serializable]
    public class InfoData
    {
        public int timestep;
        public float budget;
        public float total_earnings;
        public float total_costs;
        public float profit;
        public int events_discovered;
        public int events_total;
    }
    
    [Serializable]
    public class Directive
    {
        public Dictionary<string, float> reward_weights;
        public string upgrade;
    }
    
    // Mock data generator for testing
    public static class MockDataGenerator
    {
        public static EnvironmentState GenerateMockState(int step)
        {
            var sectors = new SectorData[8];
            for (int i = 0; i < 8; i++)
            {
                sectors[i] = new SectorData
                {
                    sector_id = i,
                    sensor_reading = UnityEngine.Random.Range(0f, 1f),
                    sensor_confidence = UnityEngine.Random.Range(0.5f, 1f),
                    activity_rate = UnityEngine.Random.Range(0.1f, 3f)
                };
            }
            
            return new EnvironmentState
            {
                schema_version = 1,
                timestep = step,
                state = new StateData
                {
                    sectors = sectors,
                    events = new EventData[0],
                    discovered_events = new EventData[0],
                    budget = 1000f - (step * 10f),
                    total_earnings = step * 50f,
                    total_costs = step * 10f,
                    upgrades = new UpgradeData(),
                    time_remaining = 1f - (step / 100f)
                },
                observation = new ObservationData
                {
                    sensor_readings = new float[8],
                    sensor_confidence = new float[8],
                    time_remaining = new float[] { 1f - (step / 100f) },
                    budget_remaining = new float[] { (1000f - step * 10f) / 10000f }
                },
                info = new InfoData
                {
                    timestep = step,
                    budget = 1000f - (step * 10f),
                    total_earnings = step * 50f,
                    total_costs = step * 10f,
                    profit = step * 40f,
                    events_discovered = step / 10,
                    events_total = step / 5
                }
            };
        }
    }
}

