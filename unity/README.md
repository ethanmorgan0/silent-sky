# Unity Client for Silent Sky

## Setup

1. Open Unity Hub
2. **Open the Unity project from:** `unity/Silent Sky/` (this is the Unity project root)
3. **No packages required for Phase 1!** The project uses mock data mode by default.

## Phase 1: No External Packages Needed

The project is configured to work **without any external packages**:
- Uses **mock data mode** by default (no NetMQ needed)
- All scripts compile with Unity built-ins only
- Perfect for Phase 1 MVP development

## Phase 2: Optional Packages (when Unity Package Manager is stable)

If you want to connect to Python backend later:
- **NetMQ**: ZeroMQ library (requires Git installed)
- **Newtonsoft.Json**: JSON serialization (or use Unity's JsonUtility)

## Scene Setup

1. Create a new scene: `ObservatoryScene`
2. Add the following GameObjects:
   - `ZMQBridge` (with `ZMQBridge.cs` component)
   - `SectorMap` (with `SectorMap.cs` component)
   - `EventVisualizer` (with `EventVisualizer.cs` component)
   - `AgentStateUI` (with `AgentStateUI.cs` component)
   - `BudgetUI` (with `BudgetUI.cs` component)
   - `MissionDirectives` (with `MissionDirectives.cs` component)
   - `UpgradeShop` (with `UpgradeShop.cs` component)

## Running

1. Start Python backend: `python python/run_episode.py --unity`
2. Press Play in Unity
3. The Unity client will connect to Python and display state updates

## Mock Data Mode

Set `useMockData = true` in `ZMQBridge` component to test without Python backend.

