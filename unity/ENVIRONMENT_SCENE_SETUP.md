# Environment Scene Setup Guide

This guide explains how to set up the new `EnvironmentScene.unity` for rendering the starfield and signal visualization.

## Scene Creation

1. **Create New Scene:**
   - File → New Scene
   - Choose "Basic (Built-in)" or "2D" template
   - Save as `Assets/Scenes/EnvironmentScene.unity`

2. **Camera Setup:**
   - Select Main Camera
   - Set Background Color to black (0, 0, 0)
   - For 2D: Set Projection to Orthographic, Size = 10
   - For 3D: Set Projection to Perspective, Field of View = 60

3. **Lighting (Optional):**
   - Remove or disable Directional Light (we want dark space)
   - Or set ambient light to very dark

## GameObject Setup

### 1. Starfield Background

1. **Create as UI element on Canvas (BEFORE SectorContainer):**
   - Right-click Canvas → Create Empty
   - Name: `StarfieldBackground`
   - **Important:** 
     - Make it a child of Canvas (so it moves with UI)
     - Place it ABOVE SectorContainer in Hierarchy (renders behind)
   - Add Component: `StarfieldBackground` (from `SilentSky.Unity.Environment`)
   
2. **Configure in Inspector:**
   - Star Count: 1000
   - Star Size: 0.1
   - Star Brightness: 1.0
   - Use UI: ✓ (checked) - This ensures alignment with hexagons
   - Auto Find Container: ✓ (checked) - Will find SectorContainer automatically
   - Target Container: (leave empty if auto-find is on, or drag SectorContainer here)
   
3. **Positioning:**
   - The starfield will automatically align with the SectorContainer in LateUpdate()
   - It will move and resize with the hexagons
   - Stars fill the same area as the hexagon container
   - **Note:** Starfield must be created BEFORE SectorContainer in Hierarchy to render behind

### 2. Fake Data Generator

1. Create empty GameObject: `FakeDataGenerator`
2. Add Component: `FakeDataGenerator` (from `SilentSky.Unity.Environment`)
3. Configure in Inspector:
   - Event Count: 50
   - Min Value: 10
   - Max Value: 100
   - Min Duration: 5
   - Max Duration: 20
   - Seed: 42 (for deterministic generation)

### 3. Signal Calculator

1. Create empty GameObject: `SignalCalculator`
2. Add Component: `SignalCalculator` (from `SilentSky.Unity.Environment`)
3. In Inspector, drag `FakeDataGenerator` to the "Data Generator" field
4. Segments will be auto-generated (19 segments)

### 4. Signal Visualizer

1. Create empty GameObject: `SignalVisualizer`
2. Add Component: `SignalVisualizer` (from `SilentSky.Unity.Environment`)
3. In Inspector:
   - Drag `SignalCalculator` to "Signal Calculator" field
   - Drag the `SectorMap` GameObject (that you created in section 5 below) to "Sector Map" field
   - Min Signal: 0
   - Max Signal: 500
   - Min Signal Color: Dark (0.1, 0.1, 0.1)
   - Max Signal Color: Bright Yellow (1, 1, 0)
   - Hexagon Opacity: 0.7 (adjust to make hexagons more/less transparent - stars show through)

### 4b. Event Visualizer (Optional but Recommended)

1. Create empty GameObject: `EventVisualizer`
2. Add Component: `EventVisualizer` (from `SilentSky.Unity.Environment`)
3. In Inspector:
   - Drag `SignalCalculator` to "Signal Calculator" field (auto-finds if not set)
   - Drag `FakeDataGenerator` to "Data Generator" field (auto-finds if not set)
   - Drag `StarfieldBackground` GameObject to "Starfield Background" field (auto-finds if not set)
   - Event Star Size: 0.5 (larger than regular stars for visibility)
   - Event Star Color: Yellow (bright color to distinguish from regular stars)
   - Event Star Brightness: 1.5 (extra bright multiplier)

**Note:** Events will appear as bright yellow stars at their (theta, phi) positions, allowing you to verify event generation and segment mapping.

### 4c. Viewport Rotation Controller

1. Create empty GameObject: `ViewportRotationController`
2. Add Component: `ViewportRotationController` (from `SilentSky.Unity.Environment`)
3. In Inspector:
   - Rotation Speed: 0.5 (radians per second - adjust for desired rotation speed)
   - Smooth Rotation: ✓ (checked) - Enables smooth interpolation
   - Smooth Damping: 5.0 (higher = faster interpolation)
   - Min Phi Offset: -1.047 (≈ -60°, prevents viewing poles)
   - Max Phi Offset: 1.047 (≈ +60°, prevents viewing poles)
   - Rotate Left Key: Left Arrow (default)
   - Rotate Right Key: Right Arrow (default)
   - Rotate Up Key: Up Arrow (default)
   - Rotate Down Key: Down Arrow (default)
   - Use WASD: ✓ (checked) - Enables WASD as alternative controls

**Controls:**
- **Arrow Keys**: Rotate viewport around sphere
- **WASD**: Alternative rotation controls (W=up, S=down, A=left, D=right)
- Viewport rotation affects what part of the sphere is visible
- Hexagons stay fixed on sphere, but move on screen as viewport rotates

### 4d. Sphere Minimap (Optional but Recommended)

1. Create empty GameObject: `SphereMinimap`
2. Add Component: `SphereMinimap` (from `SilentSky.Unity.Visualization`)
3. In Inspector:
   - Drag `ViewportRotationController` to "Rotation Controller" field (auto-finds if not set)
   - **Minimap Container**: Leave empty - will be auto-created if not assigned
   - Minimap Size: (200, 150) - Size of minimap in pixels
   - Minimap Position: (-10, -10) - Position offset from bottom-right corner
   - Background Color: Black with 70% opacity (0, 0, 0, 0.7)
   - Viewport FOV Color: Yellow with 30% opacity (1, 1, 0, 0.3)
   - Viewport FOV Border Color: Yellow with 80% opacity (1, 1, 0, 0.8)
   - Show Grid: ✓ (checked) - Shows coordinate grid on minimap

**Note:** 
- The minimap container is **automatically created** if not assigned - you don't need to create it manually
- The minimap shows the full sphere with the current viewport FOV highlighted
- It updates in real-time as you rotate the viewport
- The minimap will be positioned in the bottom-right corner of the Canvas

### 5. UI Setup (SectorMap)

1. Create Canvas (if not exists):
   - Right-click Hierarchy → UI → Canvas
   - Set Canvas to Screen Space - Overlay

2. Create SectorContainer:
   - Right-click Canvas → Create Empty
   - Name: `SectorContainer`
   - Add RectTransform (automatic)
   - **IMPORTANT:** Set anchors to center (0.5, 0.5) in RectTransform
   - Set pivot to center (0.5, 0.5)
   - Set anchoredPosition to (0, 0) to center it
   - Set sizeDelta to a reasonable size (e.g., 1000x1000) or let it size based on content

3. Create SectorMap GameObject:
   - Right-click Hierarchy → Create Empty
   - Name: `SectorMap`
   - Add Component: `SectorMap` (from `SilentSky.Unity.Visualization`)
   - In Inspector:
     - Drag `SectorContainer` to "Sector Container"
     - Drag `SectorPrefab` (from existing scene) to "Sector Prefab"
     - Hex Size: 80
     - Use Hexagonal Layout: ✓

4. Ensure SectorPrefab exists:
   - Copy from `ObservatoryScene.unity` if needed
   - Or create new: UI → Image, add Text child, add `SectorDisplay` component

## Testing

1. Press Play
2. You should see:
   - Black background with stars
   - 19 hexagons in JWST pattern
   - Hexagons colored based on signal values (sum of events in each segment)
   - Signal values displayed as text in hexagons
   - Minimap in bottom-right corner (if SphereMinimap is added)

3. **Test Rotation:**
   - Use Arrow Keys or WASD to rotate the viewport
   - Observe that events and hexagons move on screen as viewport rotates
   - Check minimap to see viewport FOV highlighted on full sphere
   - Verify that signals still calculate correctly during rotation

## Troubleshooting

### No stars visible
- Check StarfieldBackground component is on a GameObject
- Check camera can see the stars (adjust position/rotation)
- Check particle system is rendering (look in Scene view)

### Hexagons not showing signals
- Verify SignalCalculator has FakeDataGenerator assigned
- Verify SignalVisualizer has both SignalCalculator and SectorMap assigned
- Check Console for errors
- Verify segments are created (19 segments)

### Signals always zero
- Check FakeDataGenerator is generating events
- Check events are active at current time
- Verify event positions map to segments correctly

## Next Steps

Once this works, you can:
- Adjust starfield parameters for better visuals
- Tune signal color mapping
- Add time controls to see signals change over time
- Refine segment boundaries for better distribution
- Adjust rotation speed and smoothness in ViewportRotationController
- Customize minimap appearance and position
- Add click-to-jump functionality to minimap (future enhancement)

## Rotation Controls Reference

- **Left Arrow / A**: Rotate viewport left (decrease theta)
- **Right Arrow / D**: Rotate viewport right (increase theta)
- **Up Arrow / W**: Rotate viewport up (increase phi)
- **Down Arrow / S**: Rotate viewport down (decrease phi)

**Note:** Rotation is clamped to prevent viewing poles directly. The viewport FOV is 180° horizontal × 120° vertical, centered on the current viewport orientation.

### 4e. Debug Tools (Optional - For Development)

#### Hexagon Mapping Debugger

1. Create empty GameObject: `HexagonMappingDebugger`
2. Add Component: `HexagonMappingDebugger` (from `SilentSky.Unity.Environment`)
3. In Inspector:
   - **Sector Container**: Leave empty - auto-finds if not set
   - **Signal Calculator**: Leave empty - auto-finds if not set
   - **Data Generator**: Leave empty - auto-finds if not set
   - Show Hexagon Bounds: ✓ (checked) - Shows hexagon boundaries
   - Show Hexagon Centers: ✓ (checked) - Shows hexagon center points
   - Show Event Positions: ✓ (checked) - Shows event positions and mapping
   - Show Coordinate Grid: (unchecked by default) - Shows viewport coordinate grid

**Note:** 
- The debug container is **automatically created** as a child of SectorContainer
- No manual UI setup required
- Visualizes hexagon boundaries, centers, and event-to-hexagon mappings
- Useful for debugging event mapping issues

#### Spherical Coordinate Debugger

1. Create empty GameObject: `SphericalCoordinateDebugger`
2. Add Component: `SphericalCoordinateDebugger` (from `SilentSky.Unity.Environment`)
3. In Inspector:
   - **Sector Container**: Leave empty - auto-finds if not set
   - **Rotation Controller**: Leave empty - auto-finds if not set
   - Show Theta Lines: ✓ (checked) - Shows lines of constant theta (cyan)
   - Show Phi Lines: ✓ (checked) - Shows lines of constant phi (magenta)
   - Show Labels: ✓ (checked) - Shows theta/phi values in degrees
   - Theta Divisions: 8 - Number of theta lines (0 to 360°)
   - Phi Divisions: 6 - Number of phi lines (0 to 180°)
   - Line Width: 1.0 - Thickness of grid lines
   - Font Size: 10 - Size of coordinate labels

**Note:**
- The debug container is **automatically created** as a child of SectorContainer
- No manual UI setup required
- Visualizes how spherical coordinates (theta, phi) map to viewport positions
- Updates in real-time as viewport rotates
- Useful for understanding coordinate system and debugging projection issues

