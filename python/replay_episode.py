"""Episode replay tool"""

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).parent))

from silent_sky.utils.logging import EpisodeLogger


def main():
    parser = argparse.ArgumentParser(description="Replay a saved episode")
    parser.add_argument("episode_file", type=str, help="Path to episode file")
    parser.add_argument("--agent", choices=["dummy", "ppo"], default="dummy", help="Agent to use for replay")
    parser.add_argument("--format", choices=["json", "pickle"], default="json", help="Episode file format")
    
    args = parser.parse_args()
    
    logger = EpisodeLogger()
    episode_data = logger.load_episode(args.episode_file, format=args.format)
    
    print(f"Loaded episode: {args.episode_file}")
    print(f"Seed: {episode_data.get('seed', 'unknown')}")
    print(f"Total steps: {len(episode_data.get('timesteps', []))}")
    
    if "final_state" in episode_data:
        final = episode_data["final_state"]
        print(f"\nFinal State:")
        print(f"  Total reward: {final.get('total_reward', 0):.2f}")
        print(f"  Budget: ${final.get('budget', 0):.2f}")
        print(f"  Profit: ${final.get('profit', 0):.2f}")
        print(f"  Events: {final.get('events_discovered', 0)}/{final.get('events_total', 0)}")
    
    print("\nReplay functionality will be implemented in Unity EpisodePlayer.")


if __name__ == "__main__":
    main()

