"""Dummy agent - simple heuristic for Phase 1"""

import numpy as np
from typing import Dict, Optional
import gymnasium as gym


class DummyAgent:
    """Simple heuristic agent for gameplay validation"""
    
    def __init__(self, strategy: str = "greedy", seed: Optional[int] = None):
        """
        Initialize dummy agent
        
        Args:
            strategy: "greedy", "round_robin", or "hybrid"
            seed: Random seed for deterministic behavior
        """
        self.strategy = strategy
        self.rng = np.random.RandomState(seed)
        self.current_sector = 0
        self.step_count = 0
    
    def act(self, observation: Dict[str, np.ndarray]) -> Dict:
        """
        Select action based on observation
        
        Args:
            observation: Dict with sensor_readings, sensor_confidence, time_remaining, budget_remaining
        
        Returns:
            Dict with "sector" and "exposure_mode"
        """
        self.step_count += 1
        
        sensor_readings = observation["sensor_readings"]
        sensor_confidence = observation["sensor_confidence"]
        num_sectors = len(sensor_readings)
        
        if self.strategy == "greedy":
            # Observe sector with highest uncertainty (lowest confidence) or highest signal
            # Combine both: prioritize sectors with high signal but low confidence
            uncertainty_scores = (1.0 - sensor_confidence) + sensor_readings
            sector = int(np.argmax(uncertainty_scores))
            
            # Exposure mode based on uncertainty
            max_uncertainty = np.max(1.0 - sensor_confidence)
            if max_uncertainty > 0.7:
                exposure_mode = 2  # LONG for high uncertainty
            elif max_uncertainty > 0.4:
                exposure_mode = 1  # MEDIUM
            else:
                exposure_mode = 0  # SHORT
        
        elif self.strategy == "round_robin":
            # Cycle through sectors systematically
            sector = self.current_sector
            self.current_sector = (self.current_sector + 1) % num_sectors
            exposure_mode = 1  # Always MEDIUM
        
        elif self.strategy == "hybrid":
            # Greedy when events detected (high signals), round-robin otherwise
            max_signal = np.max(sensor_readings)
            if max_signal > 0.5:
                # Event detected - be greedy
                uncertainty_scores = (1.0 - sensor_confidence) + sensor_readings
                sector = int(np.argmax(uncertainty_scores))
                exposure_mode = 2  # LONG for events
            else:
                # No events - round-robin
                sector = self.current_sector
                self.current_sector = (self.current_sector + 1) % num_sectors
                exposure_mode = 0  # SHORT for scanning
        
        else:
            # Random fallback
            sector = self.rng.randint(0, num_sectors)
            exposure_mode = self.rng.randint(0, 3)
        
        return {
            "sector": sector,
            "exposure_mode": exposure_mode
        }
    
    def reset(self):
        """Reset agent state"""
        self.current_sector = 0
        self.step_count = 0

