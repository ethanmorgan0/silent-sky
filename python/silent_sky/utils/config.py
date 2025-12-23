"""Configuration management"""

import yaml
from pathlib import Path
from typing import Dict, Any, Optional


def load_config(config_path: Optional[str] = None) -> Dict[str, Any]:
    """Load configuration from YAML file"""
    if config_path is None:
        config_path = "config.yaml"
    
    config_file = Path(config_path)
    
    if not config_file.exists():
        # Return default config
        return get_default_config()
    
    with open(config_file, 'r') as f:
        config = yaml.safe_load(f)
    
    # Merge with defaults
    defaults = get_default_config()
    defaults.update(config)
    return defaults


def get_default_config() -> Dict[str, Any]:
    """Get default configuration"""
    return {
        "environment": {
            "num_sectors": 8,
            "episode_length": 100,
            "initial_budget": 1000.0,
            "seed": None
        },
        "unity": {
            "enabled": False,
            "pub_port": 5555,
            "rep_port": 5556
        },
        "agent": {
            "type": "dummy",
            "dummy_strategy": "greedy"  # or "round_robin", "hybrid"
        },
        "logging": {
            "episode_dir": "data/episodes",
            "log_level": "INFO"
        }
    }

