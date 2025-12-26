# Design Thoughts & Future Ideas

This document captures design ideas and considerations for future development. Items are tracked with checkboxes for discussion and implementation planning.

## Event Type Differentiation & Temporal Structure

- [x] **Event types with distinct characteristics**
  - Nebulas: Constant, observable every night, lower value, low risk
  - Comets: Cyclical and consistent, predictable timing
  - Supernovae: Random occurrence, very high value
  - Policy should react differently to each type
  - See README.md for full details

- [ ] **Shortened in-game year cycle**
  - ~20 nights/year (creates scarcity and makes each night more valuable)
  - Each episode represents one year of observations
  - Consider: Multi-year episodes or cycles spanning multiple episodes for pattern learning

## Sector Unlocking & Progression

- [ ] **Progressive hexagon unlocking**
  - Player starts with only 1 hexagon (sector) unlocked - the **center hexagon**
  - Must unlock additional hexagons through progression/upgrades
  - Creates meaningful progression and resource allocation decisions
  - **Layout:** 19 hexagons total (1 center + 6 in ring 1 + 12 in ring 2)
  - **Progression path:** Center → Ring 1 → Ring 2 (natural expansion outward)
  - **Considerations:**
    - How are hexagons unlocked? (Upgrades? Budget? Discoveries?)
    - Should unlocking be permanent or episode-based?
    - Does this affect agent training? (Variable action space?)
    - Starting with center hexagon makes intuitive sense (home base, focal point)

## 360-Degree Sphere & Field of View

- [ ] **Full celestial sphere observation**
  - 360-degree sphere (full sky coverage)
  - Player/agent must choose which portion of the sphere to focus on
  - **Considerations:**
    - How is the sphere divided? (Sectors? Grid? Hexagonal tiling?)
    - Does the current 18-hexagon layout represent a "field of view" window?
    - Can the field of view be rotated/changed during an episode?
    - How does this interact with upgrades (wider field of view)?
    - Does this create strategic decisions about which part of sky to observe?
    - How does this affect the agent's action space? (Sector selection + field of view orientation?)

## Tutorial & Onboarding

- [ ] **Manual control phase before AI introduction**
  - First part of the game: Player manually chooses focus/observations
  - Teaches player the core mechanics (sector selection, observation timing, tradeoffs)
  - Player learns the "feel" of the game before AI takes over
  - **Considerations:**
    - How long is the tutorial phase? (One episode? Multiple episodes?)
    - Does tutorial phase have simplified mechanics? (Fewer sectors? Clearer signals?)
    - When does AI get introduced? (After tutorial? Gradually?)
    - Does player retain some manual control after AI introduction? (Emergency override?)
    - How does this affect the core fantasy? (Transition from "I'm doing this" to "I'm directing an AI")
  - **Potential benefits:**
    - Player understands what the AI is doing (better appreciation of agent behavior)
    - Player learns the value of different decisions (better strategic planning later)
    - Creates emotional connection to the observatory before automation
    - Smooth onboarding without overwhelming complexity
  - **Potential challenges:**
    - Tutorial must be engaging, not just instructional
    - Need clear transition moment (when does AI take over?)
    - Player might prefer manual control (need to make AI feel valuable)
    - Tutorial design must teach without being boring

## Integration Questions

- [ ] **How do these ideas work together?**
  - If player starts with 1 hexagon, does that hexagon represent a "window" into the 360-degree sphere?
  - Unlocking hexagons = expanding field of view?
  - Does the 18-hexagon layout represent the maximum observable area at once?
  - Can the field of view be rotated to observe different parts of the sphere?
  - **Tutorial integration:**
    - Does tutorial start with 1 hexagon? (Simpler for learning)
    - Does tutorial teach manual focus selection before AI is introduced?
    - Does progressive unlocking happen during tutorial or after AI introduction?
    - Does 360-degree sphere become available after tutorial? (More complexity after basics learned)

## Implementation Considerations

- [ ] **Technical challenges:**
  - Variable action space (if hexagons unlock progressively)
  - Field of view rotation mechanics
  - Sphere projection and visualization in Unity
  - Agent training with evolving action space

- [ ] **Gameplay impact:**
  - Does progressive unlocking create satisfying progression?
  - Does 360-degree sphere add meaningful strategic depth?
  - How do these mechanics affect the core "deciding where to look" fantasy?
  - Do they enhance or complicate the POMDP learning problem?

## Notes

- These are exploratory ideas - not committed to implementation
- Consider how each idea serves the core game fantasy
- Balance complexity vs. gameplay depth
- Ensure mechanics support RL agent learning, not hinder it

