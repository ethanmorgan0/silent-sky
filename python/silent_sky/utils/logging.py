"""Episode serialization and logging"""

import json
import pickle
from pathlib import Path
from typing import Dict, List, Optional
from datetime import datetime

from ..env.state import EnvironmentState


class EpisodeLogger:
    """Logs and serializes episodes for replay"""
    
    def __init__(self, log_dir: str = "data/episodes"):
        self.log_dir = Path(log_dir)
        self.log_dir.mkdir(parents=True, exist_ok=True)
    
    def save_episode(
        self,
        episode_data: Dict,
        format: str = "json",
        filename: Optional[str] = None
    ) -> str:
        """
        Save episode data
        
        Args:
            episode_data: Dict containing full episode state, actions, observations, rewards
            format: "json" or "pickle"
            filename: Optional custom filename
        
        Returns:
            Path to saved file
        """
        if filename is None:
            timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
            filename = f"episode_{timestamp}"
        
        filepath = self.log_dir / f"{filename}.{format}"
        
        if format == "json":
            # Convert numpy arrays to lists for JSON
            json_data = self._to_json_serializable(episode_data)
            with open(filepath, 'w') as f:
                json.dump(json_data, f, indent=2)
        elif format == "pickle":
            with open(filepath, 'wb') as f:
                pickle.dump(episode_data, f)
        else:
            raise ValueError(f"Unknown format: {format}")
        
        return str(filepath)
    
    def load_episode(self, filepath: str, format: str = "json") -> Dict:
        """Load episode data"""
        path = Path(filepath)
        
        if format == "json":
            with open(path, 'r') as f:
                return json.load(f)
        elif format == "pickle":
            with open(path, 'rb') as f:
                return pickle.load(f)
        else:
            raise ValueError(f"Unknown format: {format}")
    
    def _to_json_serializable(self, obj):
        """Convert numpy arrays and other types to JSON-serializable"""
        import numpy as np
        
        if isinstance(obj, np.ndarray):
            return obj.tolist()
        elif isinstance(obj, (np.integer, np.floating)):
            return float(obj)
        elif isinstance(obj, dict):
            return {k: self._to_json_serializable(v) for k, v in obj.items()}
        elif isinstance(obj, list):
            return [self._to_json_serializable(item) for item in obj]
        else:
            return obj

