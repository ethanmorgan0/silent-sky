"""Training pipeline - placeholder for Phase 2"""

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))

from silent_sky.utils.config import load_config


def main():
    parser = argparse.ArgumentParser(description="Train RL agent")
    parser.add_argument("--unity", action="store_true", help="Connect to Unity for visualization")
    parser.add_argument("--headless", action="store_true", default=True, help="Run headless (default)")
    parser.add_argument("--episodes", type=int, default=100, help="Number of episodes to train")
    parser.add_argument("--config", type=str, default=None, help="Config file path")
    
    args = parser.parse_args()
    
    print("Training pipeline not implemented in Phase 1.")
    print("Phase 1 focuses on Unity + Dummy Agent.")
    print("PPO training will be added in Phase 2.")
    
    # This will be implemented in Phase 2 with Stable-Baselines3


if __name__ == "__main__":
    main()

