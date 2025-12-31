# Silent Sky

**Silent Sky is not a simulator of spaceflight. It is a game about deciding where to look when you cannot see everything, and what it means to miss something important.**

A space observatory simulation game that combines reinforcement learning with strategic gameplay. You play as a mission director managing an autonomous observatory agent that must operate under uncertainty, resource constraints, and time pressure.

## Core Vision

This is a **game first**, not just an RL project. The core fantasy is operating under uncertainty, making meaningful tradeoffs, and feeling the weight of missed opportunities. The game must be **fun and satisfying**, with clear player agency and meaningful progression.

### Key Design Principles

- **Game-first, not RL-first**: Every mechanic serves gameplay feel, not just RL correctness
- **POMDP enforcement**: Agent never receives ground truth; all observations are noisy
- **Layered constraints**: Time + resources + uncertainty create meaningful tradeoffs
- **Learnable but non-repetitive patterns**: Events follow procedural patterns that vary between episodes
- **Separation of concerns**: Python owns logic; Unity owns visualization
- **Player agency**: Strategic director role with tactical override capabilities

## Gameplay Overview

### The Setup

- **19 sky sectors** to observe (arranged in JWST-style hexagonal honeycomb pattern: 1 center + 6 in ring 1 + 12 in ring 2)
- **3 event types**: Noise (frequent, low value), Minor Transients (moderate), Major Transients (rare, high value)
- **One orbital cycle per episode** (fixed horizon, ~100 steps)
- **Partial observability**: Agent only sees noisy sensor readings, never ground truth
- **Financial system**: Operations cost money, discoveries earn money, upgrades unlock capabilities

### Player vs Agent Responsibilities

**PLAYER (Strategic Director):**

**Between Episodes:**
- Manage budget: Review earnings, plan spending, track financial progress
- Purchase upgrades: Choose which capabilities to improve (sensors, prediction, speed, processing)
- Set mission directives: Reward weights, risk tolerance, exploration bias
- Strategic planning: Analyze past episodes, identify patterns, plan upgrade path

**During Episodes (Limited Tactical Control):**
- **Priority markers**: Mark sectors as "high priority" (agent weights these higher)
- **Budget caps**: Set spending limits to prevent bankruptcy
- **Risk tolerance adjustments**: Modify agent behavior mid-episode
- Monitor agent performance and financial status
- *(Emergency override deferred to Phase 2 - adds complexity to agent-player contract)*

**After Episodes:**
- Review financial performance: Profit/loss, ROI on upgrades, cost per discovery
- Analyze missed opportunities: What could have been found with better upgrades?
- Learn patterns: Identify event patterns, plan next investments

**AGENT (Autonomous Observer):**

- **Tactical execution**: Decides which sector to observe, exposure time, when to switch
- **Autonomous operation**: Runs observation plan based on:
  - Current sensor readings and uncertainty
  - Mission directives (player preferences)
  - Learned patterns from training
  - Budget constraints (if player sets limits)
- **Adaptive behavior**: Responds to partial hints, adjusts strategy based on observations
- **Pattern learning**: Learns episode-specific patterns (sector activity, temporal clustering, spatial correlations)
- **No micromanagement**: Player cannot control individual actions (except emergency override)

### Currency & Upgrade System

**Revenue:**
- Major events: $1000-5000 (based on observation quality)
- Minor events: $200-800
- Noise: -$50 (false positive verification cost)

**Operational Costs:**
- Base observation: $10 per action
- Exposure time: $5 per 0.1 exposure time
- Sector switching: $20 per switch

**Upgrades (examples):**
- **Sensor upgrades**: Precision ($500/$1500/$3000), Field of view ($800/$2000), Sensitivity ($1000/$2500)
- **Prediction systems**: Event probability forecasts ($2000/$5000), Timing predictions ($3000/$7000)
- **Reaction speed**: Faster switching ($1500/$4000), Quick exposure ($1000/$3000)
- **Data processing**: Uncertainty reduction ($2000/$5000), Signal analysis ($2500/$6000)

Upgrades create visible improvements and unlock new strategies. Financial success enables better capabilities, creating meaningful progression.

### Pattern Learning System

**Learnable Patterns (Not Random):**

Events follow procedural patterns that the agent can learn:

- **Sector activity rates**: Each episode has "hot" sectors (high event rate) and "cold" sectors (low event rate). Which sectors are hot varies by episode.
- **Temporal clustering**: Events cluster in time windows. If a major event occurs, more events are likely nearby in time.
- **Spatial correlations**: Active sectors tend to be near other active sectors.
- **Event dependencies**: Major events are sometimes preceded by minor events in the same sector.

**Non-Repetitive (Not Predictable):**

- Each episode uses a different "pattern seed" → different sector activity, correlations, timing
- Agent must learn patterns each episode, not rely on memorized strategies
- Training across diverse episodes teaches general pattern recognition, not memorization
- Patterns are learnable but never fully deterministic (irreducible uncertainty)

**Agent Learning:**

- LSTM policy maintains hidden state to track learned patterns
- Observes noisy signals and learns episode-specific patterns through RL
- Adapts strategy mid-episode as patterns become clearer
- Never receives ground truth patterns (only noisy observations)
- Develops generalizable pattern recognition, not episode-specific memorization

**Upgrade Integration:**

- Prediction upgrades reveal partial pattern information:
  - "Sector activity forecast": Shows which sectors are likely hot (with uncertainty)
  - "Temporal predictions": Warns when major events are likely (with uncertainty)
- Upgrades reduce but never eliminate uncertainty

### Future Design: Event Type Differentiation & Temporal Structure

**Design Note (For Future Implementation):**

The policy system should be reactive to different event types with distinct characteristics:

**Temporal Structure:**
- Shortened in-game year cycle: ~20 nights/year (creates scarcity and makes each night more valuable)
- Each episode represents one year of observations

**Event Type Characteristics:**

1. **Nebulas** (Constant, Low Risk/Low Value)
   - Observable every night (constant presence)
   - Lower value, low risk
   - Provides baseline income stream
   - Agent can "rely on" these for steady revenue

2. **Comets** (Cyclical, Consistent)
   - Predictable timing patterns (cyclical)
   - Consistent appearance windows
   - Agent can learn and anticipate cycles
   - Creates strategic planning opportunities

3. **Supernovae** (Random, High Value)
   - Random occurrence (unpredictable)
   - Very high value when discovered
   - High risk/high reward tradeoff
   - Creates exciting "jackpot" moments

**Policy Implications:**
- Agent should develop different strategies for each event type
- Risk/reward tradeoffs vary by event type
- Temporal structure (20 nights/year) increases opportunity cost of each decision
- Agent must balance: steady income (nebulas) vs. predictable opportunities (comets) vs. high-risk gambles (supernovae)

## Technical Architecture

**IMPORTANT: Architecture has changed to Unity ML-Agents**

**Unity is the authoritative environment** - all world model logic runs in Unity C#. Python connects via ML-Agents Gym interface for training only.

### System Components

```
Unity (Authoritative Environment)        Python (Training Only)
├── Environment Logic (C#)               ├── Stable-Baselines3 PPO
│   ├── Event Generation                 │   ├── LSTM Policy
│   ├── Sensor System                    │   └── Training Pipeline
│   ├── Reward Calculation               └── ML-Agents Gym Interface
│   └── State Management                     (connects to Unity)
├── ML-Agents Academy/Agent
│   └── Gym Interface (auto-exposed)
└── Visualization
    ├── Sector Map (19 hexagons)
    ├── Event Visualizer
    ├── Starfield Background
    └── Signal Visualization
```

**See `docs/ARCHITECTURE.md` for full architecture details.**

### Unity Environment (ML-Agents)

- **ML-Agents framework**: Unity is the authoritative environment
- **Discrete action space**: Discrete sector selection (0-18) + discrete exposure mode (SHORT/MEDIUM/LONG) - future
- **POMDP observation space**: Only noisy sensor readings, sensor confidence, time remaining, and budget (no belief state leakage)
- **Reward system**: Rewards map to economic outcomes but agent never sees money directly
- **Deterministic seeding**: Full reproducibility and replay support
- **Headless training**: Can run Unity in batch mode for training

**Current Status:** Foundation implemented (FakeDataGenerator, SignalCalculator), ML-Agents integration in progress.

### Agent Training (Future)

- **Stable-Baselines3 PPO** with LSTM policy (via ML-Agents Gym interface)
- **Recurrent network**: Maintains pattern memory across episode
- **Episode-based training**: Train between episodes, not frame-by-frame
- **Pretraining**: Base policy pretrained on diverse pattern seeds for general competence
- **Fine-tuning**: Supports player preference adaptation
- **Non-repetitive training**: Curriculum with diverse pattern seeds prevents memorization

**Status:** Not yet implemented - requires ML-Agents integration first.

### Unity Client

- **Authoritative environment**: All world model logic runs in Unity
- **Visualization**: Real-time visualization of environment state
- **Live training view**: Watch agent operate in real-time (future)
- **Episode playback**: Review past episodes with full ground truth reveal (future)
- **Player interaction**: Mission directives, upgrade purchases (future)
- **Financial tracking**: Budget meters, revenue displays, ROI analysis (future)

### Communication

- **ML-Agents**: Automatic communication between Unity and Python
- **Gym interface**: Python connects via `gym.make()` with ML-Agents environment
- **No ZMQ needed**: ML-Agents handles all communication

**Status:** ML-Agents integration in progress. Current implementation uses mock data.

## Gameplay Loop

1. **Pre-episode**: Review budget, purchase upgrades, set mission directives
2. **During episode**: Watch agent operate, use emergency overrides if needed, monitor budget
3. **Post-episode**: Review financial performance, see missed opportunities, analyze efficiency
4. **Strategic planning**: Decide next upgrade path based on what you learned
5. **Iterate**: Each episode builds on previous investments and learnings

### Player Satisfaction Drivers

- **Financial success feels rewarding**: Profitable episodes, smart upgrade investments
- **Strategic planning pays off**: Right upgrades at right time = better discoveries
- **Partial hints create tension**: Ambiguous signals during episode ("something in sector 3, but what?")
- **Post-episode reveals create "what if" moments**: See what was missed, calculate potential earnings
- **Progression feels meaningful**: Upgrades unlock new strategies and capabilities
- **Agent learning is visible**: See agent adapt and improve as it learns patterns

## MVP Scope

**Included:**
- 8 sectors, 3 event types, 1 sensor
- Layered constraints: Time pressure + resource limits + uncertainty
- Partial hints + post-episode reveals
- Basic Unity visualization (sector map, events, uncertainty, efficiency metrics)
- Live training visualization + playback
- Episode serialization
- PPO with LSTM policy
- Mission directive UI, upgrade shop (simplified: 3-4 binary upgrades)
- Currency system and financial tracking
- Basic pattern system (sector activity rates, simple temporal clustering)

**Excluded (for now):**
- Multiple instruments
- Complex physics
- Rival agents
- React dashboard (Phase 2)
- Complex event patterns (more sophisticated patterns in future iterations)

## Development Guidelines

### Coding Principles

- **Explicit state and clear data contracts**: Favor clarity over cleverness
- **Determinism and reproducibility**: Full seeding support, deterministic replay
- **Many small files over large monoliths**: Single-responsibility modules
- **Unity scripts**: Thin, single-responsibility MonoBehaviours
- **Python environment**: Gym-style patterns, headless operation
- **Prioritize debuggability**: Strong instrumentation and logging

### Architecture Rules

- **Python is authoritative**: Owns environment state, event generation, rewards, transitions
- **Unity is visualization only**: Mirrors state, displays uncertainty, collects player input
- **No perfect-information shortcuts**: Agent never receives ground truth
- **Player never sees more than agent**: Maintains uncertainty and partial observability
- **Separation of concerns**: Python owns logic; Unity owns presentation

### Testing Strategy

- Unit tests for environment logic (deterministic)
- Integration tests for Python-Unity bridge
- Headless training validation (no Unity required)
- Episode replay verification
- Pattern learning validation (agent learns, doesn't memorize)

## Project Structure

```
silent-sky/
├── python/
│   ├── silent_sky/
│   │   ├── env/              # Environment logic
│   │   ├── agent/            # RL training
│   │   ├── bridge/           # Python-Unity communication
│   │   └── utils/            # Logging, config
│   └── train.py              # Training entry point
├── unity/
│   └── Assets/Scripts/       # Unity visualization and UI
├── data/episodes/            # Serialized episodes
└── README.md
```

## Success Criteria

- Player has clear agency: Budget management, upgrade choices, emergency overrides
- Financial system creates meaningful stakes: Profit/loss matters, upgrades feel impactful
- Player can watch agent and feel tension from tradeoffs (time, money, uncertainty)
- Post-episode review creates "what if" moments (missed earnings, upgrade ROI)
- Adjusting mission directives meaningfully changes agent behavior
- Upgrades create visible improvements: Better sensors = better results
- Agent learns patterns without feeling repetitive or predictable
- Game is playable and satisfying, not just technically correct
- Player feels like a strategic director, not a passive observer

## Remember

This project should feel like:

**"A space mission where intelligence slowly emerges under fog and constraint."**

Not:

**"An RL agent optimizing a toy problem."**

The game must be fun, the tradeoffs must feel meaningful, and the player must have agency. Reinforcement learning exists to support gameplay, decision tension, and emergent, inspectable behavior.
