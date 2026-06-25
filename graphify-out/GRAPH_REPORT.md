# Graph Report - .  (2026-06-25)

## Corpus Check
- 6 files · ~13,163 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 110 nodes · 172 edges · 22 communities (8 shown, 14 thin omitted)
- Extraction: 98% EXTRACTED · 2% INFERRED · 0% AMBIGUOUS · INFERRED: 4 edges (avg confidence: 0.85)
- Token cost: 0 input · 0 output

## Community Hubs (Navigation)
- [[_COMMUNITY_Space Engineers API & Mod Base|Space Engineers API & Mod Base]]
- [[_COMMUNITY_HudAPIv2 Initialization & Inputs|HudAPIv2 Initialization & Inputs]]
- [[_COMMUNITY_HudAPIv2 Billboards & Messages|HudAPIv2 Billboards & Messages]]
- [[_COMMUNITY_HudAPIv2 Interface Actions|HudAPIv2 Interface Actions]]
- [[_COMMUNITY_Salvage Config & Ini System|Salvage Config & Ini System]]
- [[_COMMUNITY_Feature Lifecycle & Development Flow|Feature Lifecycle & Development Flow]]
- [[_COMMUNITY_Salvage License Purchase Concept|Salvage License Purchase Concept]]
- [[_COMMUNITY_Programming Guidelines & References|Programming Guidelines & References]]
- [[_COMMUNITY_Graphify Rules & Workflows|Graphify Rules & Workflows]]
- [[_COMMUNITY_SalvageMod Main Loop & Hook|SalvageMod Main Loop & Hook]]
- [[_COMMUNITY_Advanced Ownership Overrides Feature|Advanced Ownership Overrides Feature]]
- [[_COMMUNITY_Connector Friction Safety Feature|Connector Friction Safety Feature]]
- [[_COMMUNITY_Ingame Config Menu Feature|Ingame Config Menu Feature]]
- [[_COMMUNITY_Ini Engine Feature|Ini Engine Feature]]
- [[_COMMUNITY_Menu Logic Refactoring Feature|Menu Logic Refactoring Feature]]
- [[_COMMUNITY_Safety Lockdown Feature|Safety Lockdown Feature]]
- [[_COMMUNITY_Safety Ownership Validation Feature|Safety Ownership Validation Feature]]
- [[_COMMUNITY_Wreck Integrity Pricing Feature|Wreck Integrity Pricing Feature]]
- [[_COMMUNITY_Gemini Motivation & Project Info|Gemini Motivation & Project Info]]
- [[_COMMUNITY_Release Notes & Attribution|Release Notes & Attribution]]

## God Nodes (most connected - your core abstractions)
1. `SalvageMain` - 24 edges
2. `HudAPIv2` - 17 edges
3. `MessageBase` - 10 edges
4. `MenuItemBase` - 9 edges
5. `SalvageConfig` - 8 edges
6. `MenuCategoryBase` - 7 edges
7. `SalvageMenuConfig` - 7 edges
8. `HUD Diagnostic Overlay (Under Development)` - 5 edges
9. `MenuTextInput` - 4 edges
10. `EntityMessage` - 3 edges

## Surprising Connections (you probably didn't know these)
- `SalvageMain` --references--> `HudAPIv2`  [EXTRACTED]
  SalvageMain.cs → HudAPIv2.cs
- `SalvageMain` --references--> `SalvageConfig`  [EXTRACTED]
  SalvageMain.cs → SalvageConfig.cs
- `SalvageMenuConfig` --references--> `SalvageConfig`  [EXTRACTED]
  SalvageMenuConfig.cs → SalvageConfig.cs
- `SalvageMain` --references--> `SalvageMenuConfig`  [EXTRACTED]
  SalvageMain.cs → SalvageMenuConfig.cs
- `Feature Lifecycle Management` --references--> `Multiplayer Synchronization`  [INFERRED]
  GEMINI.md → FeatureFlow/1_OnHold.md

## Import Cycles
- None detected.

## Communities (22 total, 14 thin omitted)

### Community 0 - "Space Engineers API & Mod Base"
Cohesion: 0.22
Nodes (6): IMyCubeBlock, IMyCubeGrid, IMyFaction, MySessionComponentBase, ResultEnum, SalvageMain

### Community 1 - "HudAPIv2 Initialization & Inputs"
Cohesion: 0.19
Nodes (15): Func, APIinfo, Draygo.API, MenuCategoryBase, MenuColorPickerInput, MenuItem, MenuItemBase, MenuKeybindInput (+7 more)

### Community 2 - "HudAPIv2 Billboards & Messages"
Cohesion: 0.17
Nodes (7): BillBoardHUDMessage, EntityMessage, HUDMessage, MessageBase, SpaceMessage, object, Vector2D

### Community 3 - "HudAPIv2 Interface Actions"
Cohesion: 0.19
Nodes (5): Action, bool, HudAPIv2, long, MessageTypes

### Community 4 - "Salvage Config & Ini System"
Cohesion: 0.31
Nodes (4): MyIni, SalvageConfig, SalvageMod, string

### Community 5 - "Feature Lifecycle & Development Flow"
Cohesion: 0.33
Nodes (7): Multiplayer Synchronization, HUD Diagnostic Overlay (Planned), DiagnosticService, HUD Diagnostic Overlay (Under Development), Chat Command & Target Acquisition System, Dynamic Economic Pricing Engine, Feature Lifecycle Management

## Knowledge Gaps
- **24 isolated node(s):** `Draygo.API`, `APIinfo`, `SalvageMod`, `SalvageMod`, `SalvageMod` (+19 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **14 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **Why does `SalvageMain` connect `Space Engineers API & Mod Base` to `SalvageMod Main Loop & Hook`, `HudAPIv2 Interface Actions`, `Salvage Config & Ini System`, `HudAPIv2 Initialization & Inputs`?**
  _High betweenness centrality (0.350) - this node is a cross-community bridge._
- **Why does `HudAPIv2` connect `HudAPIv2 Interface Actions` to `Space Engineers API & Mod Base`, `HudAPIv2 Initialization & Inputs`, `HudAPIv2 Billboards & Messages`?**
  _High betweenness centrality (0.322) - this node is a cross-community bridge._
- **Why does `SalvageConfig` connect `Salvage Config & Ini System` to `Space Engineers API & Mod Base`, `HudAPIv2 Initialization & Inputs`?**
  _High betweenness centrality (0.107) - this node is a cross-community bridge._
- **What connects `Draygo.API`, `APIinfo`, `SalvageMod` to the rest of the system?**
  _25 weakly-connected nodes found - possible documentation gaps or missing edges._