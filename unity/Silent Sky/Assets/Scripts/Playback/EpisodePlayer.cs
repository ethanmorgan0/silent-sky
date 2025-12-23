using System.Collections;
using UnityEngine;
using SilentSky.Unity.Bridge;

namespace SilentSky.Unity.Playback
{
    /// <summary>
    /// Minimal episode playback - Phase 1
    /// </summary>
    public class EpisodePlayer : MonoBehaviour
    {
        [Header("Playback Controls")]
        [SerializeField] private UnityEngine.UI.Button playButton;
        [SerializeField] private UnityEngine.UI.Button pauseButton;
        [SerializeField] private UnityEngine.UI.Button stepForwardButton;
        [SerializeField] private UnityEngine.UI.Button stepBackwardButton;
        [SerializeField] private UnityEngine.UI.Slider timelineSlider;
        
        private bool isPlaying = false;
        private int currentStep = 0;
        private EnvironmentState[] episodeData;
        
        private void Start()
        {
            // Setup button handlers
            if (playButton != null)
                playButton.onClick.AddListener(Play);
            if (pauseButton != null)
                pauseButton.onClick.AddListener(Pause);
            if (stepForwardButton != null)
                stepForwardButton.onClick.AddListener(StepForward);
            if (stepBackwardButton != null)
                stepBackwardButton.onClick.AddListener(StepBackward);
        }
        
        public void LoadEpisode(EnvironmentState[] data)
        {
            episodeData = data;
            currentStep = 0;
            UpdateDisplay();
        }
        
        public void Play()
        {
            isPlaying = true;
            StartCoroutine(PlaybackCoroutine());
        }
        
        public void Pause()
        {
            isPlaying = false;
        }
        
        public void StepForward()
        {
            if (episodeData != null && currentStep < episodeData.Length - 1)
            {
                currentStep++;
                UpdateDisplay();
            }
        }
        
        public void StepBackward()
        {
            if (currentStep > 0)
            {
                currentStep--;
                UpdateDisplay();
            }
        }
        
        public void JumpToStep(int step)
        {
            if (episodeData != null && step >= 0 && step < episodeData.Length)
            {
                currentStep = step;
                UpdateDisplay();
            }
        }
        
        private IEnumerator PlaybackCoroutine()
        {
            while (isPlaying && episodeData != null && currentStep < episodeData.Length)
            {
                UpdateDisplay();
                currentStep++;
                
                if (timelineSlider != null)
                {
                    timelineSlider.value = (float)currentStep / episodeData.Length;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            isPlaying = false;
        }
        
        private void UpdateDisplay()
        {
            if (episodeData == null || currentStep >= episodeData.Length) return;
            
            // Dispatch state update to visualization components
            var bridge = FindObjectOfType<ZMQBridge>();
            if (bridge != null)
            {
                // Trigger state update event
                // This would need to be exposed or we'd use a different mechanism
            }
        }
    }
}

