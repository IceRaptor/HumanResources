using BattleTech;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HumanResources
{

    public class DistributionOpts
    {
        // Defines how broad the grouping is, which influences the height of the curve.
        public float Sigma = 1;
        // Where on the number line the curve is centered, i.e. the tallest point of the curve
        public float Mu = 0;

        // Breakpoints on the PDF (Probability distribution function), from worst to best. Must be 4 values.
        public float[] Breakpoints = new float[] { };

        // Distribution mu adjustments based upon planet tag
        public Dictionary<string, CrewTagModifier> PlanetTagModifiers = new Dictionary<string, CrewTagModifier>() { };
    }

    public class CrewOpts
    {
        public bool Enabled = true;

        public int BaseDaysInContract = 15;
        public int MinContractDaysMulti = 6;
        public int MaxContractDaysMulti = 12;

        // ab^x where a = multiplier, b = exponent, x = skill level
        public int SalaryMulti = 30000;
        public float SalaryExponent = 1.1f;
        
        // Salary variance is up or down; if salary is 10,000 variance will calculate from 9,500 to 10,500
        public float SalaryVariance = 0.05f;
        // Bonus variance is always above the salary range; 10,000 salary will be between 10,000 and 13,000
        public float BonusVariance = 1.3f;

        // The percentage of salary that gets applied as hazard pay
        public float HazardPayRatio = 0.05f;

        public int HazardPayUnits = 500;

        // Value by MRB level. Should be one less than MRBRepCap in SimGameConstants, as you can always hire below the first value
        //                                                  MRB: 1, 2, 3,  4,  5
        public float[] ValueThresholdForMRBLevel = new float[] {};

        // For Combat units only; map skill levels to expertise boundaires (Green, Regular, Veteran, Elite, Legendary)
        // Does not apply to units w/o combat skills
        public float[] SkillToExpertiseThresholds = new float[] {};

        // -1 indicates no limit
        public float MaxOfType = -1;

        // Origin tags that will be added to the crew types
        public string[] originTags = new string[] { };
        public string[] traitTags = new string[] { };
    }

    public class CrewTagModifier
    {
        public float Aerospace = 0f;
        public float MechTechs = 0f;
        public float MechWarriors = 0f;
        public float MedTechs = 0f;
        public float VehicleCrews = 0f;
    }

    public class AttitudeOpts
    {
        public int ThresholdMax = 100;
        public int ThresholdBest = 75;
        public int ThresholdGood = 30;
        public int ThresholdPoor = -30;
        public int ThresholdWorst = -75;
        public int ThresholdMin = -100;

        public float FavoredFactionChance = 0.3f;
        public float HatedFactionChance = 0.3f;

        public List<string> CandidateFactionsFavored = new List<string>();
        [JsonIgnore]
        public List<FactionValue> FavoredFactionCandidates = new List<FactionValue>();

        public List<string> CandidateFactionsHated = new List<string>();
        [JsonIgnore]
        public List<FactionValue> HatedFactionCandidates = new List<FactionValue>();

        // Bonus added when a contract is renewed with the merc
        public int RehireBonusMod = 2;

        public MonthlyAttitudeMods Monthly = new MonthlyAttitudeMods();
        public PerMissionAttitudeMods PerMission = new PerMissionAttitudeMods();
    }

    public class MonthlyAttitudeMods
    {
        public int FavoredEmployerAlliedMod = 6;
        public int HatedEmployerAlliedMod = -30;

        public int EconSpartanMod = -6;
        public int EconRestrictiveMod = -3;
        public int EconomyNormalMod = 0;
        public int EconomyGenerousMod = 2;
        public int EconomyExtravagantMod = 4;

        public float Decay = 0.05f;
    }

    public class PerMissionAttitudeMods
    {
        public int ContractSuccessMod = 1;
        public int ContractFailedGoodFaithMod = -2;
        public int ContractFailedMod = -10;

        public int PilotKilledMod = -10;

        public int DeployedOnMissionMod = 1;
        public int BenchedOnMissionMod = -2;

        public int FavoredFactionIsEmployerMod = 1;
        public int FavoredFactionIsTargetMod = -3;
        public int HatedFactionIsEmployerMod = -3;
        public int HatedFactionIsTargetMod = 3;
    }

    public class LifePathOps
    {
        public double RandomTagChance = 0.2;
    }

    public class ScarcityOps
    {
        public bool Enabled = true;

        public CrewTagModifier Defaults = new CrewTagModifier();

        public Dictionary<string, CrewTagModifier> PlanetTagModifiers = new Dictionary<string, CrewTagModifier>()
        {
        };

    }

    public class HiringHallOpts
    {
        public DistributionOpts SkillDistribution = new DistributionOpts()
        {
            // Rookie, Regular, Veteran, Elite, Legendary
            Breakpoints = new float[] {},
            PlanetTagModifiers = new Dictionary<string, CrewTagModifier>()
        };
        
        public DistributionOpts SizeDistribution = new DistributionOpts()
        {
            // Tiny, Small, Medium, Large, Huge
            Breakpoints = new float[] {},
            PlanetTagModifiers = new Dictionary<string, CrewTagModifier>()
        };

        public LifePathOps LifePath = new LifePathOps();

        public ScarcityOps Scarcity = new ScarcityOps();
        
        public PointsBySkillAndSizeOpts PointsBySkillAndSize = new PointsBySkillAndSizeOpts();

        public CrewOpts AerospaceWings = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = 1,
            HazardPayRatio = 0f
    };

        public CrewOpts MechTechCrews = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = -1,
            HazardPayRatio = 0f
        };

        public CrewOpts MedTechCrews = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = 2,
            HazardPayRatio = 0f
        };

        public CrewOpts MechWarriors = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = -1,
            HazardPayRatio = 0.05f
        };

        public CrewOpts VehicleCrews = new CrewOpts
        {
            Enabled = true,
            BaseDaysInContract = 15,
            MinContractDaysMulti = 6,
            MaxContractDaysMulti = 12,
            SalaryMulti = 30000,
            SalaryExponent = 1.1f,
            SalaryVariance = 1.1f,
            BonusVariance = 1.5f,
            MaxOfType = -1,
            HazardPayRatio = 0.05f
        };

        public float RoninChance = 0.3f;
    }

    public class PointsBySkillAndSizeOpts
    {
        public int[][] Aerospace = new int[][] {};
        public int[][] MechTech = new int[][] {};
        public int[][] MedTech = new int[][] {};
    }

    public class HeadHuntingOpts
    {
        public bool Enabled = true;

        public float[] EconMods = new float[] { };
        public float[] ChanceBySkill = new float[] { };

        // Check for poaching N times per month. On a failed check (nobody was poached, wait this many days)
        public int FailedCooldownIntervalMin = 3;
        public int FailedCooldownIntervalMax = 7;

        // On a successful check, wait this many days
        public int SuccessCooldownIntervalMin = 15;
        public int SuccessCooldownIntervalMax = 45;

        // Crew cannot be attempted to be poached more than 1 time in this period
        public int CrewCooldownIntervalMin = 60;
        public int CrewCooldownIntervalMax = 120;

        // Determine a random amount between the bonus and bonus x CounterOfferVariance to determine is their 'counter-offer' to keep them
        public float CounterOfferVariance = 1.5f;

        public List<string> PlanetBlacklist = new List<string>() { };

        // Poaching goes from most legendary to least ?

        // You pay an additional bonus to keep them AND their re-hire bonus changes to the new amount
        // They leave and the head-hunters pay-back their hiring bonus
        // They leave and pocket the hiring bonus (if disgrunted or less)
    }

    public class CrewCfg
    {
        public float[] AerospaceRGB = { 0.376f, 0.533f, 0.604f };
        public Color AerospaceColor = Color.cyan;

        public float[] MechTechCrewRGB = { 0.808f, 0.71f, 0.278f };
        public Color MechTechCrewColor = Color.yellow;

        public float[] MedTechCrewRGB = { 0.871f, 0.278f, 0.216f };
        public Color MedTechCrewColor = Color.red;

        public float[] VehicleCrewRGB = { 0.486f, 0.745f, 0.525f };
        public Color VehicleCrewColor = Color.green;

        public float[] MechwarriorRGB = { 0.376f, 0.533f, 0.604f };
        public Color MechwarriorColor = Color.blue;

    }

    public class Icons
    {
        public string CrewPortrait_Aerospace = "hr_jet-fighter";
        public string CrewPortrait_MedTech = "hr_hospital-cross";
        public string CrewPortrait_MechTech = "hr_auto-repair";
        public string CrewPortrait_Vehicle = "hr_apc";
    }

    public class ModConfig
    {
        public bool Debug = false;
        public bool Trace = false;

        public bool DebugCommands = true;

        public Icons Icons = new Icons();

        public CrewCfg Crew = new CrewCfg();

        public HiringHallOpts HiringHall = new HiringHallOpts();

        public HeadHuntingOpts HeadHunting = new HeadHuntingOpts();

        public AttitudeOpts Attitude = new AttitudeOpts();

        public void LogConfig()
        {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG: {this.Debug} Trace: {this.Trace}");
            Mod.Log.Info?.Write($"  DebugCommands: {this.DebugCommands}");
            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write("--- Icons ---");
            Mod.Log.Info?.Write($"  CrewPortrait_Aerospace: {this.Icons.CrewPortrait_Aerospace}  CrewPortrait_MedTech: {this.Icons.CrewPortrait_MedTech}");
            Mod.Log.Info?.Write($"  CrewPortrait_MechTech: {this.Icons.CrewPortrait_MechTech}  CrewPortrait_Vehicle: {this.Icons.CrewPortrait_Vehicle}");
            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write("--- Crew Config ---");
            Mod.Log.Info?.Write($"  AerospaceRGB: {this.Crew.AerospaceRGB}  AerospaceColor: {this.Crew.AerospaceColor}");
            Mod.Log.Info?.Write($"  MechTechCrewRGB: {this.Crew.MechTechCrewRGB}  MechTechCrewColor: {this.Crew.MechTechCrewColor}");
            Mod.Log.Info?.Write($"  MedTechCrewRGB: {this.Crew.MedTechCrewRGB}  MedTechCrewColor: {this.Crew.MedTechCrewColor}");
            Mod.Log.Info?.Write($"  VehicleCrewRGB: {this.Crew.VehicleCrewRGB}  VehicleCrewColor: {this.Crew.VehicleCrewColor}");
            Mod.Log.Info?.Write($"  MechwarriorRGB: {this.Crew.MechwarriorRGB}  MechwarriorColor: {this.Crew.MechwarriorColor}");
            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write("--- Hiring Hall Config ---");
            Mod.Log.Info?.Write($"  SkillDistribution: { string.Join(", ", this.HiringHall.SkillDistribution) }");
            Mod.Log.Info?.Write($"  SizeDistribution: { string.Join(", ", this.HiringHall.SizeDistribution) }");
            Mod.Log.Info?.Write($"");
            
            Mod.Log.Info?.Write($"  -- Scarcity --");
            Mod.Log.Info?.Write($"  Enabled: {this.HiringHall.Scarcity.Enabled}");
            Mod.Log.Info?.Write($"  - Default Planet Scarcity -");
            Mod.Log.Info?.Write($"  Aerospace: {this.HiringHall.Scarcity.Defaults.Aerospace}  MechTechs: {this.HiringHall.Scarcity.Defaults.MechTechs}  " +
                $"MedTechs: {this.HiringHall.Scarcity.Defaults.MedTechs}  MechWarriors: {this.HiringHall.Scarcity.Defaults.MechWarriors}  " +
                $"VehicleCrews: {this.HiringHall.Scarcity.Defaults.VehicleCrews} ");
            Mod.Log.Info?.Write($"  - Planet Tag Modifiers -");
            foreach (KeyValuePair<string, CrewTagModifier> kvp in this.HiringHall.Scarcity.PlanetTagModifiers)
            {
                Mod.Log.Info?.Write($"  Tag: {kvp.Key} => Aerospace: {this.HiringHall.Scarcity.Defaults.Aerospace}  MechTechs: {this.HiringHall.Scarcity.Defaults.MechTechs}  " +
                $"MedTechs: {this.HiringHall.Scarcity.Defaults.MedTechs}  MechWarriors: {this.HiringHall.Scarcity.Defaults.MechWarriors}  " +
                $"VehicleCrews: {this.HiringHall.Scarcity.Defaults.VehicleCrews} ");
            }
            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write($"  -- PointsBySkillAndSize --");
            Mod.Log.Info?.Write($"  Aerospace Points");
            for (int i = 0; i < this.HiringHall.PointsBySkillAndSize.Aerospace.Length; i++)
                Mod.Log.Info?.Write("[" + string.Join(", ", this.HiringHall.PointsBySkillAndSize.Aerospace[i]) + "]");
            Mod.Log.Info?.Write($"  MechTech Points");
            for (int i = 0; i < this.HiringHall.PointsBySkillAndSize.MechTech.Length; i++)
                Mod.Log.Info?.Write("[" + string.Join(", ", this.HiringHall.PointsBySkillAndSize.MechTech[i]) + "]");
            Mod.Log.Info?.Write($"  MedTech Points");
            for (int i = 0; i < this.HiringHall.PointsBySkillAndSize.MedTech.Length; i++)
                Mod.Log.Info?.Write("[" + string.Join(", ", this.HiringHall.PointsBySkillAndSize.MedTech[i]) + "]");
            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write($"  -- AerospaceWings --");
            Mod.Log.Info?.Write($"  Enabled: {this.HiringHall.AerospaceWings.Enabled}");
            Mod.Log.Info?.Write($"  BaseDaysInContract: {this.HiringHall.AerospaceWings.BaseDaysInContract}  MinContractDaysMulti: {this.HiringHall.AerospaceWings.MinContractDaysMulti}  MaxContractDaysMulti: {this.HiringHall.AerospaceWings.MaxContractDaysMulti}");
            Mod.Log.Info?.Write($"  SalaryMulti: {this.HiringHall.AerospaceWings.SalaryMulti}  SalaryExponent: {this.HiringHall.AerospaceWings.SalaryExponent}  " +
                $"SalaryVariance: {this.HiringHall.AerospaceWings.SalaryVariance}  BonusVariance: {this.HiringHall.AerospaceWings.BonusVariance}");
            Mod.Log.Info?.Write($"  MaxOfType: {this.HiringHall.AerospaceWings.MaxOfType}  HazardPayRatio: {this.HiringHall.AerospaceWings.HazardPayRatio}");

            Mod.Log.Info?.Write($"  -- MechTechCrews --");
            Mod.Log.Info?.Write($"  Enabled: {this.HiringHall.MechTechCrews.Enabled}");
            Mod.Log.Info?.Write($"  BaseDaysInContract: {this.HiringHall.MechTechCrews.BaseDaysInContract}  MinContractDaysMulti: {this.HiringHall.MechTechCrews.MinContractDaysMulti}  MaxContractDaysMulti: {this.HiringHall.MechTechCrews.MaxContractDaysMulti}");
            Mod.Log.Info?.Write($"  SalaryMulti: {this.HiringHall.MechTechCrews.SalaryMulti}  SalaryExponent: {this.HiringHall.MechTechCrews.SalaryExponent}  " +
                $"SalaryVariance: {this.HiringHall.MechTechCrews.SalaryVariance}  BonusVariance: {this.HiringHall.MechTechCrews.BonusVariance}");
            Mod.Log.Info?.Write($"  MaxOfType: {this.HiringHall.MechTechCrews.MaxOfType}  HazardPayRatio: {this.HiringHall.MechTechCrews.HazardPayRatio}");

            Mod.Log.Info?.Write($"  -- MedTechCrews --");
            Mod.Log.Info?.Write($"  Enabled: {this.HiringHall.MedTechCrews.Enabled}");
            Mod.Log.Info?.Write($"  BaseDaysInContract: {this.HiringHall.MedTechCrews.BaseDaysInContract}  MinContractDaysMulti: {this.HiringHall.MedTechCrews.MinContractDaysMulti}  MaxContractDaysMulti: {this.HiringHall.MedTechCrews.MaxContractDaysMulti}");
            Mod.Log.Info?.Write($"  SalaryMulti: {this.HiringHall.MedTechCrews.SalaryMulti}  SalaryExponent: {this.HiringHall.MedTechCrews.SalaryExponent}  " +
                $"SalaryVariance: {this.HiringHall.MedTechCrews.SalaryVariance}  BonusVariance: {this.HiringHall.MedTechCrews.BonusVariance}");
            Mod.Log.Info?.Write($"  MaxOfType: {this.HiringHall.MedTechCrews.MaxOfType}  HazardPayRatio: {this.HiringHall.MedTechCrews.HazardPayRatio}");

            Mod.Log.Info?.Write($"  -- MechWarriors --");
            Mod.Log.Info?.Write($"  Enabled: {this.HiringHall.MechWarriors.Enabled}");
            Mod.Log.Info?.Write($"  BaseDaysInContract: {this.HiringHall.MechWarriors.BaseDaysInContract}  MinContractDaysMulti: {this.HiringHall.MechWarriors.MinContractDaysMulti}  MaxContractDaysMulti: {this.HiringHall.MechWarriors.MaxContractDaysMulti}");
            Mod.Log.Info?.Write($"  SalaryMulti: {this.HiringHall.MechWarriors.SalaryMulti}  SalaryExponent: {this.HiringHall.MechWarriors.SalaryExponent}  " +
                $"SalaryVariance: {this.HiringHall.MechWarriors.SalaryVariance}  BonusVariance: {this.HiringHall.MechWarriors.BonusVariance}");
            Mod.Log.Info?.Write($"  MaxOfType: {this.HiringHall.MechWarriors.MaxOfType}  HazardPayRatio: {this.HiringHall.MechWarriors.HazardPayRatio}");

            Mod.Log.Info?.Write($"  -- VehicleCrews --");
            Mod.Log.Info?.Write($"  Enabled: {this.HiringHall.VehicleCrews.Enabled}");
            Mod.Log.Info?.Write($"  BaseDaysInContract: {this.HiringHall.VehicleCrews.BaseDaysInContract}  MinContractDaysMulti: {this.HiringHall.VehicleCrews.MinContractDaysMulti}  MaxContractDaysMulti: {this.HiringHall.VehicleCrews.MaxContractDaysMulti}");
            Mod.Log.Info?.Write($"  SalaryMulti: {this.HiringHall.VehicleCrews.SalaryMulti}  SalaryExponent: {this.HiringHall.VehicleCrews.SalaryExponent}  " +
                $"SalaryVariance: {this.HiringHall.VehicleCrews.SalaryVariance}  BonusVariance: {this.HiringHall.VehicleCrews.BonusVariance}");
            Mod.Log.Info?.Write($"  MaxOfType: {this.HiringHall.VehicleCrews.MaxOfType}  HazardPayRatio: {this.HiringHall.VehicleCrews.HazardPayRatio}");

            Mod.Log.Info?.Write($"");


            Mod.Log.Info?.Write("--- HeadHunting Config ---");
            Mod.Log.Info?.Write($"  Enabled: {this.HeadHunting.Enabled}");
            Mod.Log.Info?.Write($"  EconMods: [ { string.Join(", ", this.HeadHunting.EconMods) } ]");
            Mod.Log.Info?.Write($"  ChanceBySkill: [ { string.Join(", ", this.HeadHunting.ChanceBySkill) } ]");
            Mod.Log.Info?.Write($"  FailedCooldownIntervalMin: {this.HeadHunting.FailedCooldownIntervalMin}  FailedCooldownIntervalMax: {this.HeadHunting.FailedCooldownIntervalMax}");
            Mod.Log.Info?.Write($"  SuccessCooldownIntervalMin: {this.HeadHunting.SuccessCooldownIntervalMin}  SuccessCooldownIntervalMax: {this.HeadHunting.SuccessCooldownIntervalMax}");
            Mod.Log.Info?.Write($"  CrewCooldownIntervalMin: {this.HeadHunting.CrewCooldownIntervalMin}  CrewCooldownIntervalMax: {this.HeadHunting.CrewCooldownIntervalMax}");
            Mod.Log.Info?.Write($"  CounterOfferVariance: {this.HeadHunting.CounterOfferVariance}");
            Mod.Log.Info?.Write($"  PlanetBlacklist: [ { string.Join(", ", this.HeadHunting.PlanetBlacklist) } ]");

            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write("--- Attitude Config ---");
            Mod.Log.Info?.Write($"  ThresholdMax: {this.Attitude.ThresholdMax}  ThresholdBest: {this.Attitude.ThresholdBest}  ThresholdGood: {this.Attitude.ThresholdGood}");
            Mod.Log.Info?.Write($"  ThresholdPoor: {this.Attitude.ThresholdPoor}  ThresholdWorst: {this.Attitude.ThresholdWorst}  ThresholdMin: {this.Attitude.ThresholdMin}");

            Mod.Log.Info?.Write($"  FavoredFactionChance: {this.Attitude.FavoredFactionChance}  HatedFactionChance: {this.Attitude.HatedFactionChance}");
            Mod.Log.Info?.Write($"  CandidateFavoredFactions: {string.Join(",", this.Attitude.FavoredFactionCandidates.Select(x => x.FactionDefID).ToList())}");
            Mod.Log.Info?.Write($"  CandidateHatedFactions: {string.Join(",", this.Attitude.HatedFactionCandidates.Select(x => x.FactionDefID).ToList())}");

            Mod.Log.Info?.Write($"  RehireBonusMod: {this.Attitude.RehireBonusMod}");
            Mod.Log.Info?.Write("   -- Monthly Mods --");
            Mod.Log.Info?.Write($"  FavoredEmployerAlliedMod: {this.Attitude.Monthly.FavoredEmployerAlliedMod}  HatedEmployerAlliedMod: {this.Attitude.Monthly.HatedEmployerAlliedMod}");
            Mod.Log.Info?.Write($"  Decay: {this.Attitude.Monthly.Decay}");
            Mod.Log.Info?.Write($"  EconSpartanMod: {this.Attitude.Monthly.EconSpartanMod}  EconRestrictiveMod: {this.Attitude.Monthly.EconRestrictiveMod}  " +
                $"EconomyNormalMod: {this.Attitude.Monthly.EconomyNormalMod}  EconomyGenerousMod: {this.Attitude.Monthly.EconomyGenerousMod}  " +
                $"EconomyExtravagantMod: {this.Attitude.Monthly.EconomyExtravagantMod}");
            Mod.Log.Info?.Write("   -- PerMission Mods --");
            Mod.Log.Info?.Write($"  ContractSuccessMod: {this.Attitude.PerMission.ContractSuccessMod}  ContractFailedGoodFaithMod: {this.Attitude.PerMission.ContractFailedGoodFaithMod}  " +
                $"ContractFailedMod: {this.Attitude.PerMission.ContractFailedMod}");
            Mod.Log.Info?.Write($"  PilotKilledMod: {this.Attitude.PerMission.PilotKilledMod}");
            Mod.Log.Info?.Write($"  DeployedOnMissionMod: {this.Attitude.PerMission.DeployedOnMissionMod}  BenchedOnMissionMod: {this.Attitude.PerMission.BenchedOnMissionMod}");
            Mod.Log.Info?.Write($"  FavoredFactionIsEmployerMod: {this.Attitude.PerMission.FavoredFactionIsEmployerMod}  FavoredFactionIsTargetMod: {this.Attitude.PerMission.FavoredFactionIsTargetMod}");
            Mod.Log.Info?.Write($"  HatedEmployerIsEmployerMod: {this.Attitude.PerMission.HatedFactionIsEmployerMod}  HatedEmployerIsTargetMod: {this.Attitude.PerMission.HatedFactionIsTargetMod}");

            Mod.Log.Info?.Write($"");

            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            // Translate color values into Color objects
            if (this.Crew.MechTechCrewRGB != null && this.Crew.MechTechCrewRGB.Length == 3)
            {
                this.Crew.MechTechCrewColor = new Color(this.Crew.MechTechCrewRGB[0], this.Crew.MechTechCrewRGB[1], this.Crew.MechTechCrewRGB[2]);
            }
            if (this.Crew.MedTechCrewRGB != null && this.Crew.MedTechCrewRGB.Length == 3)
            {
                this.Crew.MedTechCrewColor = new Color(this.Crew.MedTechCrewRGB[0], this.Crew.MedTechCrewRGB[1], this.Crew.MedTechCrewRGB[2]);
            }
            if (this.Crew.VehicleCrewRGB != null && this.Crew.VehicleCrewRGB.Length == 3)
            {
                this.Crew.VehicleCrewColor = new Color(this.Crew.VehicleCrewRGB[0], this.Crew.VehicleCrewRGB[1], this.Crew.VehicleCrewRGB[2]);
            }
            if (this.Crew.MechwarriorRGB != null && this.Crew.MechwarriorRGB.Length == 3)
            {
                this.Crew.MechwarriorColor = new Color(this.Crew.MechwarriorRGB[0], this.Crew.MechwarriorRGB[1], this.Crew.MechwarriorRGB[2]);
            }
            if (this.Crew.AerospaceRGB!= null && this.Crew.AerospaceRGB.Length == 3)
            {
                this.Crew.AerospaceColor = new Color(this.Crew.AerospaceRGB[0], this.Crew.AerospaceRGB[1], this.Crew.AerospaceRGB[2]);
            }

            // TODO: Validate the hiring hall distributions

            // Add any missing default values
            InitAttitudeDefaults();
            InitHiringHallDefaults();
            InitHeadHuntingDefaults();
        }

        private void InitAttitudeDefaults()
        {

            // Default and process favored factions
            if (this.Attitude.CandidateFactionsFavored.Count == 0)
                this.Attitude.CandidateFactionsFavored = new List<string>()
                {
                    "faction_AuriganRestoration", "faction_AuriganDirectorate", "faction_ComStar",
                    "faction_Davion", "faction_Liao", "faction_Kurita", "faction_Marik", "faction_Steiner",
                    "faction_TaurianConcordat", "faction_MagistracyOfCanopus"
                };

            // Default and process hated factions
            if (this.Attitude.CandidateFactionsHated.Count == 0)
                this.Attitude.CandidateFactionsHated = new List<string>()
                {
                    "faction_AuriganRestoration", "faction_AuriganDirectorate", "faction_ComStar",
                    "faction_Davion", "faction_Liao", "faction_Kurita", "faction_Marik", "faction_Steiner",
                    "faction_TaurianConcordat", "faction_MagistracyOfCanopus"
                };

            List<FactionValue> allFactions = FactionEnumeration.FactionList.Where(fv => fv.DoesGainReputation).ToList();
            foreach (FactionValue faction in allFactions)
            {
                if (this.Attitude.CandidateFactionsFavored.Contains(faction.FactionDefID))
                {
                    this.Attitude.FavoredFactionCandidates.Add(faction);
                    this.Attitude.CandidateFactionsFavored.Remove(faction.FactionDefID);
                }

                if (this.Attitude.CandidateFactionsHated.Contains(faction.FactionDefID))
                {
                    this.Attitude.HatedFactionCandidates.Add(faction);
                    this.Attitude.CandidateFactionsHated.Remove(faction.FactionDefID);
                }
            }
        }

        private void InitHiringHallDefaults()
        {
            // SkillDistribution
            // Rookie, Regular, Veteran, Elite, Legendary
            if (this.HiringHall.SkillDistribution.Breakpoints.Length == 0)
                this.HiringHall.SkillDistribution.Breakpoints = new float[] { -0.5f, 1f, 1.75f, 2.5f };

            // SizeDistribution
            // Tiny, Small, Medium, Large, Huge
            if (this.HiringHall.SizeDistribution.Breakpoints.Length == 0)
                this.HiringHall.SizeDistribution.Breakpoints = new float[] { -0.25f, 0.25f, 1.25f, 2f };
            
            // Scarcity
            if (this.HiringHall.Scarcity.PlanetTagModifiers.Count == 0)
            {
                this.HiringHall.Scarcity.PlanetTagModifiers.Add("planet_civ_innersphere",
                    new CrewTagModifier() { MechWarriors = 1f, VehicleCrews = 2f, MechTechs = 1f, MedTechs = 1f, Aerospace = 1f }
                );
                this.HiringHall.Scarcity.PlanetTagModifiers.Add("planet_civ_periphery",
                    new CrewTagModifier() { MechWarriors = 1f, VehicleCrews = 1f, MechTechs = 0f, MedTechs = 0f, Aerospace = 1f }
                );
                this.HiringHall.Scarcity.PlanetTagModifiers.Add("planet_civ_primitive",
                    new CrewTagModifier() { MechWarriors = -2f, VehicleCrews = 1f, MechTechs = -2f, MedTechs = -4f, Aerospace = -4f }
                );
            }

            // PointsBySkillAndSize
            if (this.HiringHall.PointsBySkillAndSize.Aerospace.Length == 0)
            {
                this.HiringHall.PointsBySkillAndSize.Aerospace = new int[][]
                {
                    new int[] { 1, 2, 3, 5, 8 },
                    new int[] { 2, 4, 6, 10, 16 },
                    new int[] { 3, 6, 9, 15, 24 },
                    new int[] { 4, 8, 12, 20, 32 },
                    new int[] { 5, 10, 15, 25, 40 }
                };
            }
            if (this.HiringHall.PointsBySkillAndSize.MechTech.Length == 0)
            {
                this.HiringHall.PointsBySkillAndSize.MechTech = new int[][]
                {
                    new int[] { 1, 2, 3, 5, 8 },
                    new int[] { 2, 4, 6, 10, 16 },
                    new int[] { 3, 6, 9, 15, 24 },
                    new int[] { 4, 8, 12, 20, 32 },
                    new int[] { 5, 10, 15, 25, 40 }
                };
            }
            if (this.HiringHall.PointsBySkillAndSize.MedTech.Length == 0)
            {
                this.HiringHall.PointsBySkillAndSize.MedTech = new int[][]
                {
                    new int[] { 1, 2, 3, 5, 8 },
                    new int[] { 2, 4, 6, 10, 16 },
                    new int[] { 3, 6, 9, 15, 24 },
                    new int[] { 4, 8, 12, 20, 32 },
                    new int[] { 5, 10, 15, 25, 40 }
                };
            }

            // AerospaceWings
            if (this.HiringHall.AerospaceWings.ValueThresholdForMRBLevel.Length == 0)
                this.HiringHall.AerospaceWings.ValueThresholdForMRBLevel = new float[] { 8, 16, 24, 32, 99 };
            if (this.HiringHall.AerospaceWings.SkillToExpertiseThresholds.Length == 0)
                this.HiringHall.AerospaceWings.SkillToExpertiseThresholds = new float[] { 10, 18, 27, 35, 99 };

            // MechTechCrews
            if (this.HiringHall.MechTechCrews.ValueThresholdForMRBLevel.Length == 0)
                this.HiringHall.MechTechCrews.ValueThresholdForMRBLevel = new float[] { 8, 16, 24, 32, 99 };
            if (this.HiringHall.MechTechCrews.SkillToExpertiseThresholds.Length == 0)
                this.HiringHall.MechTechCrews.SkillToExpertiseThresholds = new float[] { 10, 18, 27, 35, 99 };

            // MedTechCrews
            if (this.HiringHall.MedTechCrews.ValueThresholdForMRBLevel.Length == 0)
                this.HiringHall.MedTechCrews.ValueThresholdForMRBLevel = new float[] { 8, 16, 24, 32, 99 };
            if (this.HiringHall.MedTechCrews.SkillToExpertiseThresholds.Length == 0)
                this.HiringHall.MedTechCrews.SkillToExpertiseThresholds = new float[] { 10, 18, 27, 35, 99 };

            // MechWarriors
            if (this.HiringHall.MechWarriors.ValueThresholdForMRBLevel.Length == 0)
                this.HiringHall.MechWarriors.ValueThresholdForMRBLevel = new float[] { 10, 17, 24, 31, 99 };
            if (this.HiringHall.MechWarriors.SkillToExpertiseThresholds.Length == 0)
                this.HiringHall.MechWarriors.SkillToExpertiseThresholds = new float[] { 10, 18, 27, 35, 99 };

            // VehicleCrews
            if (this.HiringHall.VehicleCrews.ValueThresholdForMRBLevel.Length == 0)
                this.HiringHall.VehicleCrews.ValueThresholdForMRBLevel = new float[] { 10, 17, 24, 31, 99 };
            if (this.HiringHall.VehicleCrews.SkillToExpertiseThresholds.Length == 0)
                this.HiringHall.VehicleCrews.SkillToExpertiseThresholds = new float[] { 10, 18, 27, 35, 99 };

        }
        private void InitHeadHuntingDefaults()
        {
            // EconMods
            if (this.HeadHunting.EconMods.Length == 0)
                this.HeadHunting.EconMods = new float[] { 0.35f, 0.2f, 0f, -0.2f, -0.35f };

            // ChanceBySkill
            if (this.HeadHunting.ChanceBySkill.Length == 0)
                this.HeadHunting.ChanceBySkill = new float[] { 0.05f, 0.1f, 0.15f, 0.3f, 0.6f };

            // PlanetBlacklist
            if (this.HeadHunting.PlanetBlacklist.Count == 0)
                this.HeadHunting.PlanetBlacklist = new List<string>() { "planet_civ_primitive", "planet_other_plague", "planet_pop_none" };
        }
    }
}

