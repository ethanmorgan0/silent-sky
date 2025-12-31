# Silent Sky Documentation

This directory contains authoritative documentation for the Silent Sky project.

## Key Documents

### Architecture & Status

- **`ARCHITECTURE.md`** - Current architecture (Unity ML-Agents)
  - System components and design decisions
  - Current implementation status
  - Migration path from Python-authoritative to Unity ML-Agents

- **`CURRENT_STATUS.md`** - Implementation status
  - Completed components ✅
  - In progress / known issues 🔧
  - Not yet implemented ⏳
  - Deprecated/legacy code

### Planning & Roadmap

- **`NEXT_STEPS.md`** - Development roadmap
  - Recommended next steps
  - Priority order
  - Estimated effort

- **`20251223_IMPLEMENTATION_PLAN.md`** - Original implementation plan
  - **Note:** Contains deprecated Python-authoritative architecture
  - Kept for reference, but architecture has changed
  - See `ARCHITECTURE.md` for current architecture

### Design & Analysis

- **`SPHERE_ROTATION_DESIGN.md`** - Sphere rotation design decisions
  - Layered architecture (gameplay vs visualization)
  - Future 3D visualization compatibility
  - Rotation behavior and controls

- **`ROTATION_DEBUG_ANALYSIS.md`** - Rotation bug analysis
  - Theta jumping at 0/2π boundary
  - Phi clamping issues
  - Debugging approach

- **`COORDINATE_SYSTEM_ANALYSIS.md`** - Coordinate system details
  - Spherical coordinates (theta, phi)
  - Two-step mapping (Sphere → Viewport → Hexagon)
  - Hexagon layout (JWST pattern)

- **`DESIGN_THOUGHTS.md`** - Design ideas and future considerations
  - Event type differentiation
  - Temporal structure
  - Progressive unlocking
  - Tutorial/onboarding

## Architecture Evolution

The project architecture has evolved:

1. **Original Plan** (20251223_IMPLEMENTATION_PLAN.md):
   - Python authoritative (Gymnasium)
   - Unity visualization only
   - ZeroMQ communication

2. **Current Architecture** (ARCHITECTURE.md):
   - Unity authoritative (ML-Agents)
   - Python training only
   - ML-Agents communication

**Key Change:** Moved to Unity ML-Agents to eliminate sync issues and leverage ML-Agents' built-in communication.

## Quick Reference

- **Current architecture:** Unity ML-Agents (see `ARCHITECTURE.md`)
- **Current status:** Foundation complete, ML-Agents integration next (see `CURRENT_STATUS.md`)
- **Next steps:** Fix rotation bugs, event type differentiation, ML-Agents integration (see `NEXT_STEPS.md`)
- **Coordinate system:** Spherical (theta, phi) with two-step mapping (see `COORDINATE_SYSTEM_ANALYSIS.md`)

## For AI Agents

When working on this project, refer to:
1. `ARCHITECTURE.md` for current architecture
2. `CURRENT_STATUS.md` for what's done and what's not
3. `NEXT_STEPS.md` for recommended priorities
4. Plan files in `C:\Users\eam\.cursor\plans\` for detailed implementation plans

**Important:** The architecture is Unity ML-Agents, not Python-authoritative. All world model logic should be in Unity C#.

