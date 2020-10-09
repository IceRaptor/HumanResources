using BattleTech;
using System;

namespace HumanResources.Extensions
{
    public static class PilotExtensions
    {
        public static CrewDetails Evaluate(this Pilot pilot)
        {
            return new CrewDetails(pilot);
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

        public CrewDetails(Pilot pilot)
        {
            if (pilot != null && pilot.pilotDef != null && pilot.pilotDef.PilotTags != null)
            {
                //Mod.Log.Debug?.Write($"Evaluating tags on pilot: {pilot.Name}");
                foreach (string tag in pilot.pilotDef.PilotTags)
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

                    // Size
                    if (ModTags.Tag_Crew_Size_1.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        size = 1;
                        sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_1];
                    }
                    if (ModTags.Tag_Crew_Size_2.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        size = 2;
                        sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_2];
                    }
                    if (ModTags.Tag_Crew_Size_3.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        size = 3;
                        sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_3];
                    }
                    if (ModTags.Tag_Crew_Size_4.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        size = 4;
                        sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_4];
                    }
                    if (ModTags.Tag_Crew_Size_5.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        size = 5;
                        sizeLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Size_5];
                    }

                    // Skill
                    if (ModTags.Tag_Crew_Skill_1.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skill = 1;
                        skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_1];
                    }
                    if (ModTags.Tag_Crew_Skill_2.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skill = 2;
                        skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_2];
                    }
                    if (ModTags.Tag_Crew_Skill_3.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skill = 3;
                        skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_3];
                    }
                    if (ModTags.Tag_Crew_Skill_4.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skill = 4;
                        skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_4];
                    }
                    if (ModTags.Tag_Crew_Skill_5.Equals(tag, StringComparison.InvariantCultureIgnoreCase))
                    {
                        skill = 5;
                        skillLabel = Mod.LocalizedText.Labels[ModText.LT_Crew_Skill_Level_5];
                    }
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

        public int MechTechPoints 
        {  
            get 
            {
                if (!IsMechTechCrew) return 0;

                int points = 0;
                try
                {
                    points = Mod.Config.HiringHall.MechTechPointsBySkillAndSize[Skill][Size];
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
                    points = Mod.Config.HiringHall.MedTechPointsBySkillAndSize[Skill][Size];
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, $"Failed to read medTech points matrix for skill: {Skill} and size: {Size}." +
                        $"This should not happen, check mod.config HiringHall.MechTechPointsBySkillAndSize values!");
                }
                return points;
            }
        }

    }
}
