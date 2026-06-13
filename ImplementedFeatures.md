# Implemented Features - Legal Salvage Mod

This document outlines the features and architecture that have been successfully developed, refactored, and integrated into the core codebase of the Legal Salvage Mod for *Space Engineers*.

> **Development Note:** The core code and structural refactoring of this repository have been developed with the assistance of **Gemini (Large Language Model built by Google)**, optimizing for performance, API compliance, and clean code practices.

## 1. Chat Command & Target Acquisition System
* **Chat Command Trigger:** Added a localized chat listener for the `/salvage` command. The command is intercepted and hidden from other players in the chat (`sendToEveryone = false`).
* **Advanced Camera Raycast:** Implemented a 50-meter line-of-sight raycast engine that safely retrieves the target `IMyCubeGrid` from both 1st and 3rd person camera views.

## 2. Safety & Ownership Validation Walls
* **Unowned Grid Detection:** The system detects if a wreck has no legal owners (`BigOwners` count is zero), allowing players to salvage it freely without any pop-up or fee.
* **Self-Ownership Filter:** Prevents players from initiating a legal salvage process on grids they already legally own.
* **Faction & Faction-Type Verification:** Filters targets based on their owning faction. It explicitly blocks salvage requests if the grid belongs to a real player faction, allowing legal negotiations exclusively with NPC factions.
* **Reputation Threshold Wall:** Introduces a reputation checkpoint. If a player's standing with the NPC faction is hostile (below `-500`), all legal salvage negotiations are instantly denied.

## 3. Dynamic Economic Pricing Engine
* **Mass & Structural Telemetry:** The engine accurately computes the total physical mass of the grid and counts the exact number of attached sub-grids (connected via rotors, hinges, or pistons).
* **Grid-Type Specific Routing:** Separate pricing logic handlers have been implemented to route base costs per kilogram differently for Small Grids, Large Grids, and Static Stations, introducing an additional mechanical complexity tax for articulated assemblies.
* **Modular Interface-Based Valuation:** Created an isolated pricing matrix function (`GetSpecialBlockValue`) that scans the grid block-by-block using C# interface pattern matching (`IMyReactor`, `IMyJumpDrive`, `IMyRefinery`, `IMyLargeTurretBase`, etc.). This adds dynamic technological surcharges to the final licensing fee and natively supports modded blocks.
* **DRY Code Refactoring:** Unified the block scanning and mass extraction logic into a single shared helper function (`AnalyzeGridStructure`), allowing for independent scaling factors (`smallGridScale`, `largeGridScale`, `stationScale`) per grid type.

## 4. Wreck Integrity Pricing Surcharges (Debris Factor)
* **Dynamic Health & Progress Assessment:** Refactored the block-by-block valuation within `AnalyzeGridStructure` to leverage the `IMySlimBlock` API. 
* **Adaptive Surcharge Scaling:** The engine extracts structural data via `slimBlock.CurrentDamage` and `slimBlock.MaxIntegrity` to establish a `healthRatio`. This is combined with `slimBlock.BuildLevelRatio` to form an `integrityModifier`. Heavily damaged, uncompleted, or hitted blocks have their technological surcharges scaled down proportionally, ensuring players never pay full price for ruined components.

## 5. Advanced Two-Step Ownership Overrides
* **Hacked & Unowned Block Adaptation:** Addressed a critical core engine behavior where functional blocks grinded below their hacking threshold drop ownership to `0`, causing native grid-wide methods to skip them.
* **Targeted Terminal Sweep:** Implemented a post-transfer loop filtering for `IMyTerminalBlock` instances whose ownership did not sync. The script casts these exceptions to explicitly force ownership registration, matching player parameters.

## 6. Automated Safety Lockdown Routine
Upon a successful transaction and synchronous grid ownership transfer, a safety protocol cycles through all functional blocks (`IMyFunctionalBlock`) across the main grid and all attached sub-grids to prevent immediate accidents or friendly fire:
* **Weapons & AI Turrets:** Automatically disabled (`Enabled = false`) to neutralize defensive systems and custom modded weapons (e.g., *Consolidation Armament*).
* **Automation Systems:** Programmable Blocks and Timer Blocks are shut down to halt defensive or self-repairing NPC scripts.
* **Thrusters & Gyroscopes:** Absolute thrust overrides are zeroed out (`ThrustOverride = 0f`), gyroscope override states are turned off (`GyroOverride = false`), and rotational axes are completely reset before shutting down the blocks to prevent the ship from drifting or spinning out of control.
* **Mechanical Articulations (Rotors & Hinges):** Safety brakes are engaged (`RotorLock = true`), target velocities are reset to `0 RPM`, and blocks are powered down to eliminate mechanical oscillations and physical damage ("Klang").

## 7. User Interface (UI) Feedback
* **Mission Screen Contexts:** Integrated the game's native `ShowMissionScreen` API to present polished pop-up dialogs for contract terms, purchase confirmation, account balance errors, and transaction success reports.

## 8. Connector Friction & Safety Filtering
* **Velocity-Aware Disconnection:** The safety lockdown system checks the target grid's `LinearVelocity` and `AngularVelocity` before acting on connectors. If the grid is moving, the physical connection lock is preserved to prevent immediate grid shearing or collision damage.
* **Magnetic Force Suppression:** Forcing `Enabled = false` on connected or near-locked connectors eliminates their native magnetic attraction pull, preventing dangerous physics oscillations ("Klang") during ownership transitions.