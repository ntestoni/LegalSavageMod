# Legal Salvage Mod - Version 1.0 (Initial Release Candidate)

## Overview
This version introduces the core functionality of the Legal Salvage Mod, allowing players to legally negotiate and purchase rights to dismantle faction-owned wrecks within *Space Engineers*. This prevents reputation loss associated with unauthorized wreck destruction.

## New Features Implemented in v1.0
*   **Full Ownership Validation:** System checks ensure that salvage requests are only processed against NPC-owned factions, blocking player-owned grids entirely.
*   **Advanced Pricing Engine:** Dynamic calculation of licensing fees based on:
    *   Total physical mass (Kg).
    *   Grid type (Small, Large, Station) with specific scaling factors.
    *   Structural complexity and count of attached sub-grids.
    *   Technological components (Reactors, Jump Drives, Refineries, etc.) via a modular interface system.
*   **Integrity Surcharges:** Pricing is dynamically reduced based on the structural integrity (`healthRatio`) and build level of individual blocks, ensuring fair cost assessment for damaged wrecks.
*   **Safety Lockdown Protocol:** Upon successful transaction, all functional systems (weapons, thrusters, rotors, etc.) are automatically disabled and locked down to prevent immediate accidents or physical damage ("Klang").
*   **Robust Ownership Transfer:** Implements a two-step transfer process that forces ownership registration on individual blocks, even if the core engine fails to sync them.

## Key Improvements & Polish
*   **Configuration System:** Full support for external INI configuration files (`SalvageConfig.ini`), allowing administrators to tune pricing multipliers and thresholds without recompiling code.
*   **UI/UX:** Implemented a polished Mission Screen interface for all transaction steps, providing clear feedback on costs, requirements, and success/failure states.
*   **Admin Tools:** Added `/salvage reload` command for server admins to force configuration synchronization mid-game.

## Known Limitations & Future Work (V1.1+)
*   [To be filled after QA]
*   The current system relies on the player's local balance API; potential issues with multi-server/proxy environments may require further testing.