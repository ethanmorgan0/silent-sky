# Unity Asset Setup Guide

This guide explains how to set up the Unity assets (prefabs, UI, scene) for Silent Sky.

## Required Scene Setup

### 1. Canvas Setup
Your scene needs a **Canvas** for UI elements:

1. Right-click in Hierarchy → **UI → Canvas**
2. Set Canvas to **Screen Space - Overlay** (default)
3. This will auto-create an **EventSystem** (needed for UI interaction)

### 2. Sector Container
Create an empty GameObject to hold the sectors:

1. Right-click in Hierarchy → **Create Empty**
2. Name it `SectorContainer`
3. Add it as a **child of the Canvas** (drag it under Canvas in Hierarchy)
4. Add a **RectTransform** component (should be automatic)
5. Set RectTransform anchors to **stretch-stretch** (click the anchor preset in top-left of RectTransform)
6. Set all margins to 0

### 3. Sector Prefab Setup
Create a prefab for individual sectors (hexagonal shape, JWST-style):

#### Option A: Simple Setup (Recommended for MVP)
1. Right-click in Hierarchy → **UI → Image**
2. Name it `SectorPrefab`
3. Set Image color to white (or any default color)
4. Set RectTransform size to **100x100** (or your preferred size)
   - Note: The hexagon sprite will be generated automatically by `HexagonSpriteGenerator`
   - The Image component's sprite will be set programmatically to a hexagon shape
5. Add a **Text** component as a child:
   - Right-click `SectorPrefab` → **UI → Text - TextMeshPro** (or **UI → Legacy → Text**)
   - Name it `LabelText`
   - Position it at top of the image
   - Set text to "Sector 0" (placeholder)
   - Center align the text
6. Add another **Text** component for readings:
   - Right-click `SectorPrefab` → **UI → Text - TextMeshPro** (or **UI → Legacy → Text**)
   - Name it `ReadingText`
   - Position it at center/bottom of the image
   - Set text to "0.00" (placeholder)
   - Center align the text
7. Add the `SectorDisplay` component:
   - Select `SectorPrefab`
   - Add Component → Search for `SectorDisplay`
   - The component will auto-find the Image and Text components
8. **Create Prefab:**
   - Drag `SectorPrefab` from Hierarchy to `Assets/Prefabs/` folder (create folder if needed)
   - Delete the instance from the scene (we'll instantiate it via code)

#### Option B: Manual Assignment (If auto-find doesn't work)
1. Follow steps 1-7 above
2. In the `SectorDisplay` component Inspector:
   - Drag `SectorPrefab`'s Image component to `Sector Image` field
   - Drag `LabelText` to `Sector Label` field
   - Drag `ReadingText` to `Reading Text` field

### 4. Scene GameObject Setup

#### ZMQBridge GameObject
1. Right-click in Hierarchy → **Create Empty**
2. Name it `ZMQBridge`
3. Add Component → `ZMQBridge` (from `SilentSky.Unity.Bridge` namespace)
4. In Inspector:
   - ✅ Check `Use Mock Data` (for Phase 1)
   - Set `Mock Update Interval` to `0.1` (10 updates per second)

#### SectorMap GameObject
1. Right-click in Hierarchy → **Create Empty**
2. Name it `SectorMap`
3. Add Component → `SectorMap` (from `SilentSky.Unity.Visualization` namespace)
4. In Inspector:
   - Drag `SectorContainer` (from step 2) to `Sector Container` field
   - Drag `SectorPrefab` (the prefab you created) to `Sector Prefab` field
   - Set `Hex Size` to `80` (size of each hexagon, adjust for your canvas)
   - ✅ Check `Use Hexagonal Layout` (default, creates JWST-style honeycomb pattern)

## Quick Setup Checklist

- [ ] Canvas exists in scene
- [ ] EventSystem exists (auto-created with Canvas)
- [ ] SectorContainer exists as child of Canvas
- [ ] SectorPrefab created with:
  - [ ] Image component
  - [ ] LabelText (Text component)
  - [ ] ReadingText (Text component)
  - [ ] SectorDisplay component
- [ ] SectorPrefab saved as prefab in Assets folder
- [ ] ZMQBridge GameObject with ZMQBridge component
- [ ] SectorMap GameObject with SectorMap component
- [ ] SectorMap references assigned (Container and Prefab)

## Testing

1. Press Play
2. Check Console for:
   - "ZMQBridge: GenerateMockData() started"
   - "ZMQBridge: Generating mock state step X"
   - No errors about missing components
3. You should see:
   - 18 hexagonal sectors arranged in a JWST-style honeycomb pattern
   - Sectors changing color (green = high confidence, yellow = ambiguous, dark = low info)
   - Numbers updating in the reading text (likelihood percentages)
   - Cyan border on the currently observed sector

## Troubleshooting

### "SectorMap: Missing prefab or container"
- Make sure SectorContainer and SectorPrefab are assigned in SectorMap Inspector

### Sectors not updating
- Check Console for ZMQBridge logs
- Verify ZMQBridge GameObject is in scene
- Check that SectorMap subscribed to ZMQBridge events (should happen in Start())

### "New Text" showing instead of data
- The prefab's Text components weren't found
- Check that Text components are children of the Image GameObject
- Verify SectorDisplay component is on the prefab root

### Sectors not visible
- Check Canvas is set to Screen Space - Overlay
- Verify SectorContainer is child of Canvas
- Check RectTransform settings on SectorContainer

## Next Steps

Once this works, you can add:
- BudgetUI component
- EventVisualizer component
- MissionDirectives component
- UpgradeShop component

Each follows similar patterns - create UI elements, add the component, assign references.

