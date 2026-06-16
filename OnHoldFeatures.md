# On Hold Features - Legal Salvage Mod

This document collect features that are desirable but not yet planned for future updates of the Legal Salvage Mod. Contribution to these areas is highly welcome.

## Multiplayer Synchronization (Network Sync Framework)
* **Objective:** Transition the mod architecture from a local client-side execution to a fully synchronized Server/Client network model suitable for Dedicated Servers.
* **Implementation Plan:** * Implement a robust Network Packet/Message Handler using `MyAPIGateway.Multiplayer`.
    * Ensure the chat command is initiated on the client, but banking operations and structural ownership rewrites are validated and executed securely on the **Server side** (`IsServer`) to prevent client exploits and desyncs.

## HUD Diagnostic Overlay (Real-Time Visual License Fee Breakdown)
* **Objective:** Allow players to inspect wrecks and visualize real-time estimated salvage licensing fees before entering transaction confirmations.
* **Implementation Plan:** 
    * Execute a client-side camera raycast checking target grids at regular intervals (e.g. every 10 ticks).
    * Display an overlay block using the Text HUD API (`HUDMessage`) at a fixed screen coordinate detailing structural mass, sub-grids count, owner faction standing, and estimated Licensing Fee breakdown.
