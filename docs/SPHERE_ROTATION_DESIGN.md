# Sphere Rotation & 3D Visualization Design Analysis

## Current System Architecture

### Gameplay Layer (Abstract Sphere)
- **Coordinate System**: `(theta, phi)` - 2D spherical coordinates
- **Events**: Abstract gameplay entities with angular position only
- **Sectors**: Discrete hexagon regions mapped to sphere
- **Signal Calculation**: Uses angular position to map events to hexagons
- **Viewport Projection**: Equirectangular projection maps `(theta, phi)` → 2D viewport `[0,1]`

### Visualization Layer (2D UI)
- **Rendering**: 2D UI elements (hexagons, stars, event markers)
- **Positioning**: Uses `ViewportProjection` to convert sphere coords to UI space
- **No 3D**: Everything is flat 2D projection

## Future Vision Requirements

### Visual Objects
- **3D Positions**: Objects need `(r, theta, phi)` - radial distance + angular position
- **Distance-Based Rendering**:
  - Far objects (large r): 2D sprites/images
  - Close objects (small r): 3D models with physics simulation
- **Hybrid Events**: Some events have visual representations, some visuals are decorative
- **Minimap**: Show current viewport orientation on full sphere

## Design Compatibility Analysis

### ✅ **Compatible: Layered Architecture**

The current abstract sphere approach **can coexist** with 3D visualization. Here's how:

```
┌─────────────────────────────────────────────────────────┐
│  Gameplay Layer (Abstract Sphere)                       │
│  - Events: (theta, phi) only                          │
│  - Sectors: Hexagon mapping                                  │
│  - Signals: Angular-based calculation                  │
│  - No knowledge of distance                             │
└─────────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────────┐
│  Visualization Layer (3D Rendering)                      │
│  - Objects: (r, theta, phi)                            │
│  - Camera: Rotates around sphere center                 │
│  - Rendering: Distance-based (sprite vs 3D model)        │
│  - Viewport: Projects 3D objects to 2D screen          │
└─────────────────────────────────────────────────────────┘
```

### Key Design Principles

1. **Separation of Concerns**:
   - Gameplay logic stays abstract (theta, phi only)
   - Visualization adds distance (r) for rendering
   - Events can have associated visual objects, but gameplay doesn't need r

2. **Mapping Strategy**:
   - Events at `(theta, phi)` can have visual objects at `(r, theta, phi)`
   - Visual objects can exist independently (decorative)
   - Distance (r) is purely visual - doesn't affect gameplay

3. **Viewport Rotation**:
   - Rotate the camera/viewport around sphere center
   - Objects at different distances render correctly
   - Minimap shows viewport orientation on abstract sphere

## Implementation Approach

### Phase 1: Sphere Rotation (Current Priority)

**What We're Building:**
- Rotate viewport around abstract sphere
- Keep current 2D UI visualization
- Add rotation controls (keyboard/mouse)
- Minimap showing viewport orientation

**Architecture:**
- Extend `ViewportProjection` to accept rotation offset `(theta_offset, phi_offset)`
- Update hexagon mapping to account for rotation
- Add rotation state to environment
- Visual feedback for current orientation

**Compatibility:**
- ✅ Fully compatible with current system
- ✅ No changes to gameplay layer needed
- ✅ Sets foundation for 3D visualization

### Phase 2: 3D Visualization Layer (Future)

**What We'll Add:**
- 3D camera system looking outward from sphere center
- Objects positioned at `(r, theta, phi)` in 3D space
- Distance-based rendering (sprite vs 3D model)
- Visual objects associated with events

**Architecture:**
```
Gameplay Event (theta, phi)
    ↓
Visual Object Manager
    ↓
3D Object at (r, theta, phi)
    ↓
Camera Rendering (viewport rotation)
    ↓
2D Screen Display
```

**Key Components:**
1. **VisualObjectManager**: Maps events to visual objects, manages decorative objects
2. **DistanceRenderer**: Chooses sprite vs 3D model based on distance
3. **3DCameraSystem**: Rotates camera around sphere, projects to viewport
4. **ObjectPool**: Manages visual object lifecycle

**Compatibility:**
- ✅ Gameplay layer unchanged (still uses theta, phi)
- ✅ Visualization layer adds distance (r) independently
- ✅ Events can have visuals, but gameplay doesn't need them

## Design Decisions

### Do We Need to Render "Inside of Sphere"?

**Answer: No - We Render Objects in 3D Space**

We don't need to render the inside of a sphere. Instead:
- Camera is at origin (0, 0, 0) or slightly offset
- Objects are positioned in 3D space at `(r, theta, phi)`
- Camera looks outward, rotating around origin
- Objects render based on distance and viewport

**Why This Works:**
- Gameplay uses abstract sphere (just coordinates)
- Visualization renders objects in 3D space
- Camera rotation = viewport rotation
- Objects at different distances render correctly

### Coordinate System Mapping

**Gameplay → Visualization:**
```
Event (theta, phi) 
  → Visual Object (r, theta, phi)
    → 3D Position: (r*sin(phi)*cos(theta), r*cos(phi), r*sin(phi)*sin(theta))
      → Camera View → Screen
```

**Key Insight:** 
- Gameplay only needs `(theta, phi)` - angular position
- Visualization adds `r` - radial distance
- The two are independent - gameplay doesn't care about distance

## Sphere Rotation Implementation Plan

### 1. Extend ViewportProjection

**Current:**
- Fixed center at `(theta=0, phi=π/2)`
- Projects `(theta, phi)` → viewport `[0,1]`

**New:**
- Accept rotation offset `(theta_offset, phi_offset)`
- Rotate viewport center before projection
- Update `ProjectToViewport(theta, phi, theta_offset, phi_offset)`

### 2. Rotation Controls

**Input:**
- Keyboard: Arrow keys or WASD for rotation
- Mouse: Drag to rotate (optional)
- Smooth rotation with easing

**State:**
- Current viewport orientation `(theta_offset, phi_offset)`
- Rotation speed/limits
- Clamp to prevent gimbal lock

### 3. Hexagon Mapping Update

**Current:**
- Hexagons have fixed centers on sphere
- Events map to hexagons using point-in-hexagon test

**New:**
- Hexagon centers rotate with viewport
- OR: Keep hexagons fixed, rotate viewport independently
- **Decision Needed**: Should hexagons rotate with viewport, or stay fixed?

### 4. Minimap

**Purpose:**
- Show current viewport orientation on full sphere
- Help player understand where they're looking
- Show hexagon layout on sphere

**Implementation:**
- Small UI element showing sphere projection
- Highlight current viewport FOV
- Show hexagon centers
- Update in real-time as viewport rotates

## Questions to Resolve

### 1. Hexagon Rotation Behavior

**Option A: Hexagons Rotate with Viewport**
- Hexagons are "attached" to viewport
- Always visible in center of screen
- Simpler: hexagons stay in same screen position
- **Issue**: Hexagons don't represent fixed regions of sphere

**Option B: Hexagons Stay Fixed, Viewport Rotates**
- Hexagons represent fixed regions of sphere
- Viewport rotates to see different regions
- More complex: hexagons move on screen as viewport rotates
- **Better**: Maintains hexagon = fixed sphere region

**Recommendation: Option B** - Hexagons stay fixed on sphere, viewport rotates

### 2. Event-to-Visual Mapping

**When to create visual objects?**
- All events get visuals? (may be too many)
- Only major events? (may be too few)
- Configurable per event type?

**Recommendation:** Start with major events only, expand later

### 3. Distance Ranges

**What distances for sprite vs 3D model?**
- Far threshold: `r > threshold` → sprite
- Close threshold: `r < threshold` → 3D model
- Smooth transition zone?

**Recommendation:** Start with simple threshold, refine later

## Compatibility Conclusion

✅ **Current system is compatible with future 3D visualization**

**Why:**
1. Gameplay layer is abstract (theta, phi only) - no changes needed
2. Visualization layer can add distance (r) independently
3. Viewport rotation sets foundation for 3D camera system
4. Events can have visuals without affecting gameplay

**What Changes:**
- **Phase 1 (Rotation)**: Extend viewport projection, add rotation controls
- **Phase 2 (3D)**: Add 3D rendering layer on top of gameplay layer
- **Gameplay**: Stays abstract, no changes needed

**What Stays the Same:**
- Event generation (theta, phi only)
- Hexagon mapping (angular-based)
- Signal calculation (sector-based)
- Python environment (no changes)

## Next Steps

1. **Implement sphere rotation** (Phase 1)
   - Extend `ViewportProjection` with rotation
   - Add rotation controls
   - Update hexagon mapping
   - Add minimap

2. **Plan 3D visualization** (Phase 2 - future)
   - Design visual object system
   - Plan distance-based rendering
   - Design camera system
   - Map events to visuals

3. **Keep gameplay abstract** (Always)
   - Events stay at (theta, phi)
   - Sectors stay angular-based
   - No distance in gameplay logic

