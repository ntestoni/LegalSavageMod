using System;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Utils;
using Draygo.API;

namespace SalvageMod
{
    /// <summary>
    /// Manages the construction and registration of the F2 in-game configuration menu
    /// via Draygo's Text HUD API. Isolated from the core gameplay loop of SalvageMain
    /// to improve code maintainability and separation of concerns.
    /// </summary>
    public class SalvageMenuConfig
    {
        // Reference to the shared configuration instance for reading and updating settings
        private readonly SalvageConfig _modConfig;

        /// <summary>
        /// Creates a new menu configuration manager bound to the given config object.
        /// </summary>
        /// <param name="modConfig">The shared mod configuration instance.</param>
        public SalvageMenuConfig(SalvageConfig modConfig)
        {
            _modConfig = modConfig;
        }

        /// <summary>
        /// Instantiates all root categories, subcategories, and setting input fields
        /// within the HudAPI F2 configuration menu. Called as the HudAPIv2 registration callback.
        /// </summary>
        public void CreateModMenu()
        {
            try
            {
                // Root Categories
                var adminMenuRoot = new HudAPIv2.MenuRootCategory("Legal Salvage Mod", HudAPIv2.MenuRootCategory.MenuFlag.AdminMenu, "Legal Salvage - Admin Configuration");
                var playerMenuRoot = new HudAPIv2.MenuRootCategory("Legal Salvage Mod", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Legal Salvage Options");

                // Player Options
                CreateBoolToggle("HUD Diagnostic Overlay", "GeneralSettings", "EnableDiagnosticOverlay", () => _modConfig.EnableDiagnosticOverlay, playerMenuRoot);

                // Admin Sub-Categories
                var catGeneral = new HudAPIv2.MenuSubCategory("General Settings", adminMenuRoot, "General Settings");
                var catMultipliers = new HudAPIv2.MenuSubCategory("Grid Mass Multipliers", adminMenuRoot, "Grid Mass Multipliers");
                var catBasePricing = new HudAPIv2.MenuSubCategory("Base Grid Pricing", adminMenuRoot, "Base Grid Pricing & Fallbacks");
                var catSpecialPrices = new HudAPIv2.MenuSubCategory("Special Block Prices", adminMenuRoot, "Special Block Surcharges");
                var catMilitary = new HudAPIv2.MenuSubCategory("Military & Weapon Prices", adminMenuRoot, "Military and Weapon Surcharges");
                var catAutomation = new HudAPIv2.MenuSubCategory("Automation & Utility Prices", adminMenuRoot, "Automation and Utility Surcharges");

                // 1. General Settings (6 parameters)
                CreateDoubleInput("Raycast Range", "GeneralSettings", "RaycastRange", () => _modConfig.RaycastRange, catGeneral, " m");
                CreateIntInput("Reputation Threshold", "GeneralSettings", "ReputationThreshold", () => _modConfig.ReputationThreshold, catGeneral);
                CreateDoubleInput("Min Reputation Modifier", "GeneralSettings", "MinReputationModifier", () => _modConfig.MinReputationModifier, catGeneral, "x");
                CreateDoubleInput("Max Reputation Modifier", "GeneralSettings", "MaxReputationModifier", () => _modConfig.MaxReputationModifier, catGeneral, "x");
                CreateDoubleInput("Articulated Assembly Tax", "GeneralSettings", "ArticulatedAssemblyTax", () => _modConfig.ArticulatedAssemblyTax, catGeneral, " SC");
                CreateDoubleInput("Station Tax", "GeneralSettings", "StationTax", () => _modConfig.StationTax, catGeneral, " SC");

                // 2. Grid Mass Multipliers (3 parameters)
                CreateDoubleInput("Small Grid Multiplier", "GridMassMultipliers", "SmallGridMultiplier", () => _modConfig.SmallGridScale, catMultipliers, "x");
                CreateDoubleInput("Large Grid Multiplier", "GridMassMultipliers", "LargeGridMultiplier", () => _modConfig.LargeGridScale, catMultipliers, "x");
                CreateDoubleInput("Station Multiplier", "GridMassMultipliers", "StationMultiplier", () => _modConfig.StationScale, catMultipliers, "x");

                // 3. Base Grid Pricing (8 parameters)
                CreateDoubleInput("Default Grid Mass", "BaseGridPricing", "DefaultGridMass", () => _modConfig.DefaultGridMass, catBasePricing, " Kg");
                CreateDoubleInput("Default Grid Cost", "BaseGridPricing", "DefaultGridCost", () => _modConfig.DefaultGridCost, catBasePricing, " SC");
                CreateDoubleInput("Small Grid Cost Per Kg", "BaseGridPricing", "SmallGridCostPerKg", () => _modConfig.SmallGridCostPerKg, catBasePricing, " SC/Kg");
                CreateDoubleInput("Small Grid Fallback Mass", "BaseGridPricing", "SmallGridFallbackMass", () => _modConfig.SmallGridFallbackMass, catBasePricing, " Kg");
                CreateDoubleInput("Large Grid Cost Per Kg", "BaseGridPricing", "LargeGridCostPerKg", () => _modConfig.LargeGridCostPerKg, catBasePricing, " SC/Kg");
                CreateDoubleInput("Large Grid Fallback Mass", "BaseGridPricing", "LargeGridFallbackMass", () => _modConfig.LargeGridFallbackMass, catBasePricing, " Kg");
                CreateDoubleInput("Station Cost Per Kg", "BaseGridPricing", "StationCostPerKg", () => _modConfig.StationCostPerKg, catBasePricing, " SC/Kg");
                CreateDoubleInput("Station Fallback Mass", "BaseGridPricing", "StationFallbackMass", () => _modConfig.StationFallbackMass, catBasePricing, " Kg");

                // 4. Special Block Prices (11 parameters)
                CreateDoubleInput("Reactor Price", "SpecialBlockPrices", "ReactorBasePrice", () => _modConfig.PriceReactor, catSpecialPrices, " SC");
                CreateDoubleInput("Jump Drive Price", "SpecialBlockPrices", "JumpDriveBasePrice", () => _modConfig.PriceJumpDrive, catSpecialPrices, " SC");
                CreateDoubleInput("Refinery Price", "SpecialBlockPrices", "RefineryBasePrice", () => _modConfig.PriceRefinery, catSpecialPrices, " SC");
                CreateDoubleInput("Assembler Price", "SpecialBlockPrices", "AssemblerBasePrice", () => _modConfig.PriceAssembler, catSpecialPrices, " SC");
                CreateDoubleInput("Battery Price", "SpecialBlockPrices", "BatteryBasePrice", () => _modConfig.PriceBattery, catSpecialPrices, " SC");
                CreateDoubleInput("Solar Panel Price", "SpecialBlockPrices", "SolarPanelBasePrice", () => _modConfig.PriceSolarPanel, catSpecialPrices, " SC");
                CreateDoubleInput("Gas Generator Price", "SpecialBlockPrices", "GasGeneratorBasePrice", () => _modConfig.PriceGasGenerator, catSpecialPrices, " SC");
                CreateDoubleInput("Gas Tank Price", "SpecialBlockPrices", "GasTankBasePrice", () => _modConfig.PriceGasTank, catSpecialPrices, " SC");
                CreateDoubleInput("Cargo Container Price", "SpecialBlockPrices", "CargoContainerBasePrice", () => _modConfig.PriceCargoContainer, catSpecialPrices, " SC");
                CreateDoubleInput("Conveyor Sorter Price", "SpecialBlockPrices", "ConveyorSorterBasePrice", () => _modConfig.PriceConveyorSorter, catSpecialPrices, " SC");
                CreateDoubleInput("Ship Connector Price", "SpecialBlockPrices", "ShipConnectorBasePrice", () => _modConfig.PriceShipConnector, catSpecialPrices, " SC");

                // 5. Military & Weapon Prices (3 parameters)
                CreateDoubleInput("Turret Price", "MilitaryAndWeaponPrices", "TurretBasePrice", () => _modConfig.PriceTurret, catMilitary, " SC");
                CreateDoubleInput("User Controllable Gun Price", "MilitaryAndWeaponPrices", "UserControllableGunBasePrice", () => _modConfig.PriceUserControllableGun, catMilitary, " SC");
                CreateDoubleInput("Warhead Price", "MilitaryAndWeaponPrices", "WarheadBasePrice", () => _modConfig.PriceWarhead, catMilitary, " SC");

                // 6. Automation & Utility Prices (12 parameters)
                CreateDoubleInput("Cockpit Price", "AutomationAndUtilityPrices", "CockpitBasePrice", () => _modConfig.PriceCockpit, catAutomation, " SC");
                CreateDoubleInput("Remote Control Price", "AutomationAndUtilityPrices", "RemoteControlBasePrice", () => _modConfig.PriceRemoteControl, catAutomation, " SC");
                CreateDoubleInput("Beacon Price", "AutomationAndUtilityPrices", "BeaconBasePrice", () => _modConfig.PriceBeacon, catAutomation, " SC");
                CreateDoubleInput("Radio Antenna Price", "AutomationAndUtilityPrices", "RadioAntennaBasePrice", () => _modConfig.PriceRadioAntenna, catAutomation, " SC");
                CreateDoubleInput("Projector Price", "AutomationAndUtilityPrices", "ProjectorBasePrice", () => _modConfig.PriceProjector, catAutomation, " SC");
                CreateDoubleInput("Programmable Block Price", "AutomationAndUtilityPrices", "ProgrammableBlockBasePrice", () => _modConfig.PriceProgrammableBlock, catAutomation, " SC");
                CreateDoubleInput("Timer Block Price", "AutomationAndUtilityPrices", "TimerBlockBasePrice", () => _modConfig.PriceTimerBlock, catAutomation, " SC");
                CreateDoubleInput("Sensor Block Price", "AutomationAndUtilityPrices", "SensorBlockBasePrice", () => _modConfig.PriceSensorBlock, catAutomation, " SC");
                CreateDoubleInput("Medical Room Price", "AutomationAndUtilityPrices", "MedicalRoomBasePrice", () => _modConfig.PriceMedicalRoom, catAutomation, " SC");
                CreateDoubleInput("Air Vent Price", "AutomationAndUtilityPrices", "AirVentBasePrice", () => _modConfig.PriceAirVent, catAutomation, " SC");
                CreateDoubleInput("Control Panel Price", "AutomationAndUtilityPrices", "ControlPanelBasePrice", () => _modConfig.PriceControlPanel, catAutomation, " SC");
                CreateDoubleInput("Button Panel Price", "AutomationAndUtilityPrices", "ButtonPanelBasePrice", () => _modConfig.PriceButtonPanel, catAutomation, " SC");

                MyLog.Default.WriteLineAndConsole("[LegalSavageMod] F2 Mod Menu successfully created.");
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"[LegalSavageMod] Error building F2 Menu structure: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a text input field for a double-precision floating-point setting.
        /// Validates the entered string, updates the config on disk, and refreshes the label.
        /// </summary>
        private HudAPIv2.MenuTextInput CreateDoubleInput(string label, string section, string key, Func<double> getter, HudAPIv2.MenuCategoryBase parent, string suffix = "")
        {
            HudAPIv2.MenuTextInput inputItem = null;
            inputItem = new HudAPIv2.MenuTextInput(
                $"{label}: {getter()}{suffix}",
                parent,
                $"Enter new value for {label}:",
                (enteredText) =>
                {
                    double val;
                    if (double.TryParse(enteredText, out val))
                    {
                        _modConfig.UpdateSetting(section, key, val);
                        inputItem.Text = $"{label}: {getter()}{suffix}";
                        MyAPIGateway.Utilities.ShowNotification($"{label} updated to {getter()}{suffix}", 2000, MyFontEnum.Green);
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowNotification("Error: Invalid numeric value.", 3000, MyFontEnum.Red);
                    }
                }
            );
            return inputItem;
        }

        /// <summary>
        /// Creates a text input field for an integer setting.
        /// Validates the entered string, updates the config on disk, and refreshes the label.
        /// </summary>
        private HudAPIv2.MenuTextInput CreateIntInput(string label, string section, string key, Func<int> getter, HudAPIv2.MenuCategoryBase parent, string suffix = "")
        {
            HudAPIv2.MenuTextInput inputItem = null;
            inputItem = new HudAPIv2.MenuTextInput(
                $"{label}: {getter()}{suffix}",
                parent,
                $"Enter new value for {label}:",
                (enteredText) =>
                {
                    int val;
                    if (int.TryParse(enteredText, out val))
                    {
                        _modConfig.UpdateSetting(section, key, val);
                        inputItem.Text = $"{label}: {getter()}{suffix}";
                        MyAPIGateway.Utilities.ShowNotification($"{label} updated to {getter()}{suffix}", 2000, MyFontEnum.Green);
                    }
                    else
                    {
                        MyAPIGateway.Utilities.ShowNotification("Error: Invalid integer value.", 3000, MyFontEnum.Red);
                    }
                }
            );
            return inputItem;
        }

        /// <summary>
        /// Creates a clickable toggle menu item for a boolean setting.
        /// Each click inverts the current value, updates the config on disk, and refreshes the label.
        /// </summary>
        private HudAPIv2.MenuItem CreateBoolToggle(string label, string section, string key, Func<bool> getter, HudAPIv2.MenuCategoryBase parent)
        {
            HudAPIv2.MenuItem toggleItem = null;
            toggleItem = new HudAPIv2.MenuItem(
                $"{label}: {(getter() ? "ON" : "OFF")}",
                parent,
                () =>
                {
                    bool newVal = !getter();
                    _modConfig.UpdateSetting(section, key, newVal);
                    toggleItem.Text = $"{label}: {(getter() ? "ON" : "OFF")}";
                    MyAPIGateway.Utilities.ShowNotification($"{label} set to {(getter() ? "ON" : "OFF")}", 2000, MyFontEnum.Green);
                }
            );
            return toggleItem;
        }
    }
}
