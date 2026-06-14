# Planned Features & Future Roadmap - Legal Salvage Mod

This document lists the prioritized functionalities, performance optimizations, and edge-case handlings planned for future updates of the Legal Salvage Mod. Contribution to these areas is highly welcome.

## 1. Rich In-Game Configuration Menu Layout (TextAPI Framework)
* **Objective:** Allow client users and hosts to configure core mod options or visualize detailed financial diagnostic charts using a real-time visual interface rather than manual text editor adjustments.
* **Implementation Plan:** Integrate the TextAPI mod configuration infrastructure. This template intercepts client menu buttons when typing F2 inside text prompts, rendering a contextual overlay that allows toggle controls over calculation components (e.g., dynamically turning on/off block pricing surcharges or adjusting mass per-Kilo variables directly from the game screen).