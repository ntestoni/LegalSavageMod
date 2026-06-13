using System;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SalvageMod
{
    // This attribute ensures the script is loaded by the game upon session start
    [MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
    public class SalvageMain : MySessionComponentBase
    {
        private bool _isInitialized = false;

        public override void UpdateAfterSimulation()
        {
            // Safe initialization (waits until the game world and APIs are ready)
            if (!_isInitialized)
            {
                if (MyAPIGateway.Session != null && MyAPIGateway.Utilities != null)
                {
                    // Listen for global chat messages
                    MyAPIGateway.Utilities.MessageEntered += OnMessageEntered;
                    _isInitialized = true;
                    MyLog.Default.WriteLineAndConsole("SalvageMod: Successfully initialized!");
                }
            }
        }

        protected override void UnloadData()
        {
            // Unregister events on exit to prevent potential memory leaks
            if (MyAPIGateway.Utilities != null)
            {
                MyAPIGateway.Utilities.MessageEntered -= OnMessageEntered;
            }
        }

        private void LogDebug(string message)
        {
            // Sintassi: (Testo, Durata in millisecondi, Colore del font)
            MyLog.Default.WriteLineAndConsole($"SalvageMod: {message}");
        }

        private void OnMessageEntered(string messageText, ref bool sendToEveryone)
        {
            // Trigger command when a player types /salvage in chat
            if (messageText.Equals("/salvage", StringComparison.OrdinalIgnoreCase))
            {
                sendToEveryone = false; // Hide this command from other players in chat
                CheckTargetGrid();
            }
        }

        private void CheckTargetGrid()
        {
            // Get the local player's character entity
            IMyCharacter character = MyAPIGateway.Session?.Player?.Character;
            if (character == null) return;

            // Retrieve current camera position and direction (supports both 1st and 3rd person views)
            MatrixD cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            Vector3D startPos = cameraMatrix.Translation;
            Vector3D direction = cameraMatrix.Forward;
            Vector3D endPos = startPos + (direction * 50.0); // 50-meter raycast range

            // Perform raycast against physical objects in line of sight
            IHitInfo hitInfo;
            MyAPIGateway.Physics.CastRay(startPos, endPos, out hitInfo);

            if (hitInfo != null && hitInfo.HitEntity is IMyCubeGrid)
            {
                IMyCubeGrid targetGrid = (IMyCubeGrid)hitInfo.HitEntity;

                if (targetGrid.BigOwners == null || targetGrid.BigOwners.Count == 0)
                {
                    ShowPopupMessage("SALVAGE REPORT", "This wreck is unowned.\nYou can salvage it freely without paying any licensing fee!");
                    return;
                }

                // --- Ownnership check
                long playerId = MyAPIGateway.Session.Player.IdentityId;
                long ownerId = targetGrid.BigOwners[0];

                // If the player already owns the grid, stop the transaction
                if (ownerId == playerId)
                {
                    ShowPopupMessage("SALVAGE REPORT", "You already own this grid.\nNo salvage permit needed!");
                    return;
                }

                IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);

                if (ownerFaction == null)
                {
                    ShowPopupMessage("SALVAGE ERROR", "Unknown owner or invalid faction entity.");
                    return;
                }

                if (!ownerFaction.IsEveryoneNpc())
                {
                    ShowPopupMessage("SALVAGE REJECTED", $"This grid belongs to a real player faction [{ownerFaction.Tag}].\nSalvage permit negotiations are unavailable.");
                    return;
                }

                // --- Reputation wall
                int reputation = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerId, ownerFaction.FactionId);

                // If the player is hostile (reputation below -500), deny the contract
                if (reputation < -500)
                {
                    ShowPopupMessage("TRANSACTION DENIED", $"The faction {ownerFaction.Name} [{ownerFaction.Tag}] refuses to negotiate with you.\nReason: Hostile standing (Reputation below -500).");
                    return;
                }

                ProcessSalvage(targetGrid, ownerFaction);
            }
            else
            {
                ShowPopupMessage("SALVAGE ERROR", "No grid in sight.\nPlease move closer to your target wreck (Max 50 meters).");
            }
        }

        // --- UI HELPERS ---
        private void ShowPopupMessage(string title, string message)
        {
            // TODO: change "SalvageStatus" to something more fancy
            MyAPIGateway.Utilities.ShowMissionScreen(
                title,
                "SalvageStatus",
                null,
                message,
                null,
                "OK"
            );
        }

        private void ProcessSalvage(IMyCubeGrid targetGrid, IMyFaction ownerFaction)
        {
            long targetFactionId = ownerFaction.FactionId;
            long playerId = MyAPIGateway.Session.Player.IdentityId;
            int reputation = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerId, targetFactionId);

            // Output parameters to be populated by the calculation engine
            int subGridCount;
            float totalMass;

            // Fetch base structural costs and UI telemetry data
            double totalCost = CalculateTotalStructureData(targetGrid, ownerFaction, out subGridCount, out totalMass);

            // Apply faction reputation dynamic modifiers
            double reputationMod = Math.Max(0.5, Math.Min(3.0, 1.0 - (reputation / 1500.0)));
            long finalCost = (long)(totalCost * reputationMod);
            string formattedMass = totalMass.ToString("N0");

            // Build dynamic Mission Screen context (UI)
            string title = "SALVAGE PERMIT REQUEST";
            string body = $"You are requesting a legal salvage permit from {ownerFaction.Name} [{ownerFaction.Tag}] for the wreck: {targetGrid.DisplayName}.\n\n" +
                          $"• Total Mass: {formattedMass} Kg\n";

            if (subGridCount > 0)
            {
                body += $"• Attached Sub-grids: {subGridCount}\n";
            }
            else
            {
                body += $"• Attached Sub-grids: None (Single Grid)\n";
            }

            body += $"• Licensing Fee: {finalCost.ToString("N0")} SC\n\n" +
                    $"Do you accept and wish to transfer the funds?";

            // TODO: change "SalvageContract" to something more fancy
            MyAPIGateway.Utilities.ShowMissionScreen(
                title,
                "SalvageContract",
                null,
                body,
                (result) => OnSalvageScreenClosed(result, targetGrid, ownerFaction, finalCost),
                "BUY LICENCE"
            );
        }

        // --- CALCULATION ENGINE ---
        private double CalculateTotalStructureData(IMyCubeGrid targetGrid, IMyFaction ownerFaction, out int subGridCount, out float totalMass)
        {
            // Retrieve the entire tree of physically connected grids
            var connectedGrids = new System.Collections.Generic.List<IMyCubeGrid>();
            var gridGroup = MyAPIGateway.GridGroups.GetGridGroup(GridLinkTypeEnum.Physical, targetGrid);

            if (gridGroup != null)
            {
                gridGroup.GetGrids(connectedGrids);
            }
            else
            {
                connectedGrids.Add(targetGrid);
            }

            // Compute telemetry output values for UI and Debugging
            subGridCount = connectedGrids.Count - 1;
            totalMass = 0f;
            double totalCost = 0;

            // Iterate through the grid structure tree leveraging specific cost routers
            foreach (var subGrid in connectedGrids)
            {
                float subGridMass;

                // Accumulate cost and extract individual sub-grid mass via the type router
                totalCost += GetBaseCostByGridType(subGrid, ownerFaction, out subGridMass);
                totalMass += subGridMass;
            }

            // Add an overhead mechanical complexity tax for articulated assemblies
            if (connectedGrids.Count > 1)
            {
                totalCost += (connectedGrids.Count - 1) * 2500.0;
            }

            return totalCost;
        }

        private double GetBaseCostByGridType(IMyCubeGrid subGrid, IMyFaction ownerFaction, out float gridMass)
        {
            // Compute the owner of the current subGrid
            long majorityOwnerId = (subGrid.BigOwners != null && subGrid.BigOwners.Count > 0) ? subGrid.BigOwners[0] : 0;

            // If the owner of the subgrid does belong to the target faction, exclude the grid from cost and mass computation
            // This screens out player ships, trading stations, or third-party enemy grids
            if (!ownerFaction.IsMember(majorityOwnerId))
            {
                gridMass = 0f;
                return 0.0; // Costs nothing and weighs nothing
            }

            // Static structures (Physics engine disabled by default by VRAGE)
            if (subGrid.IsStatic)
            {
                return CalculateStationCost(subGrid, ownerFaction, out gridMass);
            }

            // Mobile grids, route calculations based on block scaling definitions
            switch (subGrid.GridSizeEnum)
            {
                case MyCubeSize.Large:
                    return CalculateLargeGridCost(subGrid, ownerFaction, out gridMass);

                case MyCubeSize.Small:
                    return CalculateSmallGridCost(subGrid, ownerFaction, out gridMass);

                default:
                    gridMass = 5000f;
                    return 5000.0;
            }
        }

        // --- TYPE-SPECIFIC COST HANDLERS ---
        private double CalculateSmallGridCost(IMyCubeGrid subGrid, IMyFaction ownerFaction, out float gridMass)
        {
            // Configuration factors to scale pricing behavior for small grids
            double costPerKg = 5.0;
            double smallGridScale = 1.0;
            float fallbackMass = 15000f;

            // Delegate structural block scan and mass extraction to the core helper engine
            double specialBlocksCost = AnalyzeGridStructure(subGrid, ownerFaction, fallbackMass, smallGridScale, out gridMass);
            return (gridMass * costPerKg) + specialBlocksCost;
        }

        private double CalculateLargeGridCost(IMyCubeGrid subGrid, IMyFaction ownerFaction, out float gridMass)
        {
            // Configuration factors to scale pricing behavior for large grids
            double costPerKg = 1.5;
            double largeGridScale = 1.0;
            float fallbackMass = 500000f;

            // Delegate structural block scan and mass extraction to the core helper engine
            double specialBlocksCost = AnalyzeGridStructure(subGrid, ownerFaction, fallbackMass, largeGridScale, out gridMass);
            return (gridMass * costPerKg) + specialBlocksCost;
        }

        private double CalculateStationCost(IMyCubeGrid subGrid, IMyFaction ownerFaction, out float gridMass)
        {
            // Configuration factors to scale pricing behavior for static outposts
            double costPerKg = 0.8;
            double stationScale = 1.0;
            double stationTax = 75000.0;
            float fallbackMass = 1200000f;

            // Delegate structural block scan and mass extraction to the core helper engine
            double specialBlocksCost = AnalyzeGridStructure(subGrid, ownerFaction, fallbackMass, stationScale, out gridMass);
            double baseCost = (gridMass > 0f) ? (gridMass * costPerKg) + stationTax : 0.0;

            return baseCost + specialBlocksCost;
        }
        // --- PRICING MATRIX FUNCTION ---
        private double GetSpecialBlockValue(VRage.Game.ModAPI.IMyCubeBlock fatBlock)
        {
            if (fatBlock == null) return 0.0;

            // 1. High-tier Power Generation & FTL
            if (fatBlock is IMyReactor) return 150000.0;
            if (fatBlock is IMyJumpDrive) return 250000.0;
            if (fatBlock is IMyBatteryBlock) return 30000.0;
            if (fatBlock is IMySolarPanel) return 10000.0;

            // 2. Production & Advanced Logistics
            if (fatBlock is IMyRefinery) return 90000.0;
            if (fatBlock is IMyAssembler) return 45000.0;
            if (fatBlock is IMyGasGenerator) return 20000.0; // H2/O2 Generator
            if (fatBlock is IMyGasTank) return 15000.0;
            if (fatBlock is IMyCargoContainer) return 25000.0;
            if (fatBlock is IMyConveyorSorter) return 12000.0; // Added sorting systems

            // 3. Weapons & Defense (Covers vanilla and mods inheriting from core turret bases)
            if (fatBlock is IMyLargeTurretBase) return 35000.0;
            if (fatBlock is IMyUserControllableGun) return 15000.0; // Fixed guns/launchers
            if (fatBlock is IMyWarhead) return 50000.0;

            // 4. Control Systems & Cockpits
            if (fatBlock is IMyCockpit) return 20000.0; // Cryo pods, flight seats, cockpits
            if (fatBlock is IMyRemoteControl) return 15000.0;
            if (fatBlock is IMyBeacon) return 10000.0;
            if (fatBlock is IMyRadioAntenna) return 12000.0;

            // 5. Automation, Utility & Safety Locks
            if (fatBlock is IMyShipConnector) return 8000.0;
            if (fatBlock is IMyProjector) return 30000.0;
            if (fatBlock is IMyProgrammableBlock) return 40000.0;
            if (fatBlock is IMyTimerBlock) return 5000.0;
            if (fatBlock is IMySensorBlock) return 5000.0;
            if (fatBlock is IMyMedicalRoom) return 60000.0; // Medical rooms and survival kits
            if (fatBlock is IMyAirVent) return 4000.0;

            // 6. Basic Terminal Interfaces
            if (fatBlock is IMyControlPanel) return 1500.0;
            if (fatBlock is IMyButtonPanel) return 2500.0;

            // Any functional block not explicitly listed falls back to zero surcharge (mass only)
            return 0.0;
        }

        private double AnalyzeGridStructure(IMyCubeGrid subGrid, IMyFaction ownerFaction, float fallbackMass, double scaleFactor, out float gridMass)
        {
            gridMass = 0f;
            double specialBlocksCost = 0.0;

            // If it's a legitimate sub-grid of the target faction, apply its physics mass
            gridMass = (subGrid.Physics != null) ? subGrid.Physics.Mass : 0f;
            if (gridMass <= 0f) gridMass = fallbackMass;

            // Second pass: Compute special blocks cost, strictly matching the target faction
            var blockList = new System.Collections.Generic.List<IMySlimBlock>();
            subGrid.GetBlocks(blockList);

            foreach (var slimBlock in blockList)
            {
                // Skip armor blocks since they do not have advanced technological logic
                if (slimBlock.FatBlock == null) continue;

                // Skip blocks that does not belong to the owner faction of the target grid
                if (!ownerFaction.IsMember(slimBlock.FatBlock.OwnerId)) continue;

                // Calculate the real biological health ratio based on component damage
                float maxHp = slimBlock.MaxIntegrity;
                float currentDamage = slimBlock.CurrentDamage;
                float healthRatio = (maxHp > 0f) ? Math.Max(0f, (maxHp - currentDamage) / maxHp) : 1f;
                
                // Combine real combat health with grinder deconstruction level ratio
                float integrityModifier = healthRatio * slimBlock.BuildLevelRatio;

                // Query the valuation matrix, apply the integrity modifier and the grid scale factor
                double blockBaseValue = GetSpecialBlockValue(slimBlock.FatBlock);
                specialBlocksCost += (blockBaseValue * integrityModifier) * scaleFactor;
            }

            return specialBlocksCost;
        }


        private void OnSalvageScreenClosed(ResultEnum result, IMyCubeGrid grid, IMyFaction ownerFaction, long finalCost)
        {
            // Verify if user committed to the prompt (0 = OK/BUY LICENCE)
            if ((int)result != 0)
                return;

            // Retrieve local player handle from active session context
            var player = MyAPIGateway.Session.Player;
            if (player == null)
            {
                ShowPopupMessage("TRANSACTION ERROR", "Local player data entity could not be resolved.");
                return;
            }

            // 1. BANKING OPERATIONS
            long playerBalance = 0;

            if (player.TryGetBalanceInfo(out playerBalance))
            {
                if (playerBalance < finalCost)
                {
                    ShowPopupMessage("TRANSACTION DECLINED", $"Insufficient Space Credits!\nRequired: {finalCost.ToString("N0")} SC\nYour Balance: {playerBalance.ToString("N0")} SC");
                    return;
                }

                // Balance deduction routine via explicit relative parameter delta
                player.RequestChangeBalance(-finalCost);
            }
            else
            {
                ShowPopupMessage("TRANSACTION ERROR", "Could not safely retrieve account balance information.");
                return;
            }

            // 2. GRID OWNERSHIP TRANSFER & SAFETY LOCKDOWN SYSTEM
            CompleteSalvageOwnershipTransfer(grid, ownerFaction, player.IdentityId);

            // 3. VISUAL FEEDBACK
            ShowPopupMessage("SALVAGE PERMIT GRANTED", $"Transaction successful!\n{finalCost.ToString("N0")} SC have been deducted from your balance.\n\nThe entire grid structure is now legally registered under your ownership.");
        }

        private void CompleteSalvageOwnershipTransfer(IMyCubeGrid grid, IMyFaction ownerFaction, long playerIdentityId)
        {
            // Retrieve all connected sub-grids via physical links
            var connectedGrids = new System.Collections.Generic.List<IMyCubeGrid>();
            var gridGroup = MyAPIGateway.GridGroups.GetGridGroup(GridLinkTypeEnum.Physical, grid);

            if (gridGroup != null)
                gridGroup.GetGrids(connectedGrids);
            else
                connectedGrids.Add(grid);

            // Iterate through each connected grid to validate ownership before transferring
            foreach (var subGrid in connectedGrids)
            {
                // Compute the owner of the current subGrid
                long majorityOwnerId = (subGrid.BigOwners != null && subGrid.BigOwners.Count > 0) ? subGrid.BigOwners[0] : 0;

                // Safety Filter: Skip this sub-grid entirely if it doesn't belong to the target faction
                // (Prevents stealing player ships, neutral trading stations, or unrelated third-party grids)
                if (!ownerFaction.IsMember(majorityOwnerId))
                {
                    continue;
                }

                // Step 2: Safe Transfer of the validated grid
                // This changes the ownership of all blocks on this specific grid to the player
                subGrid.ChangeGridOwnership(playerIdentityId, MyOwnershipShareModeEnum.Faction);

                // Hacked/unowned blocks are unaffected and must be processed separately
                // We use a 2-step approach since ChangeGridOwnership():
                // 1. autmatically updates BigOwners for the whole grid,
                // 2. sends only one network packet relative to the whole grid,
                // 3. forces the refresh of terminals for all connected players.

                var blockList = new System.Collections.Generic.List<IMySlimBlock>();
                subGrid.GetBlocks(blockList);

                foreach (var slimBlock in blockList)
                {
                    // Skip armor blocks and blocks without a physical fatblock instance
                    if (slimBlock.FatBlock == null) continue;

                    // Check if the block is a terminal/functional block, but its OwnerId was either ignored 
                    // by the global routine (hacked/incomplete blocks with Owner 0) or failed to sync properly
                    if (slimBlock.FatBlock is Sandbox.ModAPI.Ingame.IMyTerminalBlock && slimBlock.FatBlock.OwnerId != playerIdentityId)
                    {
                        // Cast to the concrete game object class to access the actual ownership modification method
                        var cubeBlock = slimBlock.FatBlock as Sandbox.Game.Entities.MyCubeBlock;
                        if (cubeBlock != null)
                        {
                            // Forcefully override ownership and share mode on this specific block
                            cubeBlock.ChangeOwner(playerIdentityId, VRage.Game.MyOwnershipShareModeEnum.Faction);
                        }
                    }
                }

                // Step 3: Run the safety lockdown routine ONLY on this validated grid's systems
                ApplySafetyLockdownToGrid(subGrid);
            }
        }

        private void ApplySafetyLockdownToGrid(IMyCubeGrid grid)
        {
            System.Collections.Generic.List<IMySlimBlock> gridBlocks = new System.Collections.Generic.List<IMySlimBlock>();

            // Fetch all slim blocks inside the current grid
            gridBlocks.Clear();
            grid.GetBlocks(gridBlocks);

            foreach (var slimBlock in gridBlocks)
            {
                // Armor blocks do not have a FatBlock component. Skip them safely.
                if (slimBlock.FatBlock == null) continue;

                // Check if the block is a functional terminal block (can be turned on/off)
                var functionalBlock = slimBlock.FatBlock as IMyFunctionalBlock;
                if (functionalBlock == null) continue;

                // Typecast checks using the correct namespaces for core systems and automation components
                bool isTurret = functionalBlock is IMyLargeTurretBase;
                bool isWeapon = functionalBlock is IMyUserControllableGun;
                bool isProgrammable = functionalBlock is IMyProgrammableBlock;
                bool isTimer = functionalBlock is SpaceEngineers.Game.ModAPI.IMyTimerBlock; // Corrected namespace

                // Safely disable hazardous weapon, script, and automation systems
                if (isTurret || isWeapon || isProgrammable || isTimer)
                {
                    functionalBlock.Enabled = false;
                }

                // Handle thruster systems (Disable block and clear absolute thrust override values)
                var thruster = functionalBlock as IMyThrust;
                if (thruster != null)
                {
                    thruster.ThrustOverride = 0f;
                    thruster.Enabled = false;
                }

                // Handle gyroscope systems (Disable block and completely reset rotation override states)
                var gyro = functionalBlock as IMyGyro;
                if (gyro != null)
                {
                    gyro.GyroOverride = false;
                    gyro.Pitch = 0f;
                    gyro.Yaw = 0f;
                    gyro.Roll = 0f;
                    gyro.Enabled = false;
                }

                // Handle stator systems (Disable block, engage safety brake and reset target velocity)
                var rotor = functionalBlock as IMyMotorStator;
                if (rotor != null)
                {
                    rotor.RotorLock = true; // Engage the safety brake to completely freeze the joint
                    rotor.TargetVelocityRPM = 0f; // Reset velocity memory to avoid unexpected movement upon reactivation
                    rotor.Enabled = false;
                }
            }
        }

    }
}