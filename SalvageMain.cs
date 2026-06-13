using System;
using Sandbox.ModAPI;
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

                IMyFaction npcFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);

                if (npcFaction == null)
                {
                    ShowPopupMessage("SALVAGE ERROR", "Unknown owner or invalid faction entity.");
                    return;
                }

                if (!npcFaction.IsEveryoneNpc())
                {
                    ShowPopupMessage("SALVAGE REJECTED", $"This grid belongs to a real player faction [{npcFaction.Tag}].\nSalvage permit negotiations are unavailable.");
                    return;
                }

                // --- Reputation wall
                int reputation = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerId, npcFaction.FactionId);

                // If the player is hostile (reputation below -500), deny the contract
                if (reputation < -500)
                {
                    ShowPopupMessage("TRANSACTION DENIED", $"The faction {npcFaction.Name} [{npcFaction.Tag}] refuses to negotiate with you.\nReason: Hostile standing (Reputation below -500).");
                    return;
                }

                ProcessNpcSalvage(targetGrid, npcFaction);
            }
            else
            {
                ShowPopupMessage("SALVAGE ERROR", "No grid in sight.\nPlease move closer to your target wreck (Max 50 meters).");
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

        private void ProcessNpcSalvage(IMyCubeGrid grid, IMyFaction faction)
        {
            long playerId = MyAPIGateway.Session.Player.IdentityId;
            int reputation = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerId, faction.FactionId);

            // Output parameters to be populated by the calculation engine
            int subGridCount;
            float totalMass;

            // Fetch base structural costs and UI telemetry data
            double totalCost = CalculateTotalStructureData(grid, out subGridCount, out totalMass);

            // Apply faction reputation dynamic modifiers
            double reputationMod = Math.Max(0.5, Math.Min(3.0, 1.0 - (reputation / 1500.0)));
            long finalCost = (long)(totalCost * reputationMod);
            string formattedMass = totalMass.ToString("N0");

            // Build dynamic Mission Screen context (UI)
            string title = "SALVAGE PERMIT REQUEST";
            string body = $"You are requesting a legal salvage permit from {faction.Name} [{faction.Tag}] for the wreck: {grid.DisplayName}.\n\n" +
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
                (result) => OnSalvageScreenClosed(result, playerId, finalCost, grid),
                "BUY LICENCE"
            );
        }

        // --- CALCULATION ENGINE ---
        private double CalculateTotalStructureData(IMyCubeGrid hitGrid, out int subGridCount, out float totalMass)
        {
            // 1. Retrieve the entire tree of physically connected grids
            var connectedGrids = new System.Collections.Generic.List<IMyCubeGrid>();
            var gridGroup = MyAPIGateway.GridGroups.GetGridGroup(GridLinkTypeEnum.Physical, hitGrid);

            if (gridGroup != null)
            {
                gridGroup.GetGrids(connectedGrids);
            }
            else
            {
                connectedGrids.Add(hitGrid);
            }

            // 2. Compute telemetry output values for UI and Debugging
            subGridCount = connectedGrids.Count - 1;
            totalMass = 0f;
            double totalCost = 0;

            // 3. Iterate through the grid structure tree leveraging specific cost routers
            foreach (var subGrid in connectedGrids)
            {
                // Accumulate true grid mass if physics engine is active
                totalMass += (subGrid.Physics != null) ? subGrid.Physics.Mass : 0f;

                // Accumulate cost calculated by individual grid type handlers
                totalCost += GetBaseCostByGridType(subGrid);
            }

            // Add an overhead mechanical complexity tax for articulated assemblies
            if (connectedGrids.Count > 1)
            {
                totalCost += (connectedGrids.Count - 1) * 2500.0;
            }

            return totalCost;
        }

        private double GetBaseCostByGridType(IMyCubeGrid grid)
        {
            // Case 1: Static structures (Physics engine disabled by default by VRAGE)
            if (grid.IsStatic)
            {
                return CalculateStationCost(grid);
            }

            // Case 2: Mobile grids, route calculations based on block scaling definitions
            switch (grid.GridSizeEnum)
            {
                case MyCubeSize.Large:
                    return CalculateLargeGridCost(grid);

                case MyCubeSize.Small:
                    return CalculateSmallGridCost(grid);

                default:
                    return 5000.0; // Fail-safe fallback value
            }
        }

        // --- TYPE-SPECIFIC COST HANDLERS ---

        private double CalculateSmallGridCost(IMyCubeGrid grid)
        {
            float mass = (grid.Physics != null) ? grid.Physics.Mass : 0f;

            // Static/physics-disabled fallback routine for small vehicles
            if (mass <= 0f)
                mass = 15000f; // 15 Metric Tons standard estimate (e.g., standard rover/fighter)

            double costPerKg = 5.0;
            return mass * costPerKg;
        }

        private double CalculateLargeGridCost(IMyCubeGrid grid)
        {
            float mass = (grid.Physics != null) ? grid.Physics.Mass : 0f;

            // Static/physics-disabled fallback routine for large ships
            if (mass <= 0f)
                mass = 500000f; // 500 Metric Tons standard estimate (e.g., medium cargo freighter)

            double costPerKg = 1.5;
            return mass * costPerKg;
        }

        private double CalculateStationCost(IMyCubeGrid grid)
        {
            float mass = (grid.Physics != null) ? grid.Physics.Mass : 0f;

            // Handling routine for unpowered or anchored outposts
            if (mass <= 0f)
            {
                mass = 1200000f; // 1,200 Metric Tons standard estimate (e.g., small industrial outpost)
            }

            double costPerKg = 0.8; // Reduced per-Kg cost due to static placement...
            double stationTax = 75000.0; // ...balanced by a fixed administrative grid occupancy fee

            return (mass * costPerKg) + stationTax;
        }

        private void OnSalvageScreenClosed(ResultEnum result, long playerId, long finalCost, IMyCubeGrid grid)
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

            // 1. BANKING OPERATIONS (TryGetBalanceInfo API)
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
                MyAPIGateway.Utilities.ShowNotification("Error: Could not retrieve balance info.", 5000, MyFontEnum.Red);
                return;
            }

            // 2. GRID OWNERSHIP TRANSFER SYSTEM OVER THE ENTIRE TREE
            System.Collections.Generic.List<IMyCubeGrid> gridsToTransfer = new System.Collections.Generic.List<IMyCubeGrid>();
            var transferGroup = MyAPIGateway.GridGroups.GetGridGroup(GridLinkTypeEnum.Physical, grid);

            if (transferGroup != null)
            {
                transferGroup.GetGrids(gridsToTransfer);
            }
            else
            {
                gridsToTransfer.Add(grid);
            }

            // Apply absolute ownership rewrite across all sub-grids synchronously
            foreach (var subGrid in gridsToTransfer)
            {
                subGrid.ChangeGridOwnership(playerId, MyOwnershipShareModeEnum.None);
            }

            // 3. VISUAL FEEDBACK
            ShowPopupMessage("SALVAGE PERMIT GRANTED", $"Transaction successful!\n{finalCost.ToString("N0")} SC has been deducted from your balance.\n\nThe entire grid structure is now legally registered under your ownership.");
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
    }
}