using System.Collections.Generic;
using UnityEngine;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Generates fake events in spherical space for testing/visualization
    /// </summary>
    public class FakeDataGenerator : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int eventCount = 200; // Increased for easier debugging
        [SerializeField] private float minValue = 10f;
        [SerializeField] private float maxValue = 100f;
        [SerializeField] private float minDuration = 30f; // Longer duration so more events are active at once
        [SerializeField] private float maxDuration = 60f; // Longer duration so more events are active at once
        [SerializeField] private int seed = 42; // For deterministic generation
        
        private List<SpaceEvent> generatedEvents = new List<SpaceEvent>();
        private System.Random rng;
        
        public List<SpaceEvent> GeneratedEvents => generatedEvents;
        
        private void Awake()
        {
            rng = new System.Random(seed);
            GenerateEvents();
        }
        
        /// <summary>
        /// Generates fake events with random positions and values
        /// </summary>
        public void GenerateEvents()
        {
            generatedEvents.Clear();
            
            for (int i = 0; i < eventCount; i++)
            {
                // Random spherical coordinates with uniform distribution on sphere surface
                // Theta: uniform in [0, 2π] (this is correct)
                float theta = (float)(rng.NextDouble() * 2f * Mathf.PI);
                
                // Phi: must sample uniformly on sphere surface, not uniformly in phi
                // For uniform sphere distribution: sample cos(phi) uniformly, not phi
                // This accounts for the fact that rings near poles are smaller than rings near equator
                float cosPhi = 1f - 2f * (float)rng.NextDouble(); // Uniform in [-1, 1]
                float phi = Mathf.Acos(cosPhi); // This gives uniform distribution on sphere
                
                // Random value
                float value = Mathf.Lerp(minValue, maxValue, (float)rng.NextDouble());
                
                // Random timestamp (spread over time)
                float timestamp = (float)(rng.NextDouble() * 100f);
                
                // Random duration
                float duration = Mathf.Lerp(minDuration, maxDuration, (float)rng.NextDouble());
                
                SpaceEvent evt = new SpaceEvent("Event", value, theta, phi, timestamp, duration);
                generatedEvents.Add(evt);
            }
            
            Debug.Log($"Generated {generatedEvents.Count} fake events");
        }
        
        /// <summary>
        /// Regenerates events with a new seed
        /// </summary>
        public void RegenerateWithSeed(int newSeed)
        {
            seed = newSeed;
            rng = new System.Random(seed);
            GenerateEvents();
        }
        
        /// <summary>
        /// Gets all events active at a given time
        /// </summary>
        public List<SpaceEvent> GetActiveEvents(float currentTime)
        {
            List<SpaceEvent> active = new List<SpaceEvent>();
            foreach (var evt in generatedEvents)
            {
                if (evt.IsActive(currentTime))
                {
                    active.Add(evt);
                }
            }
            return active;
        }
    }
}

