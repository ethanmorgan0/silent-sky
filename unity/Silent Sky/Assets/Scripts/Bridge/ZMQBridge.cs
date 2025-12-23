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
        [SerializeField] private float mockUpdateInterval = 1.0f; // Slower updates - 1 second per step
        
        // NetMQ components - will be used in Phase 2
        // private SubscriberSocket subscriber;
        // private RequestSocket requester;
        // private NetMQPoller poller;
        private bool isConnected = false;
        
        // Events - use NonSerialized field to prevent Inspector crashes
        [System.NonSerialized]
        private Action<EnvironmentState> _onStateUpdate;
        
        public event Action<EnvironmentState> OnStateUpdate
        {
            add { _onStateUpdate += value; }
            remove { _onStateUpdate -= value; }
        }
        
        private Coroutine mockDataCoroutine;
        
        private void Start()
        {
            Debug.Log("ZMQBridge: Start() called");
            try
            {
                Debug.Log("ZMQBridge: useMockData = " + useMockData);
                if (useMockData)
                {
                    Debug.Log("ZMQBridge: Starting delayed coroutine");
                    // Start on next frame to avoid first-frame issues
                    StartCoroutine(StartMockDataDelayed());
                }
                else
                {
                    Debug.Log("ZMQBridge: Calling Connect()");
                    Connect();
                }
                Debug.Log("ZMQBridge: Start() completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ZMQBridge Start error: {e.Message}\n{e.StackTrace}");
            }
        }
        
        private IEnumerator StartMockDataDelayed()
        {
            Debug.Log("ZMQBridge: StartMockDataDelayed() - waiting one frame");
            // Wait one frame before starting
            yield return null;
            Debug.Log("ZMQBridge: Starting GenerateMockData coroutine");
            try
            {
                mockDataCoroutine = StartCoroutine(GenerateMockData());
                Debug.Log("ZMQBridge: Started mock data generation successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"ZMQBridge: Failed to start coroutine: {e.Message}");
            }
        }
        
        private void OnDestroy()
        {
            if (mockDataCoroutine != null)
            {
                StopCoroutine(mockDataCoroutine);
                mockDataCoroutine = null;
            }
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
            if (directive == null)
            {
                Debug.LogWarning("SendDirective: Directive is null");
                return;
            }
            
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
            Debug.Log("ZMQBridge: GenerateMockData() started");
            int step = 0;
            // Run continuously - no step limit
            
            while (this != null && enabled)
            {
                try
                {
                    Debug.Log($"ZMQBridge: Generating mock state step {step}");
                    var mockState = MockDataGenerator.GenerateMockState(step);
                    Debug.Log($"ZMQBridge: Mock state generated, invoking event (subscribers: {(_onStateUpdate?.GetInvocationList().Length ?? 0)})");
                    
                    if (_onStateUpdate != null)
                    {
                        _onStateUpdate.Invoke(mockState);
                    }
                    step++;
                    Debug.Log($"ZMQBridge: Step {step} completed, waiting {mockUpdateInterval}s");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Error generating mock data: {e.Message}\n{e.StackTrace}");
                    break;
                }
                
                yield return new WaitForSeconds(mockUpdateInterval);
            }
            Debug.Log("ZMQBridge: GenerateMockData() ended");
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
        public bool is_observing; // True if agent is currently observing this sector
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
        // Note: Dictionary is not serializable by Unity - use for runtime only
        [System.NonSerialized]
        public Dictionary<string, float> reward_weights;
        public string upgrade;
    }
    
    // Mock data generator for testing
    public static class MockDataGenerator
    {
        // Keep track of all events across steps (accumulate)
        private static List<EventData> allEventsHistory = new List<EventData>();
        private static List<EventData> discoveredEventsHistory = new List<EventData>();
        
        // Simple agent: tracks which sector it's currently observing
        private static int currentObservingSector = 0;
        private static int stepsSinceLastSwitch = 0;
        
        public static EnvironmentState GenerateMockState(int step)
        {
            var sectors = new SectorData[8];
            for (int i = 0; i < 8; i++)
            {
                float rand = UnityEngine.Random.value;
                float sensorReading;
                float sensorConfidence;
                
                // Distribution:
                // 85% - Weak with low info (low reading, very low confidence) -> DARK
                // 12% - Strong but uncertain (high reading, medium confidence) -> YELLOW
                // 3% - Strong and high confidence (high reading, high confidence) -> GREEN
                
                if (rand < 0.85f)
                {
                    // Weak with low info - most common -> DARK
                    sensorReading = UnityEngine.Random.Range(0f, 0.5f);
                    sensorConfidence = UnityEngine.Random.Range(0.1f, 0.35f); // Keep below 0.4 to ensure dark
                }
                else if (rand < 0.97f)
                {
                    // Strong but uncertain - infrequent -> YELLOW
                    sensorReading = UnityEngine.Random.Range(0.5f, 1f);
                    sensorConfidence = UnityEngine.Random.Range(0.4f, 0.65f); // Keep in yellow range
                }
                else
                {
                    // Strong and high confidence - very rare -> GREEN
                    sensorReading = UnityEngine.Random.Range(0.7f, 1f);
                    sensorConfidence = UnityEngine.Random.Range(0.75f, 1f); // Keep above 0.7 for green
                }
                
                sectors[i] = new SectorData
                {
                    sector_id = i,
                    sensor_reading = sensorReading,
                    sensor_confidence = sensorConfidence,
                    activity_rate = UnityEngine.Random.Range(0.1f, 3f),
                    is_observing = (i == currentObservingSector)
                };
            }
            
            // Simple agent behavior: switch sectors occasionally, prefer high-likelihood sectors
            stepsSinceLastSwitch++;
            if (step == 0 || stepsSinceLastSwitch > 3 || UnityEngine.Random.value < 0.2f)
            {
                // Find sector with highest likelihood (sensor_reading * confidence)
                float maxLikelihood = 0f;
                int bestSector = 0;
                for (int i = 0; i < sectors.Length; i++)
                {
                    float likelihood = sectors[i].sensor_reading * sectors[i].sensor_confidence;
                    if (likelihood > maxLikelihood)
                    {
                        maxLikelihood = likelihood;
                        bestSector = i;
                    }
                }
                // Sometimes pick randomly for exploration
                if (UnityEngine.Random.value < 0.3f)
                {
                    currentObservingSector = UnityEngine.Random.Range(0, 8);
                }
                else
                {
                    currentObservingSector = bestSector;
                }
                stepsSinceLastSwitch = 0;
            }
            
            // Generate new events every few steps
            if (step > 0 && step % 5 == 0)
            {
                int eventSector = UnityEngine.Random.Range(0, 8);
                var newEvent = new EventData
                {
                    event_type = "nova",
                    sector = eventSector,
                    timestep = step,
                    value = UnityEngine.Random.Range(50f, 200f),
                    discovered = false
                };
                allEventsHistory.Add(newEvent);
                
                // Event is discovered ONLY if agent is observing that sector
                // Higher confidence in observed sector = higher chance of discovery
                if (eventSector == currentObservingSector)
                {
                    float discoveryChance = sectors[eventSector].sensor_confidence;
                    if (UnityEngine.Random.value < discoveryChance)
                    {
                        newEvent.discovered = true;
                        discoveredEventsHistory.Add(newEvent);
                    }
                }
                // Events in other sectors are always missed
            }
            
            // Add some missed events from history (simulate old missed events)
            if (step > 10)
            {
                int missedCount = UnityEngine.Random.Range(1, 3);
                for (int i = 0; i < missedCount && allEventsHistory.Count > discoveredEventsHistory.Count; i++)
                {
                    // Find an event that wasn't discovered
                    var missedEvent = allEventsHistory.Find(e => !e.discovered && !allEventsHistory.Exists(d => d.timestep == e.timestep && d.sector == e.sector && discoveredEventsHistory.Contains(d)));
                    if (missedEvent == null) break;
                }
            }
            
            // Return accumulated events (all events that have ever occurred)
            var allEvents = new List<EventData>(allEventsHistory);
            var discoveredEvents = new List<EventData>(discoveredEventsHistory);
            
            return new EnvironmentState
            {
                schema_version = 1,
                timestep = step,
                state = new StateData
                {
                    sectors = sectors,
                    events = allEvents.ToArray(),
                    discovered_events = discoveredEvents.ToArray(),
                    budget = 1000f - (step * 10f),
                    total_earnings = step * 50f,
                    total_costs = step * 10f,
                    upgrades = new UpgradeData(),
                    time_remaining = Mathf.Max(0f, 1f - (step / 1000f)) // Clamp to prevent negative, slower decay
                },
                observation = new ObservationData
                {
                    sensor_readings = new float[8],
                    sensor_confidence = new float[8],
                    time_remaining = new float[] { Mathf.Max(0f, 1f - (step / 1000f)) },
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

