# Silent Sky - Current Implementation Status

**Last Updated:** 2025-01-XX

## Architecture

**Unity ML-Agents** - Unity is the authoritative environment, Python connects for training only.

See `docs/ARCHITECTURE.md` for full architecture details.

## Completed Components ✅

### Unity Environment (Foundation)

- ✅ **Spherical Coordinate System**: `SphericalCoordinateSystem.cs`
  - Theta/phi coordinates
  - Conversions to/from Cartesian
  - Normalization and clamping utilities

- ✅ **Viewport Projection**: `ViewportProjection.cs`
  - Equirectangular projection (180° × 120° FOV)
  - Sphere-to-viewport mapping
  - Viewport rotation support (theta_offset, phi_offset)
  - FOV boundary checking

- ✅ **Viewport Rotation**: `ViewportRotationController.cs`
  - Keyboard controls (Arrow keys/WASD)
  - Smooth rotation with interpolation
  - Rotation state management
  - Event notifications for components

- ✅ **Event System**: 
  - `SpaceEvent.cs`: Event data structure (type, value, theta, phi, timestamp, duration)
  - `FakeDataGenerator.cs`: Generates events in spherical space (deterministic, seedable)
  - Events have uniform distribution on sphere surface

- ✅ **Signal Calculation**: `SignalCalculator.cs`
  - Sums event values per hexagon (19 hexagons)
  - Uses unified two-step mapping (Sphere → Viewport → Hexagon)
  - Updates signals as events spawn/expire

- ✅ **Hexagon Mapping**: 
  - `HexagonGridMapper.cs`: Maps viewport coordinates to hexagon indices
  - `SphereToHexagonMapper.cs`: Unified sphere-to-hexagon mapping
  - Two-step process: Sphere → Viewport → Hexagon

- ✅ **Visualization**:
  - `StarfieldBackground.cs`: Procedurally generated starfield
  - `EventVisualizer.cs`: Visualizes events as bright stars on starfield
  - `SignalVisualizer.cs`: Maps signals to hexagon colors
  - `SectorMap.cs`: 19 hexagons in JWST honeycomb pattern
  - `SphereMinimap.cs`: Shows viewport orientation on full sphere

- ✅ **Debugging Tools**:
  - `SphericalCoordinateDebugger.cs`: Visualizes coordinate grid
  - `HexagonMappingDebugger.cs`: Visualizes hexagon mapping

### Coordinate System

- ✅ Unified spherical coordinate system (theta, phi)
- ✅ Two-step mapping: Sphere → Viewport → Hexagon
- ✅ Viewport rotation working (with some bugs to fix)
- ✅ 19 hexagons in JWST pattern (1 center + 6 ring 1 + 12 ring 2)

## In Progress / Known Issues 🔧

### Viewport Rotation

- ⚠️ **Theta jumping**: Horizontal snapping at 0/2π boundary (needs fix)
- ⚠️ **Phi clamping**: Vertical movement clamped to prevent pole crossing (may need refinement)
- ⚠️ **Smooth rotation**: Interpolation logic may need improvement

**Status**: Functional but buggy. See `docs/ROTATION_DEBUG_ANALYSIS.md` for details.

## Not Yet Implemented ⏳

### ML-Agents Integration

- ⏳ ML-Agents package installation
- ⏳ `ObservatoryAcademy.cs` (inherits from Academy)
- ⏳ `ObservatoryAgent.cs` (inherits from Agent)
- ⏳ Port environment logic to ML-Agents structure
- ⏳ POMDP observations (noisy sensor readings only)
- ⏳ Reward system in ML-Agents
- ⏳ Python training connection via ML-Agents Gym interface

### Event Type Differentiation

- ⏳ Distinct event types (Nebulas, Comets, Supernovae)
- ⏳ Type-specific behaviors:
  - Nebulas: Constant presence, low value
  - Comets: Cyclical timing patterns
  - Supernovae: Random, high value
- ⏳ Visual differentiation by type

### Sensor System

- ⏳ Noisy sensor readings (currently using ground truth)
- ⏳ Sensor confidence calculation
- ⏳ Exposure modes (SHORT/MEDIUM/LONG)
- ⏳ Sensor quality upgrades

### Reward System

- ⏳ Discovery value rewards
- ⏳ Operational cost penalties
- ⏳ Mission directive weights
- ⏳ Separation from money (agent never sees money)

### Python Training

- ⏳ ML-Agents Gym interface connection
- ⏳ Stable-Baselines3 PPO integration
- ⏳ LSTM policy for POMDP
- ⏳ Training pipeline
- ⏳ Episode serialization for replay

## Deprecated / Legacy

### Python Environment Code

The following Python files exist but are **deprecated** (architecture changed to Unity ML-Agents):

- `python/silent_sky/env/observatory_env.py` - Will be replaced by Unity ML-Agents
- `python/silent_sky/env/state.py` - State will be in Unity
- `python/silent_sky/env/events.py` - Events will be in Unity
- `python/silent_sky/env/sensors.py` - Sensors will be in Unity
- `python/silent_sky/env/rewards.py` - Rewards will be in Unity
- `python/silent_sky/bridge/zmq_bridge.py` - Will be replaced by ML-Agents

**Note**: These may be kept as reference during porting, but the authoritative implementation will be in Unity.

## Next Steps

1. **Fix viewport rotation bugs** (theta jumping, phi clamping)
2. **Implement event type differentiation** in Unity world model
3. **Begin ML-Agents integration** (install package, create Academy/Agent structure)
4. **Port environment logic** to ML-Agents (events, sensors, rewards)
5. **Connect Python training** via ML-Agents Gym interface

See `docs/NEXT_STEPS.md` for detailed roadmap.

## Key Design Decisions

1. **Unity is authoritative** - All world model logic in Unity C#
2. **ML-Agents for RL** - Python connects via ML-Agents Gym interface
3. **Spherical coordinates** - All positions use (theta, phi)
4. **Two-step mapping** - Sphere → Viewport → Hexagon
5. **19 hexagons** - JWST-style honeycomb pattern
6. **POMDP enforcement** - Agent never sees ground truth
7. **Deterministic** - All RNG is seedable for reproducibility

## Documentation

- `docs/ARCHITECTURE.md` - Current architecture (Unity ML-Agents)
- `docs/20251223_IMPLEMENTATION_PLAN.md` - Original plan (deprecated architecture)
- `docs/NEXT_STEPS.md` - Development roadmap
- `docs/ROTATION_DEBUG_ANALYSIS.md` - Rotation bug analysis
- `docs/SPHERE_ROTATION_DESIGN.md` - Rotation design decisions

