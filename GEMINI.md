# Project Objectives
* Create a mod for *Space Engineers*, the game developed by Keen Software House.
* Upon completion, the mod will be published on the Steam Workshop platform.

# Motivation
In the game, some random encounters involve wrecks belonging to specific factions. If a player attempts to grind down or destroy the blocks making up these wrecks, the action is flagged as an act of piracy. This decreases the player's reputation with the wreck's owning faction and increases their standing with the Space Pirates. The purpose of this mod is to allow the player, without leaving the game, to contact the faction that owns the wreck and purchase the legal rights to dismantle it, thereby preventing any reputation loss.

# Source Files and Contents
* The `SalvageMain.cs` file, written in C#, contains the core script code.
* The 'SalvageConfig.cs' file, written in C#, contains the code for script configuration.
* The `SalvageMenuConfig.cs` file, written in C#, handles UI menu construction and registration.

# Language Requirements
* All repository files must contain text and source code comments written exclusively in English.
* All dialogue phrases or UI text lines must be written in English or abstracted into localization strings to facilitate future translations into other languages.

# Feature Lifecycle Management (Flow Control; folder .\FeatureFlow\)
The development process must follow a strict feature lifecycle flow, ensuring documentation is always accurate and synchronized with the code base. This cycle involves four primary states:
1. **On Hold:**  Features deemed too complex, out of scope for the current release, or requiring significant external dependencies are documented in `1_OnHold.md`.
				 These features should not be coded until explicitly approved by the project lead and *moved* to the 'Planned' state.
2. **Planned:**  Features that have been scoped, researched, and prioritized but not yet started are listed in `2_Planned.md`.
			     Development work begins only when a feature is *moved* from this list to the `3_Develop.md` file.
3. **Develop:**  The feature which is currently under development and all the details about its implementation plan are documented in `3_Develop.md`. 
				 The header of this file should contain a brief summary of the feature currently under development.
4. **Complete:** Once development, testing, and integration of a feature are complete, it must be *moved* to the 'Complete' state and documented in `4_Complete.md`.
				 A summary of all the implemented characteristics of the feature must be written along with it.

# Programming Style & Guidelines
* Always consider the latest version of the project files.
* Modify project files the least possible to comply with my requests.
* If it becomes absolutely necessary to refactor project files, explain the reasoning behind it before doing it and ask for explicit approval.
* The file `References.md` contains list of avalable reference web sites for the ModAPI and the programming style.
* Always prioritize the public interface exposed explicitly for mods over the internal properties or concrete classes of Keen's underlying source code.
* If it becomes absolutely necessary to use internal properties or unexposed game-engine classes, verify beforehand that the code can safely execute within a modded environment.
* If internal properties are used, inform me of this architectural choice and explain the underlying technical reasons.
* Always insert meaningful comments within the code and preserve existing comments.
* If a portion of the code described by a comment is modified, update the corresponding comment accordingly.
* When adding or modifying code comments, use the English language exclusively.
* If you deem it necessary to add new files to the project structure, explain the reasoning behind it.
* Never code a feature without first documenting its intent in the planned feature file.
* Never mark a feature as implemented until the changes are reflected in the core codebase and tested thoroughly.
* When the development of a feature is completed, update the Feature Lifecycle files immediately.
* If an already implemented feature is modified or refactored, update the Feature Lifecycle files immediately.
* When adding or modifying code, list all the necessary modifications and do not list the unmodified functions.

# Finalization & Release Guidelines (Maturity Phase)
As the mod approaches completion, all development efforts must prioritize stability and polish. Any future changes must adhere to these guidelines:
*   **Mandatory Testing:** Before any code modification is committed, a comprehensive test plan covering edge cases, performance bottlenecks, and integration points must be executed and documented.
*   **Release Notes:** A dedicated `ReleaseNotes.md` file must be created/updated immediately after a successful feature implementation or major refactor, detailing changes for the user.
*   **Performance Focus:** All code reviews must include profiling checks to ensure that new features do not introduce performance degradation on large grids.

# Additional Notes
* Maintain the attribution statement inside `ReleaseNotes.md` indicating that the code was developed with your assistance. Identify yourself clearly so that it is easy to track which model version was used
* Notify me immediately whenever you are running low on context or approaching your token limits.
