"""Single episode runner - main entry point for Phase 1"""

import argparse
import sys
from pathlib import Path

# Add parent directory to path
sys.path.insert(0, str(Path(__file__).parent))

from silent_sky.env.observatory_env import ObservatoryEnv
from silent_sky.agent.dummy_agent import DummyAgent
from silent_sky.bridge.zmq_bridge import ZMQBridge
from silent_sky.utils.logging import EpisodeLogger
from silent_sky.utils.config import load_config


def main():
    parser = argparse.ArgumentParser(description="Run a single episode")
    parser.add_argument("--unity", action="store_true", help="Connect to Unity")
    parser.add_argument("--headless", action="store_true", help="Run without Unity (default)")
    parser.add_argument("--seed", type=int, default=None, help="Random seed")
    parser.add_argument("--agent", choices=["dummy", "ppo"], default="dummy", help="Agent type")
    parser.add_argument("--strategy", choices=["greedy", "round_robin", "hybrid"], default="greedy", help="Dummy agent strategy")
    parser.add_argument("--output", type=str, default=None, help="Output file for episode")
    parser.add_argument("--config", type=str, default=None, help="Config file path")
    
    args = parser.parse_args()
    
    # Load config
    config = load_config(args.config)
    
    # Override with command line args
    if args.seed is not None:
        config["environment"]["seed"] = args.seed
    if args.agent == "dummy":
        config["agent"]["dummy_strategy"] = args.strategy
    
    # Create environment
    env = ObservatoryEnv(
        num_sectors=config["environment"]["num_sectors"],
        episode_length=config["environment"]["episode_length"],
        initial_budget=config["environment"]["initial_budget"],
        seed=config["environment"]["seed"]
    )
    
    # Create agent
    if args.agent == "dummy":
        agent = DummyAgent(
            strategy=config["agent"]["dummy_strategy"],
            seed=config["environment"]["seed"]
        )
    else:
        raise NotImplementedError("PPO agent not implemented in Phase 1")
    
    # Setup ZeroMQ bridge if Unity enabled
    bridge = None
    if args.unity or (not args.headless and config["unity"]["enabled"]):
        bridge = ZMQBridge(
            pub_port=config["unity"]["pub_port"],
            rep_port=config["unity"]["rep_port"],
            enabled=True
        )
        bridge.start()
        
        # Set callback for mission directives
        def handle_directive(directive: dict):
            if "reward_weights" in directive:
                env.update_mission_directives(directive["reward_weights"])
            if "upgrade" in directive:
                env.purchase_upgrade(directive["upgrade"])
        
        bridge.set_directive_callback(handle_directive)
    
    # Setup episode logger
    logger = EpisodeLogger(log_dir=config["logging"]["episode_dir"])
    
    # Episode data for logging
    episode_data = {
        "seed": config["environment"]["seed"],
        "timesteps": [],
        "actions": [],
        "observations": [],
        "rewards": [],
        "info": []
    }
    
    # Run episode
    observation, info = env.reset(seed=config["environment"]["seed"])
    agent.reset()
    
    total_reward = 0.0
    done = False
    
    print(f"Starting episode (seed={config['environment']['seed']})...")
    
    while not done:
        # Agent selects action
        action = agent.act(observation)
        
        # Step environment
        next_observation, reward, terminated, truncated, step_info = env.step(action)
        
        # Send state to Unity if connected
        if bridge and env.state:
            bridge.send_state(env.state, next_observation, step_info)
        
        # Log step
        episode_data["timesteps"].append(env.state.timestep)
        episode_data["actions"].append(action)
        episode_data["observations"].append({
            k: v.tolist() if hasattr(v, 'tolist') else v
            for k, v in observation.items()
        })
        episode_data["rewards"].append(float(reward))
        episode_data["info"].append(step_info)
        
        total_reward += reward
        observation = next_observation
        done = terminated or truncated
        
        if env.state.timestep % 10 == 0:
            print(f"Step {env.state.timestep}/{config['environment']['episode_length']}, "
                  f"Reward: {reward:.2f}, Budget: ${env.state.budget:.2f}")
    
    # Final state
    if env.state:
        money_info = env.reward_calculator.calculate_money(env.state)
        episode_data["final_state"] = {
            "total_reward": float(total_reward),
            "budget": float(env.state.budget),
            "earnings": money_info["earnings"],
            "costs": money_info["costs"],
            "profit": money_info["profit"],
            "events_discovered": len(env.state.discovered_events),
            "events_total": len(env.state.events)
        }
    
    print(f"\nEpisode complete!")
    print(f"Total reward: {total_reward:.2f}")
    if env.state:
        print(f"Budget: ${env.state.budget:.2f}")
        print(f"Events discovered: {len(env.state.discovered_events)}/{len(env.state.events)}")
    
    # Save episode
    if args.output:
        filename = args.output
    else:
        filename = None
    
    saved_path = logger.save_episode(episode_data, format="json", filename=filename)
    print(f"Episode saved to: {saved_path}")
    
    # Cleanup
    if bridge:
        bridge.stop()
    
    return episode_data


if __name__ == "__main__":
    main()

