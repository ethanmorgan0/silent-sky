using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SilentSky.Unity.Utils;

namespace SilentSky.Unity.Environment
{
    /// <summary>
    /// Generates a procedural starfield background aligned with hexagon sectors
    /// Stars are positioned based on spherical coordinates to match sphere segments
    /// </summary>
    public class StarfieldBackground : MonoBehaviour
    {
        [Header("Starfield Settings")]
        [SerializeField] private int starCount = 1000;
        [SerializeField] private float starSize = 0.1f;
        [SerializeField] private float starBrightness = 1f;
        [SerializeField] private bool useUI = true; // Use UI Image instead of particle system for alignment
        
        [Header("Alignment")]
        [SerializeField] private RectTransform targetContainer; // Hexagon container to align with
        [SerializeField] private bool autoFindContainer = true;
        [SerializeField] private ViewportRotationController rotationController; // Viewport rotation (auto-finds if not set)
        
        [Header("Legacy Settings (for particle/sprite modes)")]
        [SerializeField] private bool randomDistribution = true;
        [SerializeField] private float distributionRadius = 50f;
        
        private ParticleSystem particleSystem;
        private ParticleSystem.Particle[] particles;
        private RectTransform rectTransform;
        
        private void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
            }
            
            // Auto-find hexagon container if not assigned
            if (autoFindContainer && targetContainer == null)
            {
                var sectorMap = FindObjectOfType<SilentSky.Unity.Visualization.SectorMap>();
                if (sectorMap != null)
                {
                    // Try to find the sector container via reflection or public method
                    // For now, search for common names
                    GameObject container = GameObject.Find("SectorContainer");
                    if (container != null)
                    {
                        targetContainer = container.GetComponent<RectTransform>();
                    }
                }
            }
            
            // Find rotation controller if not assigned
            if (rotationController == null)
            {
                rotationController = FindObjectOfType<ViewportRotationController>();
            }
            
            // Subscribe to rotation changes to update star positions
            if (rotationController != null)
            {
                rotationController.OnRotationChanged += OnViewportRotationChanged;
            }
            
            if (useUI)
            {
                CreateUIStarfield();
            }
            else
            {
                // Fallback to particle system if UI mode is disabled
                CreateParticleStarfield();
            }
        }
        
        private GameObject starContainerObj; // Store reference to star container
        private Vector2 lastContainerSize = Vector2.zero; // Track container size to only update when it changes
        
        private float enforcedMinSize = 0f; // Remember the minimum size we enforced
        
        private void LateUpdate()
        {
            // Keep starfield aligned with hexagon container when canvas resizes
            if (targetContainer != null && rectTransform != null)
            {
                // Match position and anchors of target container
                rectTransform.anchorMin = targetContainer.anchorMin;
                rectTransform.anchorMax = targetContainer.anchorMax;
                rectTransform.anchoredPosition = targetContainer.anchoredPosition;
                rectTransform.pivot = targetContainer.pivot;
                
                // Only update sizeDelta if we haven't enforced a minimum, or if target is larger
                float minSize = 800f; // Minimum size for hexagon layout
                Vector2 newSizeDelta;
                if (targetContainer.sizeDelta.x < minSize || targetContainer.sizeDelta.y < minSize)
                {
                    // Enforce minimum size
                    float newSize = Mathf.Max(minSize, targetContainer.sizeDelta.x, targetContainer.sizeDelta.y);
                    newSizeDelta = new Vector2(newSize, newSize);
                    enforcedMinSize = newSize;
                }
                else
                {
                    // Target is large enough, use it
                    newSizeDelta = targetContainer.sizeDelta;
                    enforcedMinSize = 0f; // Reset enforcement
                }
                rectTransform.sizeDelta = newSizeDelta;
                
                // Only update star positions if container size actually changed
                Vector2 currentSize = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
                if (Vector2.Distance(currentSize, lastContainerSize) > 1f) // Only if size changed by more than 1 pixel
                {
                    UpdateStarPositions();
                    lastContainerSize = currentSize;
                }
            }
        }
        
        /// <summary>
        /// Updates star anchor positions when container resizes (for anchor-based positioning)
        /// </summary>
        private void UpdateStarAnchors()
        {
            if (starContainerObj == null)
            {
                starContainerObj = transform.Find("StarContainer")?.gameObject;
            }
            if (starContainerObj == null || rectTransform == null) return;
            
            // Get effective container size (for center-anchored, use sizeDelta)
            float containerSize = Mathf.Max(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            if (containerSize < 100f)
            {
                containerSize = Mathf.Max(rectTransform.rect.width, rectTransform.rect.height);
            }
            
            RectTransform starContainerRect = starContainerObj.GetComponent<RectTransform>();
            if (starContainerRect != null)
            {
                // Ensure StarContainer fills the parent
                starContainerRect.anchorMin = Vector2.zero;
                starContainerRect.anchorMax = Vector2.one;
                starContainerRect.sizeDelta = Vector2.zero;
                starContainerRect.anchoredPosition = Vector2.zero;
            }
            
            // Update each star's anchor position
            int updatedCount = 0;
            foreach (Transform starTransform in starContainerObj.transform)
            {
                RectTransform starRect = starTransform.GetComponent<RectTransform>();
                if (starRect == null) continue;
                
                // Get stored position data
                StarPositionData positionData = starTransform.GetComponent<StarPositionData>();
                if (positionData != null)
                {
                    // Recalculate viewport position from spherical coordinates (accounts for rotation)
                    UpdateStarViewportPosition(starTransform.gameObject, positionData);
                    updatedCount++;
                }
            }
            
            // Only log occasionally to avoid spam
            if (Time.frameCount % 300 == 0) // Every ~5 seconds at 60fps
            {
                Debug.Log($"StarfieldBackground: Updated {updatedCount} star anchors. " +
                    $"Container sizeDelta: {rectTransform.sizeDelta}, effective size: {containerSize}, " +
                    $"StarContainer rect: {(starContainerRect != null ? $"{starContainerRect.rect.width}x{starContainerRect.rect.height}" : "null")}");
            }
        }
        
        /// <summary>
        /// Updates all star positions when container resizes
        /// </summary>
        private void UpdateStarPositions()
        {
            // With anchor-based positioning, we just need to update anchors
            UpdateStarAnchors();
        }
        
        /// <summary>
        /// Creates starfield using UI Images - aligns perfectly with hexagons
        /// </summary>
        private void CreateUIStarfield()
        {
            // Ensure we're on a Canvas
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("StarfieldBackground: Not on a Canvas. UI starfield requires Canvas parent.");
                CreateParticleStarfield(); // Fallback
                return;
            }
            
            // Set up RectTransform to match hexagon container (so stars align with hexagons)
            if (targetContainer != null)
            {
                rectTransform.anchorMin = targetContainer.anchorMin;
                rectTransform.anchorMax = targetContainer.anchorMax;
                rectTransform.anchoredPosition = targetContainer.anchoredPosition;
                rectTransform.sizeDelta = targetContainer.sizeDelta;
                rectTransform.pivot = targetContainer.pivot; // Match pivot too
                
                // If container is at bottom-left (anchors 0,0), center it instead
                if (targetContainer.anchorMin == Vector2.zero && targetContainer.anchorMax == Vector2.zero)
                {
                    Debug.LogWarning("StarfieldBackground: TargetContainer is at bottom-left. Centering it for better display.");
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = Vector2.zero;
                }
                
                // Always ensure container has a reasonable size for hexagon layout
                // JWST layout with hexSize ~80 needs about 600-800 pixels to fit all hexagons
                // Calculate based on hexSize if available from SectorMap
                float minSize = 800f;
                var sectorMap = FindObjectOfType<SilentSky.Unity.Visualization.SectorMap>();
                if (sectorMap != null)
                {
                    // Try to get hexSize via reflection or estimate
                    // For hexSize=80, outer ring radius is about 2*hexSize*2 = 320 pixels
                    // Add padding: 320*2 + 100 = 740 minimum
                    minSize = 800f; // Safe default
                }
                
                // Force a minimum size regardless of targetContainer size
                // This ensures stars distribute across the full hexagon area
                float newSize = Mathf.Max(minSize, targetContainer.sizeDelta.x, targetContainer.sizeDelta.y);
                rectTransform.sizeDelta = new Vector2(newSize, newSize);
                
                if (targetContainer.sizeDelta.x < minSize || targetContainer.sizeDelta.y < minSize)
                {
                    Debug.Log($"StarfieldBackground: Container size was too small ({targetContainer.sizeDelta}). Using {rectTransform.sizeDelta} to fit hexagon layout.");
                }
                else
                {
                    Debug.Log($"StarfieldBackground: Using container size {rectTransform.sizeDelta} (target was {targetContainer.sizeDelta})");
                }
            }
            else
            {
                // Fallback: center on canvas if no container specified
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.sizeDelta = new Vector2(1000f, 1000f); // Default size
                rectTransform.anchoredPosition = Vector2.zero; // Centered
            }
            
            // Create star container that fills this RectTransform
            starContainerObj = new GameObject("StarContainer");
            starContainerObj.transform.SetParent(transform, false);
            RectTransform starContainerRect = starContainerObj.AddComponent<RectTransform>();
            
            // StarContainer should fill the StarfieldBackground
            // Use stretch anchors to fill parent
            starContainerRect.anchorMin = Vector2.zero;
            starContainerRect.anchorMax = Vector2.one;
            starContainerRect.sizeDelta = Vector2.zero;
            starContainerRect.anchoredPosition = Vector2.zero;
            
            // Force immediate layout update
            Canvas.ForceUpdateCanvases();
            
            // Generate stars based on spherical coordinates projected to viewport
            // Each star represents a point on the 360-degree sphere, projected to viewport
            // This ensures stars fill the entire viewport uniformly
            Sprite starSprite = CreateStarSprite();
            
            for (int i = 0; i < starCount; i++)
            {
                GameObject star = new GameObject($"Star_{i}");
                star.transform.SetParent(starContainerObj.transform, false);
                
                Image starImage = star.AddComponent<Image>();
                starImage.sprite = starSprite;
                starImage.color = new Color(1f, 1f, 1f, starBrightness);
                
                RectTransform starRect = star.GetComponent<RectTransform>();
                
                // Generate random spherical coordinates uniformly on the sphere
                // Theta: uniform in [0, 2π]
                // Phi: sample cos(phi) uniformly to get uniform distribution on sphere surface
                float theta = Random.Range(0f, 2f * Mathf.PI);
                float cosPhi = Random.Range(-1f, 1f); // Uniform in cos(phi)
                float phi = Mathf.Acos(cosPhi); // Convert to phi [0, π]
                
                // Store spherical coordinates (source of truth) so stars can update with rotation
                StarPositionData positionData = star.AddComponent<StarPositionData>();
                positionData.theta = theta;
                positionData.phi = phi;
                
                // Size stars first - use a fixed pixel size
                float starPixelSize = starSize * 20f; // Scale factor for visibility
                starRect.sizeDelta = Vector2.one * starPixelSize;
                
                // Calculate initial viewport position (this sets the anchors)
                UpdateStarViewportPosition(star, positionData);
                
                // Debug first few stars to verify positions are different
                if (i < 5)
                {
                    Debug.Log($"Star {i}: theta={positionData.theta:F3}, phi={positionData.phi:F3}, viewport=({positionData.normalizedX:F3}, {positionData.normalizedY:F3})");
                }
                
                // Anchor-based positioning is handled by UpdateStarViewportPosition
                starRect.pivot = new Vector2(0.5f, 0.5f);
                starRect.anchoredPosition = Vector2.zero; // No offset, position is determined by anchor
                
                starImage.raycastTarget = false; // Don't block UI interactions
                
                // Ensure stars are visible
                if (starImage.color.a < 0.1f)
                {
                    starImage.color = new Color(1f, 1f, 1f, starBrightness);
                }
            }
            
            float containerWidth = rectTransform.rect.width;
            float containerHeight = rectTransform.rect.height;
            Debug.Log($"StarfieldBackground: Created {starCount} stars in UI mode. Container: {(targetContainer != null ? targetContainer.name : "null")}, " +
                $"Container rect size: {containerWidth}x{containerHeight}, " +
                $"SizeDelta: {rectTransform.sizeDelta}, Anchors: {rectTransform.anchorMin} to {rectTransform.anchorMax}, " +
                $"AnchoredPos: {rectTransform.anchoredPosition}");
            
            // If container size is too small, warn and suggest fix
            if (containerWidth < 100f || containerHeight < 100f)
            {
                Debug.LogWarning($"StarfieldBackground: Container size is very small ({containerWidth}x{containerHeight}). " +
                    $"Stars may cluster. Ensure SectorContainer has a proper sizeDelta (e.g., 1500x1500).");
            }
            
            // Force an update of star positions after a frame (when container is likely sized)
            StartCoroutine(UpdateStarsAfterFrame());
        }
        
        private IEnumerator UpdateStarsAfterFrame()
        {
            yield return null; // Wait one frame
            yield return null; // Wait another frame to ensure layout is complete
            yield return null; // Wait one more frame for layout to fully update
            
            // Force layout update
            Canvas.ForceUpdateCanvases();
            
            UpdateStarAnchors(); // Update anchors now that container should be sized
            
            // Use sizeDelta instead of rect.width for center-anchored elements
            float containerSize = Mathf.Max(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y);
            if (containerSize == 0f)
            {
                containerSize = Mathf.Max(rectTransform.rect.width, rectTransform.rect.height);
            }
            Debug.Log($"StarfieldBackground: Updated star anchors. Container sizeDelta: {rectTransform.sizeDelta}, " +
                $"rect size: {rectTransform.rect.width}x{rectTransform.rect.height}, " +
                $"effective size: {containerSize}");
        }
        
        /// <summary>
        /// Updates a star's viewport position from its spherical coordinates
        /// </summary>
        private void UpdateStarViewportPosition(GameObject star, StarPositionData positionData)
        {
            if (star == null || positionData == null) return;
            
            RectTransform starRect = star.GetComponent<RectTransform>();
            if (starRect == null) return;
            
            // Recalculate viewport position from spherical coordinates (accounts for current rotation)
            Vector2 viewportPos = ViewportProjection.ProjectToViewport(positionData.theta, positionData.phi);
            
            // Update stored viewport position
            positionData.normalizedX = viewportPos.x;
            positionData.normalizedY = viewportPos.y;
            
            // Update anchor to new position
            starRect.anchorMin = new Vector2(positionData.normalizedX, positionData.normalizedY);
            starRect.anchorMax = new Vector2(positionData.normalizedX, positionData.normalizedY);
            starRect.anchoredPosition = Vector2.zero;
        }
        
        /// <summary>
        /// Called when viewport rotation changes - updates all star positions
        /// </summary>
        private void OnViewportRotationChanged(Vector2 rotation)
        {
            if (starContainerObj == null) return;
            
            // Update positions of all stars when rotation changes
            foreach (Transform starTransform in starContainerObj.transform)
            {
                StarPositionData positionData = starTransform.GetComponent<StarPositionData>();
                if (positionData != null)
                {
                    UpdateStarViewportPosition(starTransform.gameObject, positionData);
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from rotation changes
            if (rotationController != null)
            {
                rotationController.OnRotationChanged -= OnViewportRotationChanged;
            }
        }
        
        private void CreateParticleStarfield()
        {
            // Create particle system for stars
            GameObject particleObj = new GameObject("StarfieldParticles");
            particleObj.transform.SetParent(transform);
            particleObj.transform.localPosition = Vector3.zero;
            
            particleSystem = particleObj.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startLifetime = Mathf.Infinity;
            main.startSpeed = 0f;
            main.startSize = starSize;
            main.startColor = new Color(1f, 1f, 1f, starBrightness);
            main.maxParticles = starCount;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            
            var emission = particleSystem.emission;
            emission.enabled = false; // We'll set particles manually
            
            var shape = particleSystem.shape;
            shape.enabled = false;
            
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            
            // Generate star positions
            particles = new ParticleSystem.Particle[starCount];
            for (int i = 0; i < starCount; i++)
            {
                Vector3 position;
                if (randomDistribution)
                {
                    // Random distribution in a sphere
                    position = Random.insideUnitSphere * distributionRadius;
                }
                else
                {
                    // Grid distribution
                    int gridSize = Mathf.CeilToInt(Mathf.Sqrt(starCount));
                    int x = i % gridSize;
                    int y = i / gridSize;
                    position = new Vector3(
                        (x / (float)gridSize - 0.5f) * distributionRadius * 2f,
                        (y / (float)gridSize - 0.5f) * distributionRadius * 2f,
                        Random.Range(-distributionRadius, distributionRadius)
                    );
                }
                
                particles[i].position = position;
                particles[i].startLifetime = Mathf.Infinity;
                particles[i].remainingLifetime = Mathf.Infinity;
                particles[i].startSize = starSize;
                particles[i].startColor = new Color(1f, 1f, 1f, starBrightness);
            }
            
            particleSystem.SetParticles(particles, starCount);
        }
        
        private void CreateSpriteStarfield()
        {
            // Alternative: Create individual sprites for stars
            // This is less performant but gives more control
            GameObject starContainer = new GameObject("StarContainer");
            starContainer.transform.SetParent(transform);
            
            for (int i = 0; i < starCount; i++)
            {
                GameObject star = new GameObject($"Star_{i}");
                star.transform.SetParent(starContainer.transform);
                
                SpriteRenderer renderer = star.AddComponent<SpriteRenderer>();
                renderer.sprite = CreateStarSprite();
                renderer.color = new Color(1f, 1f, 1f, starBrightness);
                renderer.sortingOrder = -100; // Behind everything
                
                Vector3 position;
                if (randomDistribution)
                {
                    position = Random.insideUnitSphere * distributionRadius;
                }
                else
                {
                    int gridSize = Mathf.CeilToInt(Mathf.Sqrt(starCount));
                    int x = i % gridSize;
                    int y = i / gridSize;
                    position = new Vector3(
                        (x / (float)gridSize - 0.5f) * distributionRadius * 2f,
                        (y / (float)gridSize - 0.5f) * distributionRadius * 2f,
                        0f
                    );
                }
                
                star.transform.localPosition = position;
                star.transform.localScale = Vector3.one * starSize;
            }
        }
        
        private Sprite CreateStarSprite()
        {
            // Create a simple white dot sprite
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++)
            {
                pixels[i] = Color.white;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            
            return Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}

