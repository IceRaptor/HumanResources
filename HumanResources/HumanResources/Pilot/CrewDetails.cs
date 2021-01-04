using BattleTech;
using HumanResources.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HumanResources.Extensions
{
    public enum CrewType
    {
        MechWarrior = 0,
        VehicleCrew = 1,
        AerospaceWing = 2,
        MechTechCrew = 3,
        MedTechCrew = 4
    }

    [JsonObject(MemberSerialization.OptOut)]
    public class CrewDetails
    {
        // Immutable properties
        public string GUID { get; set; }
        public CrewType Type { get; set; }
        public bool IsPlayer { get; set; }

        public int Size { get; set; }
        public int Skill { get; set; }
        public int Value { get; set; }
        public int HiringBonus { get; set; }
        public int Salary { get; set; }
        public int ContractTerm { get; set; }
        public int FavoredFaction { get; set; }
        public int HatedFaction { get; set; }

        // Mutable properties
        public int Attitude { get; set; }
        public int ExpirationDay { get; set; }

        // Dyanmic properties
        public int AdjustedBonus
        {
            get
            {
                float adjustedForMorale = HiringBonus *
                    ModState.SimGameState.GetExpenditureCostModifier(ModState.SimGameState.ExpenditureLevel);
                int rounded = (int)Mathf.RoundToInt(adjustedForMorale);
                return rounded;
            }
        }

        public int AdjustedSalary
        {
            get
            {
                float adjustedForMorale = Salary *
                    ModState.SimGameState.GetExpenditureCostModifier(ModState.SimGameState.ExpenditureLevel);
                int rounded = (int)Mathf.RoundToInt(adjustedForMorale);
                return rounded;
            }
        }

        public override string ToString()
        {
            return $"GUID:{GUID}  Type: {Type}  IsPlayer: {IsPlayer}  Size: {Size}  Skill: {Skill}  Value: {Value}  HiringBonus: {HiringBonus}" +
                $"  Salary: {Salary}  ContractTerm: {ContractTerm}  Loyalty: {Attitude}  ExpirationDay: {ExpirationDay}" +
                $"  AdjustedBonus: {AdjustedBonus}  AdjustedSalary: {AdjustedSalary}";
        }

        // Empty constructor for serialization
        public CrewDetails()
        {

        }

        public CrewDetails(PilotDef pilotDef, CrewType type, int sizeIdx = 0, int skillIdx = 0)
        {
            this.Type = type;
            this.GUID = Guid.NewGuid().ToString();
            this.Size = sizeIdx + 1;
            this.Skill = skillIdx + 1;

            this.Attitude = 0;

            // Calculate value and set required tags
            CrewOpts config = null;
            if (IsAerospaceCrew)
            {
                Value = Mod.Config.HiringHall.PointsBySkillAndSize.Aerospace[skillIdx][sizeIdx];
                config = Mod.Config.HiringHall.AerospaceWings;
            }
            else if (IsMechTechCrew)
            {
                Value = Mod.Config.HiringHall.PointsBySkillAndSize.MechTech[skillIdx][sizeIdx];
                config = Mod.Config.HiringHall.MechTechCrews;
            }
            else if (IsMedTechCrew)
            {
                Value = Mod.Config.HiringHall.PointsBySkillAndSize.MedTech[skillIdx][sizeIdx];
                config = Mod.Config.HiringHall.MedTechCrews;
            }
            else if (IsMechWarrior)
            {
                //Value = pilotDef.BaseGunnery + pilotDef.BonusGunnery +
                //        pilotDef.BasePiloting + pilotDef.BonusPiloting +
                //        pilotDef.BaseGuts + pilotDef.BonusGuts +
                //        pilotDef.BaseTactics + pilotDef.BonusTactics;
                Value = pilotDef.BaseGunnery + pilotDef.BasePiloting + pilotDef.BaseGuts + pilotDef.BaseTactics;
                config = Mod.Config.HiringHall.MechWarriors;
            }
            else if (IsVehicleCrew)
            {
                //Value = pilotDef.BaseGunnery + pilotDef.BonusGunnery +
                //        pilotDef.BasePiloting + pilotDef.BonusPiloting +
                //        pilotDef.BaseGuts + pilotDef.BonusGuts +
                //        pilotDef.BaseTactics + pilotDef.BonusTactics;
                Value = pilotDef.BaseGunnery + pilotDef.BasePiloting + pilotDef.BaseGuts + pilotDef.BaseTactics;
                config = Mod.Config.HiringHall.VehicleCrews;

                // Required tags
                pilotDef.PilotTags.Add(ModTags.Tag_CU_NoMech_Crew);
                pilotDef.PilotTags.Add(ModTags.Tag_CU_Vehicle_Crew);
            }
            // Add GUID tag
            pilotDef.PilotTags.Add($"{ModTags.Tag_GUID}{GUID}");

            // Calculate salary and bonus
            SalaryHelper.CalculateSalary(Value, config, out int salary, out int bonus);
            this.Salary = salary;
            this.HiringBonus = bonus;

            // Determine contract length
            if (pilotDef.IsFree && pilotDef.IsImmortal)
            {
                IsPlayer = true;
                Mod.Log.Debug?.Write("Setting expiration and contract term to 0 for player character.");
                this.ContractTerm = 0; // Free and Immortal = player character
                this.ExpirationDay = 0;
            }
            else
            {
                IsPlayer = false;
                Mod.Log.Debug?.Write("Generating contract length, new expiration day");
                this.ContractTerm = PilotHelper.RandomContractLength(config);
                this.ExpirationDay = ModState.SimGameState.DaysPassed + ContractTerm;
            }

            // Check for favored / hated faction
            List<FactionValue> factions = FactionEnumeration.FactionList.Where(fv => fv.DoesGainReputation).ToList();

            double favoredRoll = Mod.Random.NextDouble();
            if (favoredRoll < Mod.Config.Attitude.FavoredFactionChance)
            {
                int idx = Mod.Random.Next(factions.Count - 1);
                FactionValue faction = factions.ElementAt(idx);
                factions.RemoveAt(idx);
                Mod.Log.Info?.Write($"Roll {favoredRoll} < {Mod.Config.Attitude.FavoredFactionChance}, adding {faction} as favored faction");
                this.FavoredFaction = faction.ID;
            }
            else
                this.FavoredFaction = -1;

            double hatedRoll = Mod.Random.NextDouble();
            if (hatedRoll < Mod.Config.Attitude.HatedFactionChance)
            {
                int idx = Mod.Random.Next(factions.Count - 1);
                FactionValue faction = factions.ElementAt(idx);
                factions.RemoveAt(idx);
                Mod.Log.Info?.Write($"Roll {hatedRoll} < {Mod.Config.Attitude.HatedFactionChance}, adding {faction} as hated faction");
                this.HatedFaction = faction.ID;
            }
            else
                this.HatedFaction = -1;

        }

        public bool IsAerospaceCrew { get { return Type == CrewType.AerospaceWing; } }
        public bool IsMechTechCrew { get { return Type == CrewType.MechTechCrew; } }
        public bool IsMechWarrior { get { return Type == CrewType.MechWarrior; } }
        public bool IsMedTechCrew { get { return Type == CrewType.MedTechCrew; } }
        public bool IsVehicleCrew { get { return Type == CrewType.VehicleCrew; } }

        public int HazardPay
        {
            get
            {
                CrewOpts options = null;
                if (IsMechTechCrew || IsMedTechCrew || IsPlayer) return 0;
                else if (IsAerospaceCrew) options = Mod.Config.HiringHall.AerospaceWings;
                else if (IsMechWarrior) options = Mod.Config.HiringHall.MechWarriors;
                else if (IsVehicleCrew) options = Mod.Config.HiringHall.VehicleCrews;

                float payFraction = Salary * options.HazardPayRatio;
                Mod.Log.Debug?.Write($"Crew has payFraction: {payFraction} => salary: {Salary} x hazardPayRatio: {options.HazardPayRatio}");

                float fraction = (float)Math.Floor(payFraction / options.HazardPayUnits);
                float hazardPay = options.HazardPayUnits * fraction;
                Mod.Log.Debug?.Write($"  Hazard pay: {hazardPay} => payFraction: {payFraction} % modulo: {options.HazardPayUnits} = fraction: {fraction}");

                return (int)Math.Floor(hazardPay);
            }
        }

        public string SizeLabel
        {
            get
            {
                string label = "UNKNOWN";
                switch (Size)
                {
                    case 5:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_5];
                        break;
                    case 4:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_4];
                        break;
                    case 3:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_3];
                        break;
                    case 2:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_2];
                        break;
                    default:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_1];
                        break;
                }
                return label;
            }
        }

        public string ExpertiseLabel
        {
            get
            {
                int expertise = Skill;
                if (IsMechWarrior || IsVehicleCrew) expertise = Expertise;

                string label;
                switch (expertise)
                {
                    case 5:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_5];
                        break;
                    case 4:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_4];
                        break;
                    case 3:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_3];
                        break;
                    case 2:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_2];
                        break;
                    default:
                        label = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_1];
                        break;
                }
                return label;
            }
        }

        public int Expertise
        {
            get
            {
                if (IsAerospaceCrew || IsMechTechCrew || IsMedTechCrew) return Skill;

                CrewOpts opts = IsMechWarrior ? Mod.Config.HiringHall.MechWarriors : Mod.Config.HiringHall.VehicleCrews;

                int expertise = 0;
                for (int i = 0; i < opts.SkillToExpertiseThresholds.Count(); i++)
                {
                    if (Value <= opts.SkillToExpertiseThresholds[i])
                    {
                        expertise = i;
                        break;
                    }
                }

                return expertise;

            }
        }

        public bool CanBeHiredAtMRBLevel(int MRBLevel)
        {
            float threshold = 0f;
            if (IsAerospaceCrew)
                threshold = Mod.Config.HiringHall.AerospaceWings.ValueThresholdForMRBLevel[MRBLevel];
            else if (IsMechTechCrew)
                threshold = Mod.Config.HiringHall.MechTechCrews.ValueThresholdForMRBLevel[MRBLevel];
            else if (IsMechWarrior)
                threshold = Mod.Config.HiringHall.MechWarriors.ValueThresholdForMRBLevel[MRBLevel];
            else if (IsMedTechCrew)
                threshold = Mod.Config.HiringHall.MedTechCrews.ValueThresholdForMRBLevel[MRBLevel];
            else if (IsVehicleCrew)
                threshold = Mod.Config.HiringHall.VehicleCrews.ValueThresholdForMRBLevel[MRBLevel];

            Mod.Log.Debug?.Write($"CanBeHired: {Value <= threshold} => value: {Value} <= threshold: {threshold}");
            return Value <= threshold;
        }

        // -- COMPARISON FUNCTIONS --

        // Evaluation is highest skills == lowest

        // Value represents either all combined skills -or- the support points generated 
        public static int CompareByValue(CrewDetails details1, CrewDetails details2)
        {
            // Check nullity
            if (details1 == null && details2 == null) return 0;
            else if (details1 != null && details2 == null) return 1;
            else if (details1 == null && details2 != null) return -1;

            // Check skill
            if (details1.Value > details2.Value) return -1;
            else if (details2.Value > details1.Value) return 1;

            return 0;
        }

        // Evaluation is MW, VCrew, Aerospace, MechTech, MedTech
        public static int CompareByType(CrewDetails details1, CrewDetails details2)
        {
            // Check nullity
            if (details1 == null && details2 == null) return 0;
            else if (details1 != null && details2 == null) return -1;
            else if (details1 == null && details2 != null) return 1;

            // Check type
            int detailsType1 = details1.TypeSortPriority();
            int detailsType2 = details2.TypeSortPriority();

            if (detailsType1 > detailsType2) return 1;
            else if (detailsType2 > detailsType1) return -1;

            return 0;
        }

        private int TypeSortPriority()
        {
            int priority = 0;

            if (IsPlayer) priority = 0;
            if (IsMechWarrior) priority = 1;
            else if (IsVehicleCrew) priority = 2;
            else if (IsVehicleCrew) priority = 3;
            else if (IsMechTechCrew) priority = 4;
            else if (IsMedTechCrew) priority = 5;
            else if (IsAerospaceCrew) priority = 6;

            return priority;
        }
    }
}
