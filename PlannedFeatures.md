# Planned Features & Future Roadmap - Legal Salvage Mod

This document lists the prioritized functionalities, performance optimizations, and edge-case handlings planned for future updates of the Legal Salvage Mod. Contribution to these areas is highly welcome.

## 1. Wreck Integrity Pricing Surcharges (Debris Factor)
* **Objective:** Modify the block-by-block valuation within `AnalyzeGridStructure` to account for physical damage.
* **Implementation Plan:** Leverage the `IMySlimBlock` API (such as `slimBlock.BuildRatio` or integrity health checks). Heavily damaged, uncompleted, or functional-compromised blocks should have their technological pricing surcharge dynamically scaled down or zeroed out, preventing players from paying full price for ruined components.

## 2. Connector & Attached Grid Filtering
* **Objective:** Solve edge-cases regarding grids connected via magnetic docking rings (`IMyShipConnector`) rather than hard physical joints (rotors/pistons).
* **Implementation Plan:** Implement checks to ensure that if an NPC wreck is currently docked at a neutral/friendly trading post or station, the legal salvage purchase does not inadvertently transfer ownership of the entire trading station. The system should either exclude connector-linked grids from the transaction or forcefully disconnect the magnetic lock upon transaction completion.

## 3. Multiplayer Synchronization (Network Sync Framework)
* **Objective:** Transition the mod architecture from a local client-side execution to a fully synchronized Server/Client network model suitable for Dedicated Servers.
* **Implementation Plan:** * Implement a robust Network Packet/Message Handler using `MyAPIGateway.Multiplayer`.
    * Ensure the chat command is initiated on the client, but banking operations (`RequestChangeBalance`) and structural ownership rewrites (`ChangeGridOwnership`) are validated and executed securely on the **Server side** to prevent exploits and desyncs.

## 4. External Configuration File Support (XML Configuration)
* **Objective:** Decouple the pricing matrix values and configuration factors from the hardcoded C# script, making the mod fully customizable for server administrators and players.
* **Implementation Plan:** Create an input/output routine that automatically generates a default `SalvageConfig.xml` file inside the world storage directory upon the mod's first initialization. The `GetSpecialBlockValue` function will be refactored to read pricing coefficients directly from this dictionary, paving the way for eventual *Rich HUD* or in-game menu UI integrations (similar to *Build Vision*).