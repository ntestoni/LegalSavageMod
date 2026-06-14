using System;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Utils;

namespace SalvageMod
{
    public class SalvageConfig
    {
        private const string FileName = "SalvageConfig.ini";

        // Section names within the INI file
        private const string SectionGeneral = "GeneralSettings";
        private const string SectionGridScales = "GridMassMultipliers";
        private const string SectionBasePricing = "BaseGridPricing";
        private const string SectionPrices = "SpecialBlockPrices";
        private const string SectionMilitary = "MilitaryAndWeaponPrices";
        private const string SectionAutomation = "AutomationAndUtilityPrices";

        // --- SECTION: GeneralSettings ---
        public double RaycastRange { get; private set; } = 50.0;
        public int ReputationThreshold { get; private set; } = -500;
        public double MinReputationModifier { get; private set; } = 0.5;
        public double MaxReputationModifier { get; private set; } = 3.0;
        public double ArticulatedAssemblyTax { get; private set; } = 2500.0;
        public double StationTax { get; private set; } = 75000.0;

        // --- SECTION: GridMassMultipliers (Preserved from layout) ---
        public double SmallGridScale { get; private set; } = 0.5;
        public double LargeGridScale { get; private set; } = 2.0;
        public double StationScale { get; private set; } = 5.0;

        // --- SECTION: BaseGridPricing ---
        public double DefaultGridMass { get; private set; } = 5000.0;
        public double DefaultGridCost { get; private set; } = 5000.0;
        public double SmallGridCostPerKg { get; private set; } = 5.0;
        public double SmallGridFallbackMass { get; private set; } = 15000.0;
        public double LargeGridCostPerKg { get; private set; } = 1.5;
        public double LargeGridFallbackMass { get; private set; } = 500000.0;
        public double StationCostPerKg { get; private set; } = 0.8;
        public double StationFallbackMass { get; private set; } = 1200000.0;

        // --- SECTION: SpecialBlockPrices ---
        public double PriceReactor { get; private set; } = 500000;
        public double PriceJumpDrive { get; private set; } = 800000;
        public double PriceRefinery { get; private set; } = 600000;
        public double PriceAssembler { get; private set; } = 300000;
        public double PriceBattery { get; private set; } = 100000;
        public double PriceSolarPanel { get; private set; } = 10000.0;
        public double PriceGasGenerator { get; private set; } = 20000.0;
        public double PriceGasTank { get; private set; } = 15000.0;
        public double PriceCargoContainer { get; private set; } = 25000.0;
        public double PriceConveyorSorter { get; private set; } = 12000.0;
        public double PriceShipConnector { get; private set; } = 8000.0;

        // --- SECTION: MilitaryAndWeaponPrices ---
        public double PriceTurret { get; private set; } = 150000;
        public double PriceUserControllableGun { get; private set; } = 15000.0;
        public double PriceWarhead { get; private set; } = 50000.0;

        // --- SECTION: AutomationAndUtilityPrices ---
        public double PriceCockpit { get; private set; } = 20000.0;
        public double PriceRemoteControl { get; private set; } = 15000.0;
        public double PriceBeacon { get; private set; } = 10000.0;
        public double PriceRadioAntenna { get; private set; } = 12000.0;
        public double PriceProjector { get; private set; } = 30000.0;
        public double PriceProgrammableBlock { get; private set; } = 40000.0;
        public double PriceTimerBlock { get; private set; } = 5000.0;
        public double PriceSensorBlock { get; private set; } = 5000.0;
        public double PriceMedicalRoom { get; private set; } = 60000.0;
        public double PriceAirVent { get; private set; } = 4000.0;
        public double PriceControlPanel { get; private set; } = 1500.0;
        public double PriceButtonPanel { get; private set; } = 2500.0;

        /// <summary>
        /// Loads config from the world storage directory using sandboxed I/O and MyIni parser.
        /// Generates a default configuration file template if it does not exist on disk.
        /// </summary>
        public void LoadOrCreateConfig()
        {
            try
            {
                var iniParser = new MyIni();

                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(FileName, typeof(SalvageConfig)))
                {
                    using (TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(FileName, typeof(SalvageConfig)))
                    {
                        string fileContent = reader.ReadToEnd();
                        MyIniParseResult result;

                        if (iniParser.TryParse(fileContent, out result))
                        {
                            // SectionGeneral parsing
                            RaycastRange = iniParser.Get(SectionGeneral, "RaycastRange").ToDouble(RaycastRange);
                            ReputationThreshold = iniParser.Get(SectionGeneral, "ReputationThreshold").ToInt32(ReputationThreshold);
                            MinReputationModifier = iniParser.Get(SectionGeneral, "MinReputationModifier").ToDouble(MinReputationModifier);
                            MaxReputationModifier = iniParser.Get(SectionGeneral, "MaxReputationModifier").ToDouble(MaxReputationModifier);
                            ArticulatedAssemblyTax = iniParser.Get(SectionGeneral, "ArticulatedAssemblyTax").ToDouble(ArticulatedAssemblyTax);
                            StationTax = iniParser.Get(SectionGeneral, "StationTax").ToDouble(StationTax);

                            // SectionGridScales parsing
                            SmallGridScale = iniParser.Get(SectionGridScales, "SmallGridMultiplier").ToDouble(SmallGridScale);
                            LargeGridScale = iniParser.Get(SectionGridScales, "LargeGridMultiplier").ToDouble(LargeGridScale);
                            StationScale = iniParser.Get(SectionGridScales, "StationMultiplier").ToDouble(StationScale);

                            // SectionBasePricing parsing
                            DefaultGridMass = iniParser.Get(SectionBasePricing, "DefaultGridMass").ToDouble(DefaultGridMass);
                            DefaultGridCost = iniParser.Get(SectionBasePricing, "DefaultGridCost").ToDouble(DefaultGridCost);
                            SmallGridCostPerKg = iniParser.Get(SectionBasePricing, "SmallGridCostPerKg").ToDouble(SmallGridCostPerKg);
                            SmallGridFallbackMass = iniParser.Get(SectionBasePricing, "SmallGridFallbackMass").ToDouble(SmallGridFallbackMass);
                            LargeGridCostPerKg = iniParser.Get(SectionBasePricing, "LargeGridCostPerKg").ToDouble(LargeGridCostPerKg);
                            LargeGridFallbackMass = iniParser.Get(SectionBasePricing, "LargeGridFallbackMass").ToDouble(LargeGridFallbackMass);
                            StationCostPerKg = iniParser.Get(SectionBasePricing, "StationCostPerKg").ToDouble(StationCostPerKg);
                            StationFallbackMass = iniParser.Get(SectionBasePricing, "StationFallbackMass").ToDouble(StationFallbackMass);

                            // SectionPrices parsing
                            PriceReactor = iniParser.Get(SectionPrices, "ReactorBasePrice").ToDouble(PriceReactor);
                            PriceJumpDrive = iniParser.Get(SectionPrices, "JumpDriveBasePrice").ToDouble(PriceJumpDrive);
                            PriceRefinery = iniParser.Get(SectionPrices, "RefineryBasePrice").ToDouble(PriceRefinery);
                            PriceAssembler = iniParser.Get(SectionPrices, "AssemblerBasePrice").ToDouble(PriceAssembler);
                            PriceBattery = iniParser.Get(SectionPrices, "BatteryBasePrice").ToDouble(PriceBattery);
                            PriceSolarPanel = iniParser.Get(SectionPrices, "SolarPanelBasePrice").ToDouble(PriceSolarPanel);
                            PriceGasGenerator = iniParser.Get(SectionPrices, "GasGeneratorBasePrice").ToDouble(PriceGasGenerator);
                            PriceGasTank = iniParser.Get(SectionPrices, "GasTankBasePrice").ToDouble(PriceGasTank);
                            PriceCargoContainer = iniParser.Get(SectionPrices, "CargoContainerBasePrice").ToDouble(PriceCargoContainer);
                            PriceConveyorSorter = iniParser.Get(SectionPrices, "ConveyorSorterBasePrice").ToDouble(PriceConveyorSorter);
                            PriceShipConnector = iniParser.Get(SectionPrices, "ShipConnectorBasePrice").ToDouble(PriceShipConnector);

                            // SectionMilitary parsing
                            PriceTurret = iniParser.Get(SectionMilitary, "TurretBasePrice").ToDouble(PriceTurret);
                            PriceUserControllableGun = iniParser.Get(SectionMilitary, "UserControllableGunBasePrice").ToDouble(PriceUserControllableGun);
                            PriceWarhead = iniParser.Get(SectionMilitary, "WarheadBasePrice").ToDouble(PriceWarhead);

                            // SectionAutomation parsing
                            PriceCockpit = iniParser.Get(SectionAutomation, "CockpitBasePrice").ToDouble(PriceCockpit);
                            PriceRemoteControl = iniParser.Get(SectionAutomation, "RemoteControlBasePrice").ToDouble(PriceRemoteControl);
                            PriceBeacon = iniParser.Get(SectionAutomation, "BeaconBasePrice").ToDouble(PriceBeacon);
                            PriceRadioAntenna = iniParser.Get(SectionAutomation, "RadioAntennaBasePrice").ToDouble(PriceRadioAntenna);
                            PriceProjector = iniParser.Get(SectionAutomation, "ProjectorBasePrice").ToDouble(PriceProjector);
                            PriceProgrammableBlock = iniParser.Get(SectionAutomation, "ProgrammableBlockBasePrice").ToDouble(PriceProgrammableBlock);
                            PriceTimerBlock = iniParser.Get(SectionAutomation, "TimerBlockBasePrice").ToDouble(PriceTimerBlock);
                            PriceSensorBlock = iniParser.Get(SectionAutomation, "SensorBlockBasePrice").ToDouble(PriceSensorBlock);
                            PriceMedicalRoom = iniParser.Get(SectionAutomation, "MedicalRoomBasePrice").ToDouble(PriceMedicalRoom);
                            PriceAirVent = iniParser.Get(SectionAutomation, "AirVentBasePrice").ToDouble(PriceAirVent);
                            PriceControlPanel = iniParser.Get(SectionAutomation, "ControlPanelBasePrice").ToDouble(PriceControlPanel);
                            PriceButtonPanel = iniParser.Get(SectionAutomation, "ButtonPanelBasePrice").ToDouble(PriceButtonPanel);

                            MyLog.Default.WriteLineAndConsole("[LegalSavageMod] Configuration file loaded and extended successfully.");
                            return;
                        }
                        else
                        {
                            MyLog.Default.WriteLineAndConsole($"[LegalSavageMod] Config syntax error at line {result.LineNo}: {result.Error}. Regenerating profile.");
                        }
                    }
                }

                WriteConfig(iniParser);
            }
            catch (Exception ex)
            {
                MyLog.Default.WriteLineAndConsole($"[LegalSavageMod] Critical error in configuration load sub-system: {ex.Message}");
            }
        }

        /// <summary>
        /// Serializes the current memory properties into the MyIni profile and dumps it safely onto disk.
        /// </summary>
        /// <param name="iniParser">The tracking MyIni instance used to format the structural key-value text.</param>
        private void WriteConfig(MyIni iniParser)
        {
            if (iniParser == null)
                iniParser = new MyIni();

            // SectionGeneral serialization
            iniParser.Set(SectionGeneral, "RaycastRange", RaycastRange);
            iniParser.Set(SectionGeneral, "ReputationThreshold", ReputationThreshold);
            iniParser.Set(SectionGeneral, "MinReputationModifier", MinReputationModifier);
            iniParser.Set(SectionGeneral, "MaxReputationModifier", MaxReputationModifier);
            iniParser.Set(SectionGeneral, "ArticulatedAssemblyTax", ArticulatedAssemblyTax);
            iniParser.Set(SectionGeneral, "StationTax", StationTax);

            // SectionGridScales serialization
            iniParser.Set(SectionGridScales, "SmallGridMultiplier", SmallGridScale);
            iniParser.Set(SectionGridScales, "LargeGridMultiplier", LargeGridScale);
            iniParser.Set(SectionGridScales, "StationMultiplier", StationScale);

            // SectionBasePricing serialization
            iniParser.Set(SectionBasePricing, "DefaultGridMass", DefaultGridMass);
            iniParser.Set(SectionBasePricing, "DefaultGridCost", DefaultGridCost);
            iniParser.Set(SectionBasePricing, "SmallGridCostPerKg", SmallGridCostPerKg);
            iniParser.Set(SectionBasePricing, "SmallGridFallbackMass", SmallGridFallbackMass);
            iniParser.Set(SectionBasePricing, "LargeGridCostPerKg", LargeGridCostPerKg);
            iniParser.Set(SectionBasePricing, "LargeGridFallbackMass", LargeGridFallbackMass);
            iniParser.Set(SectionBasePricing, "StationCostPerKg", StationCostPerKg);
            iniParser.Set(SectionBasePricing, "StationFallbackMass", StationFallbackMass);

            // SectionPrices serialization
            iniParser.Set(SectionPrices, "ReactorBasePrice", PriceReactor);
            iniParser.Set(SectionPrices, "JumpDriveBasePrice", PriceJumpDrive);
            iniParser.Set(SectionPrices, "RefineryBasePrice", PriceRefinery);
            iniParser.Set(SectionPrices, "AssemblerBasePrice", PriceAssembler);
            iniParser.Set(SectionPrices, "BatteryBasePrice", PriceBattery);
            iniParser.Set(SectionPrices, "SolarPanelBasePrice", PriceSolarPanel);
            iniParser.Set(SectionPrices, "GasGeneratorBasePrice", PriceGasGenerator);
            iniParser.Set(SectionPrices, "GasTankBasePrice", PriceGasTank);
            iniParser.Set(SectionPrices, "CargoContainerBasePrice", PriceCargoContainer);
            iniParser.Set(SectionPrices, "ConveyorSorterBasePrice", PriceConveyorSorter);
            iniParser.Set(SectionPrices, "ShipConnectorBasePrice", PriceShipConnector);

            // SectionMilitary serialization
            iniParser.Set(SectionMilitary, "TurretBasePrice", PriceTurret);
            iniParser.Set(SectionMilitary, "UserControllableGunBasePrice", PriceUserControllableGun);
            iniParser.Set(SectionMilitary, "WarheadBasePrice", PriceWarhead);

            // SectionAutomation serialization
            iniParser.Set(SectionAutomation, "CockpitBasePrice", PriceCockpit);
            iniParser.Set(SectionAutomation, "RemoteControlBasePrice", PriceRemoteControl);
            iniParser.Set(SectionAutomation, "BeaconBasePrice", PriceBeacon);
            iniParser.Set(SectionAutomation, "RadioAntennaBasePrice", PriceRadioAntenna);
            iniParser.Set(SectionAutomation, "ProjectorBasePrice", PriceProjector);
            iniParser.Set(SectionAutomation, "ProgrammableBlockBasePrice", PriceProgrammableBlock);
            iniParser.Set(SectionAutomation, "TimerBlockBasePrice", PriceTimerBlock);
            iniParser.Set(SectionAutomation, "SensorBlockBasePrice", PriceSensorBlock);
            iniParser.Set(SectionAutomation, "MedicalRoomBasePrice", PriceMedicalRoom);
            iniParser.Set(SectionAutomation, "AirVentBasePrice", PriceAirVent);
            iniParser.Set(SectionAutomation, "ControlPanelBasePrice", PriceControlPanel);
            iniParser.Set(SectionAutomation, "ButtonPanelBasePrice", PriceButtonPanel);

            string outputText = iniParser.ToString();
            using (TextWriter writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(FileName, typeof(SalvageConfig)))
            {
                writer.Write(outputText);
            }

            MyLog.Default.WriteLineAndConsole("[LegalSavageMod] Configuration disk profile extended and synchronized.");
        }
    }
}