using UnityEngine;

namespace SilentSky.Unity.Bridge
{
    /// <summary>
    /// Minimal test version to isolate crash issues
    /// </summary>
    public class ZMQBridgeTest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("ZMQBridgeTest: Start called - no crash!");
        }
        
        private void Update()
        {
            // Just to keep it alive
        }
    }
}

