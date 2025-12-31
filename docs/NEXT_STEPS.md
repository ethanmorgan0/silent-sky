# Next Steps - Development Roadmap

## Current Status ✅

**Architecture:** Unity ML-Agents (Unity is authoritative, Python for training only)

**Completed:**
- ✅ Event-to-hexagon mapping fixed (cross product sign bug resolved)
- ✅ Visual debugging tools (HexagonMappingDebugger, SphericalCoordinateDebugger)
- ✅ Environment visualization working (events, signals, hexagons)
- ✅ Coordinate system unified (Sphere → Viewport → Hexagon)
- ✅ Unity world model foundation (FakeDataGenerator, SignalCalculator, SpaceEvent)
- ✅ Unity visualization components (SectorMap, EventVisualizer, StarfieldBackground)
- ✅ Viewport rotation system (functional but has bugs)
- ✅ 19 hexagons in JWST honeycomb pattern
- ✅ Spherical coordinate system with two-step mapping

**Working:**
- Events generate and map correctly to hexagons
- Signals calculate and display properly
- Visual debugging tools available
- Viewport rotation works (but has theta jumping and phi clamping issues)

**Not Yet Implemented:**
- ML-Agents integration (next major milestone)
- Event type differentiation (Nebulas, Comets, Supernovae)
- Noisy sensor system (currently using ground truth)
- Python training connection

## Recommended Next Steps

### 1. Fix Viewport Rotation Bugs (High Priority) 🔧

**Goal:** Fix theta jumping at 0/2π boundary and refine phi clamping behavior.

**Current Status:**
- ✅ Rotation system implemented (ViewportRotationController, ViewportProjection)
- ⚠️ Theta jumping: Horizontal snapping at 0/2π boundary
- ⚠️ Phi clamping: Vertical movement clamped, may need refinement

**Why:** 
- Rotation is functional but buggy
- Needs to be smooth and continuous for good UX
- Foundation for future gameplay (choosing which part of sky to observe)

**Implementation:**
- Fix delta calculation in UpdateRotation for theta
- Improve smooth rotation interpolation
- Refine phi clamping behavior (or implement proper pole handling)
- Test continuous rotation in both directions

**Estimated Effort:** Medium (1-2 days)

**Note:** Rotation is implemented but needs bug fixes. See `docs/ROTATION_DEBUG_ANALYSIS.md`.

---

### 2. Event Type Differentiation (High Priority) ⭐

**Goal:** Implement distinct event types with different characteristics (Nebulas, Comets, Supernovae) in Unity world model.

**Why:**
- Creates strategic depth (different strategies for different types)
- Aligns with design vision (README mentions event types)
- Makes gameplay more interesting (risk/reward tradeoffs)
- Foundation for ML-Agents environment

**Current State:**
- ✅ `FakeDataGenerator` generates events (all generic "Event" type)
- ✅ `SpaceEvent` has `eventType` field (currently just "Event")
- ⏳ No type differentiation yet

**Implementation:**

**Unity Side (`unity/Silent Sky/Assets/Scripts/Environment/`):**
- Update `FakeDataGenerator.cs`:
  - Generate different event types with distinct characteristics
  - **Nebulas**: Constant presence (always active), lower value, no timestamp/duration limits
  - **Comets**: Cyclical timing patterns, moderate value, predictable windows
  - **Supernovae**: Random occurrence, high value, short duration
  
- Update `SpaceEvent.cs`:
  - Add type-specific behaviors (IsActive logic per type)
  - Support constant presence for nebulas
  - Support cyclical patterns for comets
  
- Update `EventVisualizer.cs`:
  - Visual differentiation by type:
    - Nebulas: Blue, constant glow
    - Comets: Green, moving/trailing
    - Supernovae: Red/Orange, bright flash
  - Different rendering based on type

- Update `SignalCalculator.cs`:
  - Handle type-specific signal contributions
  - Nebulas contribute constant baseline signal

**Future (ML-Agents):**
- Event types will be part of ground truth state
- Agent will learn to recognize patterns per type
- Type-specific reward values

**Estimated Effort:** Medium (2-3 days)

---

### 3. Enhanced Event Patterns (Medium Priority) 📊

**Goal:** Improve event generation with more sophisticated patterns.

**Why:**
- Makes events learnable but non-repetitive
- Creates strategic depth (agent must learn patterns each episode)
- Aligns with design vision (pattern learning system)

**Current State:**
- Simple hot/cold sector patterns
- Basic temporal clustering (mentioned but not fully implemented)
- No spatial correlations
- No event dependencies

**Implementation:**

**Python Side:**
- **Spatial Correlations:**
  - Active sectors tend to be near other active sectors
  - Create "hot regions" on the sphere
  - Use distance-based correlation (nearby sectors have similar activity)
  
- **Temporal Clustering:**
  - Events cluster in time windows
  - If major event occurs, more events likely nearby in time
  - Implement time-based correlation matrix
  
- **Event Dependencies:**
  - Major events sometimes preceded by minor events in same sector
  - Create "precursor" events
  - Agent can learn to watch for precursors
  
- **Pattern Seeds:**
  - Each episode uses different pattern seed
  - Patterns are learnable but vary between episodes
  - Agent must learn general pattern recognition, not memorization

**Estimated Effort:** Medium (2-3 days)

---

### 4. ML-Agents Integration (High Priority) 🤖

**Goal:** Integrate Unity ML-Agents framework and connect Python training.

**Why:**
- Required for RL training (architecture decision)
- ML-Agents handles communication automatically
- Single source of truth in Unity eliminates sync issues

**Current State:**
- ✅ World model foundation exists in Unity
- ⏳ ML-Agents package not installed
- ⏳ No Academy/Agent structure yet

**Implementation:**
- **Unity Side:**
  - Install ML-Agents package via Package Manager
  - Create `ObservatoryAcademy.cs` (inherits from Academy)
  - Create `ObservatoryAgent.cs` (inherits from Agent)
  - Port environment logic to ML-Agents structure:
    - Event generation → Academy/Agent
    - Sensor system → Agent observations
    - Reward calculation → Agent rewards
  - Implement POMDP observations (noisy sensor readings only)
  - Configure BehaviorParameters for action/observation spaces
  
- **Python Side:**
  - Install ML-Agents Python package (`mlagents`)
  - Create training script that connects via ML-Agents Gym interface
  - Use Stable-Baselines3 PPO with LSTM policy
  - Test connection and training

**Estimated Effort:** High (5-7 days)

**Note:** This is the next major milestone. See `C:\Users\eam\.cursor\plans\unity_ml-agents_environment_implementation_76ba925e.plan.md` for detailed plan.

---

### 5. Visual Polish & UX (Low Priority) 🎨

**Goal:** Improve visualization and user experience.

**Why:**
- Makes the game more engaging
- Better debugging and understanding
- Professional polish

**Ideas:**
- **Event Visualization:**
  - Better visual differentiation by event type
  - Temporal indicators (when events will appear)
  - Value indicators (hover tooltips)
  
- **Hexagon Display:**
  - Better color gradients for signals
  - Uncertainty visualization
  - Activity rate indicators
  
- **UI Improvements:**
  - Time controls (pause, speed up, slow down)
  - Event timeline visualization
  - Statistics panel (events found, missed, earnings)
  
- **Camera/Viewport:**
  - Smooth rotation animations
  - Zoom controls
  - Viewport bounds visualization

**Estimated Effort:** Ongoing (can be done incrementally)

---

## Recommended Order

1. **Fix Viewport Rotation Bugs** (High priority, current blocker)
2. **Event Type Differentiation** (High impact, creates depth, foundation for ML-Agents)
3. **ML-Agents Integration** (Required for RL training, major milestone)
4. **Enhanced Event Patterns** (Medium impact, improves learning)
5. **Visual Polish** (Ongoing, as needed)

**Note:** Python-Unity Bridge (ZMQ) is deprecated - will be replaced by ML-Agents communication.

## Notes

- **Architecture:** Unity is authoritative (ML-Agents), Python is training-only
- **Unity Focus:** Steps 1, 2, 3 are primarily Unity work
- **Python Focus:** Step 3 (ML-Agents) requires Python training code
- **Integration:** ML-Agents handles communication automatically (no ZMQ needed)
- **Legacy:** Python environment code exists but is deprecated (reference only)

## Questions to Consider

1. **Sphere Rotation:**
   - Should rotation be agent-controlled or player-only?
   - Should rotation cost resources?
   - Continuous or discrete rotation?

2. **Event Types:**
   - Should all types be visible from start, or unlock progressively?
   - How do event types interact with upgrades?
   - Should temporal structure (20 nights/year) be implemented now?

3. **Patterns:**
   - How complex should patterns be?
   - Should patterns be deterministic (same seed = same pattern)?
   - How do patterns interact with event types?

4. **Bridge:**
   - Is mock data sufficient for now?
   - When do we need real Python-Unity communication?
   - Should we prioritize this or defer?

