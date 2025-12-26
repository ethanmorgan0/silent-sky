# Next Steps - Development Roadmap

## Current Status ✅

**Completed:**
- ✅ Event-to-hexagon mapping fixed (cross product sign bug resolved)
- ✅ Visual debugging tools (HexagonMappingDebugger)
- ✅ Environment visualization working (events, signals, hexagons)
- ✅ Coordinate system unified (Sphere → Viewport → Hexagon)
- ✅ Python environment structure exists (events, sensors, rewards, state)
- ✅ Unity visualization components (SectorMap, EventVisualizer, StarfieldBackground)

**Working:**
- Events generate and map correctly to hexagons
- Signals calculate and display properly
- Visual debugging tools available

## Recommended Next Steps

### 1. Sphere Rotation (High Priority) 🌐

**Goal:** Add ability to rotate the viewport/camera around the sphere to observe different regions.

**Why:** 
- Enables strategic gameplay (choosing which part of sky to observe)
- Aligns with design vision (360-degree sphere, field of view)
- Creates meaningful tradeoffs (can't see everything at once)

**Implementation:**
- **Unity Side:**
  - Add rotation controls (keyboard/mouse input)
  - Update `ViewportProjection` to accept rotation offset (theta_offset, phi_offset)
  - Rotate the FOV window around the sphere
  - Update hexagon mapping to account for rotation
  - Visual feedback showing current viewport orientation
  
- **Python Side:**
  - Add viewport rotation state to environment
  - Make rotation an agent action (or player control)
  - Update observation space to reflect rotated viewport
  - Consider: Is rotation an agent action or player-only control?

**Design Questions:**
- Should rotation be continuous or discrete (snap to hexagon centers)?
- Should rotation cost resources (time/money)?
- Should agent control rotation, or is it player-only?
- How does rotation interact with upgrades (wider FOV)?

**Estimated Effort:** Medium (2-3 days)

---

### 2. Event Type Differentiation (High Priority) ⭐

**Goal:** Implement distinct event types with different characteristics (Nebulas, Comets, Supernovae).

**Why:**
- Creates strategic depth (different strategies for different types)
- Aligns with design vision (README mentions event types)
- Makes gameplay more interesting (risk/reward tradeoffs)

**Current State:**
- Python has 3 basic types: NOISE, MINOR_TRANSIENT, MAJOR_TRANSIENT
- All events are essentially the same (just different values/probabilities)
- No temporal structure or distinct behaviors

**Implementation:**

**Python Side (`python/silent_sky/env/events.py`):**
- Add new event types: `NEBULA`, `COMET`, `SUPERNOVA`
- **Nebulas:**
  - Constant presence (always observable)
  - Lower value, low risk
  - Steady income stream
  - No decay (always there)
  
- **Comets:**
  - Cyclical timing patterns
  - Predictable appearance windows
  - Moderate value
  - Can learn cycles
  
- **Supernovae:**
  - Random occurrence (unpredictable)
  - Very high value
  - High risk/high reward
  - Rare but exciting

- Add temporal structure:
  - Shortened year cycle (~20 nights/year)
  - Each episode = one year
  - Events follow yearly patterns

- Update event generation:
  - Different generation logic per type
  - Temporal clustering for comets
  - Random distribution for supernovae
  - Constant presence for nebulas

**Unity Side:**
- Visual differentiation:
  - Different colors/shapes for each event type
  - Nebulas: Blue, constant glow
  - Comets: Green, moving/trailing
  - Supernovae: Red/Orange, bright flash
  
- UI indicators:
  - Event type labels
  - Temporal predictions (for comets)
  - Value indicators

**Estimated Effort:** Medium-High (3-4 days)

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

### 4. Python-Unity Bridge (Medium Priority) 🔌

**Goal:** Get real ZeroMQ communication working (currently using mock data).

**Why:**
- Enables live Python-Unity integration
- Allows testing with real environment
- Prepares for RL training visualization

**Current State:**
- Python side: `zmq_bridge.py` exists and appears functional
- Unity side: `ZMQBridge.cs` uses mock data mode
- NetMQ packages removed (commented out)

**Implementation:**
- **Unity Side:**
  - Re-add NetMQ packages (or find alternative)
  - Implement real ZMQ subscriber
  - Handle connection lifecycle
  - Test with Python environment
  
- **Python Side:**
  - Verify `zmq_bridge.py` works correctly
  - Test state serialization/deserialization
  - Ensure JSON format matches Unity expectations

**Estimated Effort:** Low-Medium (1-2 days, depends on package issues)

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

1. **Sphere Rotation** (High impact, enables strategic gameplay)
2. **Event Type Differentiation** (High impact, creates depth)
3. **Enhanced Event Patterns** (Medium impact, improves learning)
4. **Python-Unity Bridge** (Enables integration testing)
5. **Visual Polish** (Ongoing, as needed)

## Notes

- **Defer RL:** All steps above can be done without RL agent
- **Python Focus:** Steps 2, 3, 4 are primarily Python work
- **Unity Focus:** Steps 1, 5 are primarily Unity work
- **Integration:** Step 4 enables testing Python changes in Unity

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

