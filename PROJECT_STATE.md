# Project State – SpaceVoxel Alpha Slice

## Core Systems Implemented
- First-person camera movement (WASD + mouse)
- AR-style overlay toggled with B
- Horizontal selector UI
- ScriptableObject-driven component data model
- Prototype component spawning (fuel + nozzle blocks)

## Folder Layout
Assets/
- Scripts/
  - Data/
    - ComponentDefinitionSO.cs
    - ComponentCategorySO.cs
  - Gameplay/
    - PrototypeComponentSpawner.cs
    - SimpleFPController.cs
  - UI/
    - ComponentSelectorUI_SO.cs
- Prefabs/
  - Components/
    - FuelBlock.prefab
    - NozzleBlock.prefab
  - UI/
    - SelectorTile.prefab
- Data/
  - Components/
    - Categories/
      - Cat_Propulsion.asset
    - Definitions/
      - Comp_SolidState.asset

## Current Behavior
- Press B opens selector
- Auto-drills into Propulsion
- Clicking Solid State spawns two transparent blocks in front of player
- No placement or wiggle yet

## Known Next Steps
- Ghost placement + wiggle
- Hull generation around fuel/nozzle
- Animated selector transitions

## Scene Hierarchy (Main.unity)

- Player
  - Main Camera
- Ground
- Canvas
  - AROverlay
    - SelectorBar
      - ComponentScroll

## System Flow

[ComponentCategorySO]
        |
        v
[ComponentSelectorUI_SO]
        |
        v
[ComponentDefinitionSO]
        |
        v
[PrototypeComponentSpawner]
        |
        v
[FuelBlock + NozzleBlock Prefabs]

## Authoritative Data
- All selectable components come from ScriptableObjects
- UI does not hardcode component names
- Spawner operates on ComponentDefinitionSO only
