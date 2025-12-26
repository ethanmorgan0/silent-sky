# Coordinate System & Event Mapping Analysis

## Current Implementation (Hybrid/Inconsistent Approach)

### What We're Doing:
1. **Event Generation**: Events created with random spherical coordinates (theta, phi)
2. **Visual Positioning**: Events positioned using **equirectangular projection**
   - `x = theta / (2π)` (maps 0-2π → 0-1)
   - `y = 1 - (phi / π)` (maps 0-π → 1-0, inverted)
3. **Signal Calculation**: Events mapped to segments using `SphereSegment.FindSegmentForPoint(theta, phi)`
   - Segments defined by arbitrary phi/theta boundaries (center band, ring 1, ring 2)
   - Segments don't correspond to hexagon positions
4. **Hexagon Layout**: Hexagons arranged in hexagonal grid (JWST pattern)
   - Positioned using axial coordinates (q, r)
   - No mathematical relationship to sphere segments

### The Problem:
- **Visual positioning** (equirectangular) ≠ **Signal calculation** (arbitrary segment boundaries) ≠ **Hexagon layout** (hexagonal grid)
- An event visually positioned in one hexagon may have its signal counted in a different segment
- Example: Event at (theta, phi) appears in Sector 15 visually, but `SphereSegment.FindSegmentForPoint` maps it to a different segment (maybe Sector 10)

### Pros:
- ✅ Simple to implement
- ✅ Fast (no 3D rendering)
- ✅ Events appear uniformly across sky

### Cons:
- ❌ **Inconsistent**: Three different coordinate systems that don't align
- ❌ **Confusing**: Visual doesn't match signal calculation
- ❌ **Hard to debug**: Mismatches between what you see and what's calculated
- ❌ **Not principled**: No clear mathematical relationship between systems

---

## Approach 1: Abstract Sphere with Direct Math Mapping

### Concept:
- Define sphere abstractly (just coordinates, no rendering)
- Generate events at spherical coordinates (theta, phi)
- **Direct mathematical mapping**: `(theta, phi) → hexagon_index`
- Hexagons represent fixed regions of the sphere
- Visual positioning matches signal calculation exactly

### Implementation Strategy (Simplified):

**Chosen Approach: Distance-Based Mapping**
- Project hexagon grid positions to sphere using equirectangular projection
- Each hexagon gets a "center" on the sphere (theta_center, phi_center)
- For each event, find nearest hexagon center using great-circle distance
- This creates Voronoi-like regions on the sphere

**Why This:**
- ✅ Simple: Just distance calculation
- ✅ Predictable: Hexagons map to adjacent regions on sphere
- ✅ Extensible: Can add movement/sweeping later without changing core mapping
- ✅ Consistent: One function for both signal and visual

### Visual Positioning:
- Events positioned uniformly using equirectangular projection (independent of hexagons)
- Events are "facts of the matter" - they exist on the sphere regardless of observers
- Hexagons are "observers" that see different regions

### Signal Calculation:
- Use same mapping function: `GetHexagonForEvent(theta, phi)`
- Sum event values per hexagon
- Ensures visual matches calculation

### Pros:
- ✅ **Consistent**: One mapping function for both signal and visual
- ✅ **Deterministic**: Same event always maps to same hexagon
- ✅ **Fast**: Pure math, no 3D rendering
- ✅ **Debuggable**: Clear relationship between coordinates and hexagons
- ✅ **Simple**: Can implement quickly, add complexity later
- ✅ **Extensible**: Easy to add movement/sweeping later

### Cons:
- ❌ Equirectangular projection distorts areas (poles compressed)
- ❌ Hexagon regions on sphere won't have equal areas
- ❌ But: This is acceptable for now, can refine later

### Design Constraints:
- Must ensure every (theta, phi) maps to exactly one hexagon
- Should handle edge cases (poles, wrap-around at theta=0/2π)
- Hexagon boundaries should be continuous (no gaps or overlaps)
- Hexagons should map to adjacent regions matching their UI layout

---

## Approach 2: 3D Sphere with Frustum Projection

### Concept:
- Create actual 3D sphere mesh
- Position events as 3D points on sphere surface
- Each hexagon represents a camera frustum viewing a portion of the sphere
- Project frustum view to 2D for display
- Signal = sum of events visible in that frustum

### Pros:
- ✅ **Physically accurate**: Represents actual telescope/observatory behavior
- ✅ **Intuitive**: Hexagons = field of view regions
- ✅ **Consistent**: Visual and signal use same frustum
- ✅ **Realistic**: Can simulate actual observation constraints

### Cons:
- ❌ **Complex**: Requires 3D rendering, camera setup, frustum calculations
- ❌ **Performance**: More expensive (multiple cameras, 3D math)
- ❌ **Overkill**: May be more than needed for this use case
- ❌ **UI complexity**: Need to render 3D sphere in 2D UI context

### Design Constraints:
- Must position 19 cameras to cover entire sphere without gaps
- Camera frustums should align with hexagon layout
- Need to project 3D sphere to 2D for UI display
- Performance: 19 cameras rendering simultaneously

---

## Decision: Approach 1 with Distance-Based Mapping

### Implementation Plan:

1. **Create `HexagonSphereMapper` class**:
   - Projects hexagon grid positions to sphere
   - Calculates hexagon centers on sphere (theta, phi)
   - Implements `GetHexagonForEvent(theta, phi)` using great-circle distance

2. **Update `SignalCalculator`**:
   - Replace `SphereSegment.FindSegmentForPoint` with `HexagonSphereMapper.GetHexagonForEvent`
   - Remove dependency on `SphereSegment` arbitrary boundaries

3. **Keep `EventVisualizer`**:
   - Continue using equirectangular projection for uniform visual positioning
   - Events are independent of hexagons (facts of the matter)

4. **Future Extensibility**:
   - Can add hexagon movement/sweeping by updating hexagon centers dynamically
   - Can refine area calculations later if needed
   - Can switch to different projection if equirectangular becomes limiting

### Why Equirectangular for Visual:
- **Uniform distribution**: Events appear evenly across the sky
- **Simple**: Standard projection, easy to understand
- **Independent**: Events exist on sphere regardless of hexagon observers
- **Future-proof**: Can change projection later without breaking mapping logic

The key insight: **Equirectangular is fine for visual positioning** (showing where events are), but we need a **consistent mapping function** for signal calculation (which hexagon "sees" each event).
