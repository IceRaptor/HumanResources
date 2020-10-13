using BattleTech;
using System;
using UnityEngine;

namespace HumanResources.Extensions
{
    public static class PilotExtensions
    {
        public static CrewDetails Evaluate(this PilotDef pilotDef)
        {
            return new CrewDetails(pilotDef);
        }
    }

    public class CrewDetails
    {
        private readonly bool hasCrewFlagMechTech = false;
        private readonly bool hasCrewFlagMedTech = false;
        private readonly bool hasCrewFlagVehicle = false;
        private readonly bool hasCrewFlagAerospace = false;

        private readonly int size = 1;
        private readonly string sizeLabel = "UNKNOWN";

        private readonly int skill = 1;
        private readonly string skillLabel = "UNKNOWN";

        private readonly int bonus = 0;
        private readonly int salary = 0;

        public CrewDetails(PilotDef pilotDef)
        {
            if (pilotDef != null && pilotDef.PilotTags != null)
            {
                //Mod.Log.Debug?.Write($"Evaluating tags on pilot: {pilot.Name}");
                foreach (string tag in pilotDef.PilotTags)
                {
                    //Mod.Log.Debug?.Write($" -- tag: {tag}");

                    // Type
                    if (ModTags.Tag_Crew_Type_Aerospace.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasCrewFlagAerospace = true;
                    }
                    if (ModTags.Tag_Crew_Type_MechTech.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasCrewFlagMechTech = true;
                    }
                    if (ModTags.Tag_Crew_Type_MedTech.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasCrewFlagMedTech = true;
                    }
                    if (ModTags.Tag_Crew_Type_Vehicle.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        hasCrewFlagVehicle = true;
                    }

                    if (tag.StartsWith(ModTags.Tag_Crew_Size_Prefix))
                    {
                        string value = tag.Substring(ModTags.Tag_Crew_Size_Prefix.Length);
                        int size = Int32.Parse(value);
                        switch (size)
                        {
                            case 5:
                                sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_5];
                                break;
                            case 4:
                                sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_4];
                                break;
                            case 3:
                                sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_3];
                                break;
                            case 2:
                                sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_2];
                                break;
                            default:
                                sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_1];
                                break;
                        }

                    }

                    if (tag.StartsWith(ModTags.Tag_Crew_Skill_Prefix))
                    {
                        string value = tag.Substring(ModTags.Tag_Crew_Skill_Prefix.Length);
                        int skill = Int32.Parse(value);
                        switch (skill)
                        {
                            case 5:
                                skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_5];
                                break;
                            case 4:
                                skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_4];
                                break;
                            case 3:
                                skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_3];
                                break;
                            case 2:
                                skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_2];
                                break;
                            default:
                                skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_1];
                                break;
                        }
                    }

                    if (tag.StartsWith(ModTags.Tag_Crew_Bonus_Prefix))
                    {
                        string value = tag.Substring(ModTags.Tag_Crew_Bonus_Prefix.Length);
                        bonus = Int32.Parse(value);
                    }

                    if (tag.StartsWith(ModTags.Tag_Crew_Salary_Prefix))
                    {
                        string value = tag.Substring(ModTags.Tag_Crew_Salary_Prefix.Length);
                        salary = Int32.Parse(value);
                    }

                }

                // If no bonus was found, set it
                if (bonus == 0 || salary == 0)
                {
                    if (IsMechTechCrew || IsMedTechCrew)
                    {
                        bonus = skill * size * Mod.Config.HiringHall.BonusCostPerPoint;
                        salary = skill * size * Mod.Config.HiringHall.SalaryCostPerPoint;
                        Mod.Log.Debug?.Write($" - crew hiring bonus: {bonus}  monthlyCost: {salary}");
                    }
                    else
                    {
                        bonus = ModState.SimGameState.GetMechWarriorValue(pilotDef);
                        salary = bonus;
                        Mod.Log.Debug?.Write($" - warrior hiring bonus: {bonus}  monthlyCost: {salary}");
                    }

                    // Apply a variance
                    float bonusFrac = bonus / 10f;
                    float bonusVarianceRange = (bonusFrac * Mod.Config.HiringHall.SalaryVariance);
                    float bonusRawVariance = Mod.Random.Next((int)Math.Floor(bonusVarianceRange));
                    int bonusVariance = (int)Math.Floor(bonusRawVariance * 10f);
                    Mod.Log.Debug?.Write($" bonusVariance => frac: {bonusFrac}  varianceRange: {bonusVarianceRange}  rawVariance: {bonusRawVariance}  variance: {bonusVariance}");
                    bonus += bonusVariance;

                    float salaryFrac = bonus / 10f;
                    float salaryVarianceRange = (bonusFrac * Mod.Config.HiringHall.SalaryVariance);
                    float salaryRawVariance = Mod.Random.Next((int)Math.Floor(bonusVarianceRange));
                    int salaryVariance = (int)Math.Floor(bonusRawVariance * 10f);
                    Mod.Log.Debug?.Write($" salaryVariance => frac: {salaryFrac}  varianceRange: {salaryVarianceRange}  rawVariance: {salaryRawVariance}  variance: {salaryVariance}");
                    salary += salaryVariance;

                    pilotDef.PilotTags.Add($"{ModTags.Tag_Crew_Bonus_Prefix}{bonus}");
                    pilotDef.PilotTags.Add($"{ModTags.Tag_Crew_Salary_Prefix}{salary}");
                }

            }
        }

        public bool IsAerospaceCrew { get { return hasCrewFlagAerospace; } }
        public bool IsMechTechCrew { get { return hasCrewFlagMechTech; } }
        public bool IsMedTechCrew { get { return hasCrewFlagMedTech; } }
        public bool IsVehicleCrew { get { return hasCrewFlagVehicle; } }
        public bool IsMechWarrior { get { return !hasCrewFlagAerospace && !hasCrewFlagMechTech && !hasCrewFlagMedTech && !hasCrewFlagVehicle; } }

        public int Size { get { return size; } }
        public string SizeLabel { get { return sizeLabel; } }

        public int Skill { get { return skill; } }
        public string SkillLabel { get { return skillLabel; } }

        public int Bonus { get { return bonus; } }
        public int Salary { get { return salary; } }

        public int MechTechPoints
        {
            get
            {
                if (!IsMechTechCrew) return 0;

                int points = 0;
                try
                {
                    points = Mod.Config.HiringHall.PointsBySkillAndSize.MechTech[Skill][Size];
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, $"Failed to read mechTech points matrix for skill: {Skill} and size: {Size}." +
                        $"This should not happen, check mod.config HiringHall.MechTechPointsBySkillAndSize values!");
                }
                return points;
            }
        }

        public int MedTechPoints
        {
            get
            {
                if (!IsMedTechCrew) return 0;

                int points = 0;
                try
                {
                    points = Mod.Config.HiringHall.PointsBySkillAndSize.MedTech[Skill][Size];
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, $"Failed to read medTech points matrix for skill: {Skill} and size: {Size}." +
                        $"This should not happen, check mod.config HiringHall.MechTechPointsBySkillAndSize values!");
                }
                return points;
            }
        }

        public int AerospacePoints
        {
            get
            {
                if (!IsAerospaceCrew) return 0;
                int points = 0;
                try
                {
                    points = Mod.Config.HiringHall.PointsBySkillAndSize.Aerospace[Skill][Size];
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, $"Failed to read aerospace points matrix for skill: {Skill} and size: {Size}." +
                        $"This should not happen, check mod.config HiringHall.MechTechPointsBySkillAndSize values!");
                }
                return points;
            }
        }

        public int AdjustedBonus
        {
            get
            {
                float adjustedForMorale = Bonus *
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

    }
}
