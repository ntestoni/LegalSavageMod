# On Hold Features - Legal Salvage Mod

This document collect features that are desirable but not yet planned for future updates of the Legal Salvage Mod. Contribution to these areas is highly welcome.

## Multiplayer Synchronization (Network Sync Framework)
* **Objective:** Transition the mod architecture from a local client-side execution to a fully synchronized Server/Client network model suitable for Dedicated Servers.
* **Implementation Plan:** * Implement a robust Network Packet/Message Handler using `MyAPIGateway.Multiplayer`.
    * Ensure the chat command is initiated on the client, but banking operations and structural ownership rewrites are validated and executed securely on the **Server side** (`IsServer`) to prevent client exploits and desyncs.