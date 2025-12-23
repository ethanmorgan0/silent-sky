# Silent Sky - Quick Start Guide

## Phase 1 MVP Implementation

This guide will help you get the Phase 1 MVP running.

## Prerequisites

- Python 3.8+
- `uv` package manager (install from https://astral.sh/uv)
- Unity 2021.3+ (for visualization)
- ZeroMQ libraries

## Python Setup

1. Install `uv` (if not already installed):
```bash
# Windows (PowerShell)
irm https://astral.sh/uv/install.ps1 | iex

# Or via pip
pip install uv
```

2. Navigate to `python/` directory:
```bash
cd python
```

3. Create virtual environment and install dependencies:
```bash
uv venv
uv pip install -e .
```

Or if you prefer to use the lock file (after running `uv lock`):
```bash
uv sync
```

## Running Python Backend (Headless)

Test the environment without Unity:
```bash
uv run python run_episode.py --headless --seed 42 --strategy greedy
```

Or activate the venv first:
```bash
# Windows PowerShell
.\.venv\Scripts\Activate.ps1
# Windows CMD
.venv\Scripts\activate.bat
# Linux/Mac
source .venv/bin/activate

python run_episode.py --headless --seed 42 --strategy greedy
```

This will:
- Run a single episode with a dummy agent
- Save episode data to `data/episodes/`
- Print episode summary

## Running with Unity

1. **Start Python backend:**
```bash
uv run python run_episode.py --unity --seed 42
```

Or with activated venv:
```bash
python run_episode.py --unity --seed 42
```

2. **In Unity:**
   - Open the Unity project from `unity/Silent Sky/` directory (this is the Unity project root)
   - Install required packages (see `unity/README.md`)
   - Create scene with required GameObjects (see `unity/README.md`)
   - Press Play

The Unity client will connect to Python and visualize the episode.

## Testing Different Strategies

Try different dummy agent strategies:
```bash
uv run python run_episode.py --headless --strategy greedy
uv run python run_episode.py --headless --strategy round_robin
uv run python run_episode.py --headless --strategy hybrid
```

## Project Structure

```
silent-sky/
├── python/              # Python environment and agents
│   ├── silent_sky/     # Main package
│   ├── run_episode.py  # Main entry point
│   ├── pyproject.toml  # Project dependencies (uv)
│   └── requirements.txt # Legacy (kept for compatibility)
├── unity/              # Unity visualization client
│   └── Assets/Scripts/ # Unity C# scripts
├── data/               # Episode data
└── docs/               # Documentation
```

## Next Steps

- Phase 1: Unity + Dummy Agent (current)
- Phase 2: Real RL Agent (PPO with LSTM)
- Phase 3: Survival Layer (optional)
- Phase 4: Observability Tools (optional)

## Troubleshooting

**Python errors:**
- Make sure all dependencies are installed: `uv pip install -e .`
- Check Python version: `python --version` (should be 3.8+)
- Make sure `uv` is installed: `uv --version`

**Unity connection issues:**
- Verify ZeroMQ ports match (default: 5555, 5556)
- Check firewall settings
- Try mock data mode in Unity (`useMockData = true`)

**Episode not saving:**
- Check `data/episodes/` directory exists
- Verify write permissions

