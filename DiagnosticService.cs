using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;
using Draygo.API;

namespace SalvageMod
{
    /// <summary>
    /// Manages the real-time HUD Diagnostic Overlay, displaying dynamic license fee breakdowns
    /// when the player looks at NPC wrecks. Under development, adhering to Single Responsibility Principle.
    /// </summary>
    public class DiagnosticService
    {
        private HudAPIv2.HUDMessage _hudMessage;
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly Vector2D _screenPosition = new Vector2D(-0.95, 0.3); // Top-left quadrant of the screen

        /// <summary>
        /// Updates the HUD diagnostic overlay.
        /// Executes a client-side raycast and displays real-time pricing and telemetry of the targeted grid.
        /// If the option is disabled, hides the HUD message and exits immediately to minimize engine load.
        /// </summary>
        public void Update()
        {
            var mainInstance = SalvageMain.Instance;
            if (mainInstance == null) return;

            var config = mainInstance.Config;
            if (config == null) return;

            // If the feature is disabled by the user via F2 menu, clear and skip execution entirely
            if (!config.EnableDiagnosticOverlay)
            {
                Hide();
                return;
            }

            var hudApi = mainInstance.HudApi;
            if (hudApi == null || !hudApi.Heartbeat)
            {
                return;
            }

            // Client-side camera raycast to locate grid target
            IMyCubeGrid targetGrid = GetTargetGridFromCamera(config.RaycastRange);
            if (targetGrid == null)
            {
                Hide();
                return;
            }

            // Create HUDMessage instance if it does not exist yet
            if (_hudMessage == null)
            {
                _hudMessage = new HudAPIv2.HUDMessage(
                    _sb, 
                    _screenPosition, 
                    Scale: 0.8d, 
                    HideHud: true, 
                    Shadowing: true, 
                    ShadowColor: Color.Black, 
                    Font: "white"
                );
            }

            // Compile information and update the overlay text builder
            BuildOverlayText(targetGrid, config);
            _hudMessage.Visible = true;
        }

        /// <summary>
        /// Builds the text structure for the HUD overlay based on the target grid's ownership and state.
        /// </summary>
        private void BuildOverlayText(IMyCubeGrid targetGrid, SalvageConfig config)
        {
            _sb.Clear();
            _sb.AppendLine("<color=yellow>=== WRECK SALVAGE DIAGNOSTIC ===</color>");
            _sb.Append("Grid Name: ").AppendLine(targetGrid.DisplayName);

            // 1. Unowned Wreck Verification
            if (targetGrid.BigOwners == null || targetGrid.BigOwners.Count == 0)
            {
                _sb.AppendLine("Ownership: <color=gray>Unowned</color>");
                _sb.AppendLine("Salvage Status: <color=green>FREE TO SALVAGE</color>");
                _sb.AppendLine("================================");
                return;
            }

            long playerId = MyAPIGateway.Session.Player.IdentityId;
            long ownerId = targetGrid.BigOwners[0];

            // 2. Self-ownership Filter
            if (ownerId == playerId)
            {
                _sb.AppendLine("Ownership: <color=green>Self-Owned</color>");
                _sb.AppendLine("Salvage Status: <color=green>No Permit Needed</color>");
                _sb.AppendLine("================================");
                return;
            }

            IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(ownerId);
            if (ownerFaction == null)
            {
                _sb.AppendLine("Ownership: <color=red>Unknown Owner</color>");
                _sb.AppendLine("Salvage Status: <color=red>Negotiations Unavailable</color>");
                _sb.AppendLine("================================");
                return;
            }

            // 3. Faction Verification (Real players vs NPC factions)
            if (!ownerFaction.IsEveryoneNpc())
            {
                _sb.Append("Owner Faction: ").AppendLine(ownerFaction.Tag);
                _sb.AppendLine("Faction Type: <color=red>Player Faction</color>");
                _sb.AppendLine("Salvage Status: <color=red>Permit Unavailable</color>");
                _sb.AppendLine("================================");
                return;
            }

            // 4. Reputation Standing Check
            int reputation = MyAPIGateway.Session.Factions.GetReputationBetweenPlayerAndFaction(playerId, ownerFaction.FactionId);
            _sb.Append("Faction: ").Append(ownerFaction.Name).Append(" [").Append(ownerFaction.Tag).AppendLine("]");
            _sb.Append("Reputation: ").Append(reputation.ToString());

            if (reputation < config.ReputationThreshold)
            {
                _sb.AppendLine(" <color=red>(Hostile)</color>");
                _sb.AppendLine("Salvage Status: <color=red>REFUSED (Hostile Standing)</color>");
                _sb.AppendLine("================================");
                return;
            }
            else if (reputation < 500)
            {
                _sb.AppendLine(" <color=orange>(Neutral)</color>");
            }
            else
            {
                _sb.AppendLine(" <color=green>(Friendly)</color>");
            }

            // 5. Dynamic economic telemetry extraction (Reusing core logic)
            int subGridCount;
            double totalMass;
            double estimatedFee = SalvageMain.Instance.CalculateTotalStructureData(targetGrid, ownerFaction, out subGridCount, out totalMass);

            // Compute reputation scale coefficient matching standard transaction formulas
            double repModifier = 1.0;
            if (reputation >= 0)
            {
                // Positive standing discounts costs down to MinReputationModifier
                double discountFactor = (double)reputation / 1500.0;
                repModifier = Math.Max(config.MinReputationModifier, 1.0 - discountFactor);
            }
            else
            {
                // Negative standing increases costs up to MaxReputationModifier
                double penaltyFactor = (double)Math.Abs(reputation) / (double)Math.Abs(config.ReputationThreshold);
                repModifier = Math.Min(config.MaxReputationModifier, 1.0 + (penaltyFactor * (config.MaxReputationModifier - 1.0)));
            }

            long finalCost = (long)Math.Round(estimatedFee * repModifier);

            _sb.Append("Total Mass: ").Append(totalMass.ToString("N0")).AppendLine(" kg");
            _sb.Append("Sub-grids: ").AppendLine(subGridCount.ToString());
            _sb.Append("Licensing Fee: <color=green>").Append(finalCost.ToString("N0")).AppendLine(" SC</color>");
            _sb.AppendLine("================================");
        }

        /// <summary>
        /// Client-side raycast using camera matrix to look for targeted grid.
        /// </summary>
        private IMyCubeGrid GetTargetGridFromCamera(double maxDistance)
        {
            IMyCharacter character = MyAPIGateway.Session?.Player?.Character;
            if (character == null) return null;

            MatrixD cameraMatrix = MyAPIGateway.Session.Camera.WorldMatrix;
            Vector3D origin = cameraMatrix.Translation;
            Vector3D direction = cameraMatrix.Forward;
            Vector3D targetPoint = origin + (direction * maxDistance);

            IHitInfo hitInfo;
            MyAPIGateway.Physics.CastRay(origin, targetPoint, out hitInfo);

            if (hitInfo != null && hitInfo.HitEntity is IMyCubeGrid)
            {
                return (IMyCubeGrid)hitInfo.HitEntity;
            }

            return null;
        }

        /// <summary>
        /// Hides the HUD message and clears content.
        /// </summary>
        public void Hide()
        {
            if (_hudMessage != null)
            {
                _hudMessage.Visible = false;
            }
        }

        /// <summary>
        /// Cleans up Draygo API elements to prevent leaks.
        /// </summary>
        public void Close()
        {
            if (_hudMessage != null)
            {
                _hudMessage.DeleteMessage();
                _hudMessage = null;
            }
        }
    }
}
