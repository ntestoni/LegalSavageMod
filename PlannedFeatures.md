# Planned Features & Future Roadmap - Legal Salvage Mod

This document lists the prioritized functionalities, performance optimizations, and edge-case handlings planned for future updates of the Legal Salvage Mod. Contribution to these areas is highly welcome.

## 1. External Configuration File Support (INI Configuration)
* **Objective:** Decouple all the numerical values and configuration factors from the hardcoded C# script, making the mod fully customizable for server administrators and players.
* **Implementation Plan:** Create an input/output routine that automatically generates a default `Config.ini` file inside the world storage directory upon the mod's first initialization. Mod functions will be refactored to read coefficients directly from this dictionary, paving the way for eventual *Rich HUD* or in-game menu UI integrations (similar to *Build Vision*).

## 2. Fully implement Player customizable and toggleable features
* **Ojective:** Allow to customize or turn off entirely most of this mod's features
* **Implementation Plan:** Using the TextAPI mod menu, the user will open chat and press F2, then a button must appear top-left. Alternatively the user can edit the `Config.ini` file inside the world storage directory which can then be reloaded mid-game using '/salvage reload' in chat.
