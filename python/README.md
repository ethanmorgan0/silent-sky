# Silent Sky Python Environment

## Setup

1. Install `uv` (if not already installed):
```bash
# Windows (PowerShell)
irm https://astral.sh/uv/install.ps1 | iex

# Or via pip
pip install uv
```

2. Create virtual environment and install dependencies:
```bash
uv venv
uv pip install -e .
```

Or use `uv sync` if you have a `uv.lock` file:
```bash
uv sync
```

## Running

### Single Episode (Headless)
```bash
uv run python run_episode.py --headless --seed 42
```

Or activate the virtual environment first:
```bash
# Windows PowerShell
.\.venv\Scripts\Activate.ps1
# Windows CMD
.venv\Scripts\activate.bat
# Linux/Mac
source .venv/bin/activate

python run_episode.py --headless --seed 42
```

### Single Episode (with Unity)
```bash
uv run python run_episode.py --unity --seed 42
```

Or with activated venv:
```bash
python run_episode.py --unity --seed 42
```

### Options
- `--unity`: Connect to Unity for visualization
- `--headless`: Run without Unity (default)
- `--seed N`: Set random seed
- `--agent dummy|ppo`: Agent type (only dummy in Phase 1)
- `--strategy greedy|round_robin|hybrid`: Dummy agent strategy
- `--output filename`: Save episode to file

## Testing

Run a quick test:
```bash
uv run python run_episode.py --headless --seed 42 --strategy greedy
```

## Project Structure

- `silent_sky/env/`: Environment implementation
- `silent_sky/agent/`: Agent implementations
- `silent_sky/bridge/`: Python-Unity communication
- `silent_sky/utils/`: Utilities (logging, config)

## Configuration

Edit `config.yaml` to change environment parameters, Unity connection settings, etc.

