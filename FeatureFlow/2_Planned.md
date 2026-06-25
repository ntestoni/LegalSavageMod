
## HUD Diagnostic Overlay (Real-Time Visual License Fee Breakdown)
* **Objective:** Allow players to inspect wrecks and visualize real-time estimated salvage licensing fees before entering transaction confirmations.
* **Implementation Plan:** 

    * Execute a client-side camera raycast checking target grids at regular intervals (e.g. every 10 ticks).
    * Display an overlay block using the Text HUD API (`HUDMessage`) at a fixed screen coordinate detailing structural mass, sub-grids count, owner faction standing, and estimated Licensing Fee breakdown.