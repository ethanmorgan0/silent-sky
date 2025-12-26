using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Manages viewport rotation state and input handling.
    /// Controls rotation of the viewport around the abstract sphere.
    /// </summary>
    public class ViewportRotationController : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [SerializeField] private float rotationSpeed = 0.5f; // Radians per second
        [SerializeField] private bool smoothRotation = true;
        [SerializeField] private float smoothDamping = 5f; // For smooth rotation interpolation
        
        [Header("Rotation Limits")]
        [SerializeField] private float minPhiOffset = -Mathf.PI; // Allow full range for continuous rotation
        [SerializeField] private float maxPhiOffset = Mathf.PI;  // Allow full range for continuous rotation
        
        [Header("Input Settings")]
        [SerializeField] private bool useWASD = true; // Alternative controls
        
        // Current rotation state
        private float currentThetaOffset = 0f;
        private float currentPhiOffset = 0f;
        
        // Target rotation (for smooth rotation)
        private float targetThetaOffset = 0f;
        private float targetPhiOffset = 0f;
        
        // Event for rotation changes
        public event Action<Vector2> OnRotationChanged;
        
        private void Start()
        {
            // Initialize viewport rotation to default (0, 0)
            ViewportProjection.SetViewportRotation(0f, 0f);
            targetThetaOffset = 0f;
            targetPhiOffset = 0f;
            currentThetaOffset = 0f;
            currentPhiOffset = 0f;
        }
        
        private void Update()
        {
            HandleInput();
            UpdateRotation();
        }
        
        private void HandleInput()
        {
            // Get keyboard input using new Input System
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            
            float thetaDelta = 0f;
            float phiDelta = 0f;
            
            // Check arrow keys
            if (keyboard.leftArrowKey.isPressed)
                thetaDelta -= rotationSpeed * Time.deltaTime;
            if (keyboard.rightArrowKey.isPressed)
                thetaDelta += rotationSpeed * Time.deltaTime;
            if (keyboard.upArrowKey.isPressed)
                phiDelta += rotationSpeed * Time.deltaTime;
            if (keyboard.downArrowKey.isPressed)
                phiDelta -= rotationSpeed * Time.deltaTime;
            
            // Check WASD if enabled
            if (useWASD)
            {
                if (keyboard.aKey.isPressed)
                    thetaDelta -= rotationSpeed * Time.deltaTime;
                if (keyboard.dKey.isPressed)
                    thetaDelta += rotationSpeed * Time.deltaTime;
                if (keyboard.wKey.isPressed)
                    phiDelta += rotationSpeed * Time.deltaTime;
                if (keyboard.sKey.isPressed)
                    phiDelta -= rotationSpeed * Time.deltaTime;
            }
            
            // Update target rotation
            if (thetaDelta != 0f || phiDelta != 0f)
            {
                targetThetaOffset += thetaDelta;
                targetPhiOffset += phiDelta;
                
                // Clamp phiOffset to keep centerPhiRaw within [0, π]
                // DEFAULT_CENTER_PHI = π/2, so clamp phiOffset to [-π/2, π/2]
                targetPhiOffset = Mathf.Clamp(targetPhiOffset, -Mathf.PI / 2f, Mathf.PI / 2f);
                
                // Don't normalize theta here - let it accumulate for smooth wrapping
                // LerpAngle will handle the wrapping smoothly, and we'll normalize after interpolation
            }
        }
        
        private void UpdateRotation()
        {
            // Work entirely with unbounded values - no normalization during interpolation
            // This preserves continuity across 0/2π boundary
            // Normalization only happens in ViewportProjection.GetViewportCenter() when needed
            
            // For theta: compute delta directly from unbounded values
            // Handle wrapping by taking shortest path
            float thetaDelta = targetThetaOffset - currentThetaOffset;
            
            // If delta is large, we might have wrapped - take shortest path
            if (thetaDelta > Mathf.PI)
                thetaDelta -= 2f * Mathf.PI;
            else if (thetaDelta < -Mathf.PI)
                thetaDelta += 2f * Mathf.PI;
            
            if (smoothRotation)
            {
                // Interpolate unbounded values directly
                currentThetaOffset += thetaDelta * smoothDamping * Time.deltaTime;
                
                // If very close to target, snap to prevent drift
                float remainingDelta = targetThetaOffset - currentThetaOffset;
                if (Mathf.Abs(remainingDelta) < 0.001f || 
                    (remainingDelta > 0 && remainingDelta < 0.001f) ||
                    (remainingDelta < 0 && remainingDelta > -0.001f))
                {
                    currentThetaOffset = targetThetaOffset;
                }
                
                currentPhiOffset = Mathf.Lerp(currentPhiOffset, targetPhiOffset, smoothDamping * Time.deltaTime);
            }
            else
            {
                // Instant rotation - apply delta directly
                currentThetaOffset += thetaDelta;
                currentPhiOffset = targetPhiOffset;
            }
            
            // Periodically normalize to prevent unbounded growth (but preserve relative position)
            // Only do this when values get very large to avoid precision issues
            if (Mathf.Abs(currentThetaOffset) > 20f * Mathf.PI)
            {
                float normalizeAmount = Mathf.Floor(currentThetaOffset / (2f * Mathf.PI)) * 2f * Mathf.PI;
                currentThetaOffset -= normalizeAmount;
                targetThetaOffset -= normalizeAmount;
            }
            
            // Update ViewportProjection (it will normalize thetaOffset when computing centerTheta)
            ViewportProjection.SetViewportRotation(currentThetaOffset, currentPhiOffset);
            
            // Notify listeners
            OnRotationChanged?.Invoke(new Vector2(currentThetaOffset, currentPhiOffset));
        }
        
        /// <summary>
        /// Gets the current rotation offset.
        /// </summary>
        public Vector2 GetRotation()
        {
            return new Vector2(currentThetaOffset, currentPhiOffset);
        }
        
        /// <summary>
        /// Sets the rotation offset directly (for programmatic control).
        /// </summary>
        public void SetRotation(float thetaOffset, float phiOffset)
        {
            // Keep thetaOffset unbounded to preserve continuity
            // Calculate delta from current to target to maintain unbounded state
            float thetaDelta = thetaOffset - currentThetaOffset;
            if (thetaDelta > Mathf.PI) thetaDelta -= 2f * Mathf.PI;
            else if (thetaDelta < -Mathf.PI) thetaDelta += 2f * Mathf.PI;
            targetThetaOffset = currentThetaOffset + thetaDelta;
            
            // Allow phiOffset to be unbounded - wrapping will happen in GetViewportCenter
            targetPhiOffset = phiOffset;
            
            if (!smoothRotation)
            {
                currentThetaOffset = targetThetaOffset;
                currentPhiOffset = targetPhiOffset;
                ViewportProjection.SetViewportRotation(currentThetaOffset, currentPhiOffset);
            }
        }
        
        /// <summary>
        /// Gets the current viewport center position on the sphere.
        /// </summary>
        public Vector2 GetViewportCenter()
        {
            return ViewportProjection.GetViewportCenter();
        }
    }
}

