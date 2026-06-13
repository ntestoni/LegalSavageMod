# Project Objectives
* Create a mod for *Space Engineers*, the game developed by Keen Software House.
* Upon completion, the mod will be published on the Steam Workshop platform.

# Motivation
In the game, some random encounters involve wrecks belonging to specific factions. If a player attempts to grind down or destroy the blocks making up these wrecks, the action is flagged as an act of piracy. This decreases the player's reputation with the wreck's owning faction and increases their standing with the Space Pirates. The purpose of this mod is to allow the player, without leaving the game, to contact the faction that owns the wreck and purchase the legal rights to dismantle it, thereby preventing any reputation loss.

# Files and Contents
* The `SalvageMain.cs` file, written in C#, contains the core script code.
* The `PlannedFeatures.md` file, written in Markdown, lists the features left to implement.
* The `ImplementedFeatures.md` file, written in Markdown, lists the features already successfully implemented.

# Language Requirements
* All repository files must contain text and source code comments written exclusively in English.
* All dialogue phrases or UI text lines must be written in English or abstracted into localization strings to facilitate future translations into other languages.

# Programming Style & Guidelines
* Always prioritize the public interface exposed explicitly for mods over the internal properties or concrete classes of Keen's underlying source code.
* If it becomes absolutely necessary to use internal properties or unexposed game-engine classes, verify beforehand that the code can safely execute within a modded environment.
* If internal properties are used, inform me of this architectural choice and explain the underlying technical reasons.
* Always insert meaningful comments within the code and preserve existing comments.
* If a portion of the code described by a comment is modified, update the corresponding comment accordingly.
* When adding or modifying code comments, use the English language exclusively.
* If you deem it necessary to add new files to the project structure, explain the reasoning behind it.
* When the development of a feature is completed, update the `PlannedFeatures.md` and `ImplementedFeatures.md` files immediately.
* If an already implemented feature is modified or refactored, update the `ImplementedFeatures.md` file accordingly.
* When adding or modifying code, list all the necessary modifications and do not list the unmodified functions.

# Additional Notes
* Maintain the attribution statement inside `ImplementedFeatures.md` indicating that the code was developed with your assistance. Identify yourself clearly so that it is easy to track which model version was used (Gemini 1.5 Pro / 2.0 Flash in 2026).
* Notify me immediately whenever you are running low on context or approaching your token limits.
* When chatting with me, use the Italian language.
