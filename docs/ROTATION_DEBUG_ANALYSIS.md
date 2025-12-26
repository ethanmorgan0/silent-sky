# Rotation Boundary Jumping - Full End-to-End Analysis

## Problem Statement
1. **Theta (horizontal)**: Still jumps across to the other side when crossing 0/2π boundary
2. **Phi (vertical)**: Now fully bounded and cannot be increased above 180° (user wants this fixed)

## Complete Data Flow Analysis

### Step 1: User Input (HandleInput)
**Location**: `ViewportRotationController.HandleInput()`
- User presses right arrow → `thetaDelta = +0.01` (example, depends on rotationSpeed)
- `targetThetaOffset += thetaDelta` (line 88)
- **State**: `targetThetaOffset` is now unbounded (e.g., 6.28 → 6.29)

**Key observation**: `targetThetaOffset` accumulates unbounded. No normalization here.

### Step 2: Rotation Update (UpdateRotation)
**Location**: `ViewportRotationController.UpdateRotation()`

#### For Theta:
**Current implementation (lines 128-155)**:
1. Normalize both current and target: `normalizedCurrent = NormalizeTheta(6.27) = 0.13`, `normalizedTarget = NormalizeTheta(6.29) = 0.15`
2. Interpolate: `interpolated = LerpAngle(0.13, 0.15, ...) = 0.14`
3. Compute delta: `delta = 0.14 - 0.13 = 0.01`
4. Handle wrapping: delta is small, no wrapping needed
5. Apply: `currentThetaOffset += 0.01 = 6.28`

**This should work correctly** - but let's trace what happens at the boundary...

#### Critical Boundary Case - Theta Crossing 2π:
**Scenario**: `currentThetaOffset = 6.27`, `targetThetaOffset = 6.29` (just crossed 2π)

1. `normalizedCurrent = NormalizeTheta(6.27) = 6.27 - 2π = 0.13`
2. `normalizedTarget = NormalizeTheta(6.29) = 6.29 - 2π = 0.15`
3. `interpolated = LerpAngle(0.13, 0.15, ...) = 0.14`
4. `delta = 0.14 - 0.13 = 0.01`
5. `currentThetaOffset += 0.01 = 6.28`

**This looks correct!** But wait...

#### The Real Problem - When Both Are On Opposite Sides:
**Scenario**: `currentThetaOffset = 6.27` (normalized = 0.13), but `targetThetaOffset = 0.15` (already normalized from previous frame)

Wait, that shouldn't happen if we're keeping them unbounded. Let me think...

**Actually, the issue might be in the periodic normalization (lines 177-182)**:
- If `currentThetaOffset = 20.1π` and `targetThetaOffset = 20.2π`
- We subtract `20π`, getting `0.1π` and `0.2π`
- But if we're in the middle of interpolation, this causes a visual jump

**But the user says it jumps at 0/2π, not at large values.**

#### Alternative Hypothesis - GetViewportCenter Normalization:
**Location**: `ViewportProjection.GetViewportCenter()` (line 129)
- `normalizedThetaOffset = NormalizeTheta(thetaOffset)`
- If `thetaOffset = 6.27`, normalized = `0.13`
- If `thetaOffset = 6.29`, normalized = `0.15`
- `centerTheta = NormalizeTheta(0 + 0.13) = 0.13`
- `centerTheta = NormalizeTheta(0 + 0.15) = 0.15`

**This should be continuous** - small changes in thetaOffset produce small changes in centerTheta.

#### The Actual Problem - ProjectToViewport:
**Location**: `ViewportProjection.ProjectToViewport()` (lines 55-62)

When projecting a point `theta = 0.5` (example):
- `centerTheta = 0.13` (from GetViewportCenter)
- `rawDelta = 0.5 - 0.13 = 0.37`
- Normalize: `deltaTheta = 0.37` (within [-π, π])
- `x = 0.5 + (0.37 / π) = 0.5 + 0.118 = 0.618`

**But what if centerTheta jumps?**
- Frame 1: `centerTheta = 0.13`, point at `theta = 0.5` → `x = 0.618`
- Frame 2: `centerTheta = 0.15`, point at `theta = 0.5` → `x = 0.5 + (0.35 / π) = 0.611`

This is a small change, not a jump.

**Unless...** what if `centerTheta` jumps from `0.1` to `6.2` (normalized = 0.1)? That would cause a huge jump!

**Root cause identified**: If `thetaOffset` is unbounded and we normalize it in `GetViewportCenter`, but the normalization happens at a different time than when we set it, there could be a frame where:
- `thetaOffset = 6.27` (unbounded)
- Normalize: `0.13`
- Next frame: `thetaOffset = 6.29` (unbounded)  
- Normalize: `0.15`

But wait, that's still continuous. Unless...

**The real issue**: When `thetaOffset` crosses a 2π boundary in the unbounded space, the normalized value jumps. For example:
- `thetaOffset = 6.27` → normalized = `0.13`
- `thetaOffset = 6.29` → normalized = `0.15`
- But if we're interpolating and `thetaOffset` goes from `6.27` to `6.29`, the normalized value goes from `0.13` to `0.15`, which is correct.

**Unless the problem is that `centerTheta` is computed from `DEFAULT_CENTER_THETA + normalizedThetaOffset`**:
- `DEFAULT_CENTER_THETA = 0`
- `normalizedThetaOffset = 0.13`
- `centerTheta = NormalizeTheta(0 + 0.13) = 0.13` ✓

This should be fine.

### Step 3: Visual Update
**Location**: Components listening to `OnRotationChanged` (StarfieldBackground, EventVisualizer)

When `OnRotationChanged` fires with `(currentThetaOffset, currentPhiOffset)`:
- Components call `ViewportProjection.ProjectToViewport()` for each star/event
- This uses the current `thetaOffset` stored in `ViewportProjection`

**Potential issue**: If `ViewportProjection.thetaOffset` is updated at a different time than when `OnRotationChanged` fires, there could be a mismatch.

**Actually, looking at line 185**: `ViewportProjection.SetViewportRotation(currentThetaOffset, currentPhiOffset)` is called BEFORE `OnRotationChanged?.Invoke()`, so they should be in sync.

## The Real Root Cause

After tracing through the entire flow, I believe the issue is:

**The periodic normalization (lines 177-182) can cause a jump**:
- When `currentThetaOffset > 20π`, we subtract a multiple of 2π
- This happens DURING interpolation, not at a clean boundary
- If we're interpolating from `20.1π` to `20.2π`, and suddenly both become `0.1π` and `0.2π`, the visual position jumps

**But the user says it happens at 0/2π, not at large values.**

**Alternative hypothesis**: The issue is in how we compute the delta. When `currentThetaOffset` and `targetThetaOffset` are both large but close (e.g., 6.27 and 6.29), normalizing them gives 0.13 and 0.15. The delta is 0.02, which is correct. But if there's any floating point error or timing issue, the delta could be computed incorrectly.

**Actually, I think I see it now**: The problem is that `LerpAngle` returns a value that might wrap. If `normalizedCurrent = 0.1` and `normalizedTarget = 6.2` (normalized = 0.1), `LerpAngle` might return something near 0.1, but the delta calculation `interpolated - normalizedCurrent` could be wrong if `LerpAngle` wrapped.

Wait, `LerpAngle` handles wrapping internally, so it should return the shortest path. But then `interpolated - normalizedCurrent` might be large if we wrapped the long way.

**The actual bug**: When computing `delta = interpolated - normalizedCurrent`, if `LerpAngle` wrapped (e.g., went from 0.1 to 6.2, which is the same as 0.1, but `LerpAngle` might interpolate through the long path), the delta could be ~6.1, which we then try to fix with the wrapping check. But this is fragile.

## For Phi Issue

**Current behavior**: Phi is clamped to prevent going above 180° (lines 113-126)
- When `targetCenterPhi > π - POLE_THRESHOLD`, we clamp it and redirect to theta
- This prevents the direction reversal, but also prevents going above 180°

**User wants**: To be able to go above 180° (continue rotating)

**The fundamental issue**: Phi = π is the south pole. You can't go "past" it in spherical coordinates. To continue rotating, you need to:
1. Rotate around the pole (change theta)
2. Or use a different coordinate system (quaternion)

## Summary of Root Causes

### Theta Jumping:
1. **Primary suspect**: The delta calculation in `UpdateRotation` when `LerpAngle` wraps
2. **Secondary suspect**: Periodic normalization causing jumps during interpolation
3. **Tertiary suspect**: Timing issue between `SetViewportRotation` and `OnRotationChanged`

### Phi Bounded:
1. **Root cause**: Clamping prevents going above 180°
2. **Solution needed**: Implement proper pole rotation (rotate around pole by changing theta)

## Next Steps (Analysis Only - No Code)

1. Add debug logging to trace exact values when jump occurs:
   - Log `currentThetaOffset`, `targetThetaOffset`, `normalizedCurrent`, `normalizedTarget`, `interpolated`, `delta`, `centerTheta` at each frame
   - Identify the exact frame where the jump occurs
   - See what values change discontinuously

2. Test hypothesis about `LerpAngle` wrapping:
   - Check if `interpolated - normalizedCurrent` is ever > π or < -π
   - Verify the wrapping fix (lines 140-141) is working correctly

3. Test periodic normalization:
   - Check if jumps occur when `currentThetaOffset` is near `20π`
   - Verify the normalization preserves visual continuity

4. For phi:
   - Understand user's expectation: do they want to rotate past the pole, or just not have direction reverse?
   - If past pole: implement pole rotation (change theta when at pole)
   - If just no reversal: current clamping might be acceptable, but needs refinement
