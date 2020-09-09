using System.Collections.Generic;

namespace PitCrew
{
    public class ModCrewNames
    {
        public List<string> MechTech = new List<string>();
        public List<string> MedTech = new List<string>();
        public List<string> Vehicle = new List<string>();
    }

    public class ModText
    {
        public const string FT_Skill_Level_1 = "SKILL_LEVEL_1";
        public const string FT_Skill_Level_2 = "SKILL_LEVEL_2";
        public const string FT_Skill_Level_3 = "SKILL_LEVEL_3";
        public const string FT_Skill_Level_4 = "SKILL_LEVEL_4";
        public const string FT_Skill_Level_5 = "SKILL_LEVEL_5";

        public const string FT_Crew_Size_1 = "CREW_SIZE_1";
        public const string FT_Crew_Size_2 = "CREW_SIZE_2";
        public const string FT_Crew_Size_3 = "CREW_SIZE_3";
        public const string FT_Crew_Size_4 = "CREW_SIZE_4";
        public const string FT_Crew_Size_5 = "CREW_SIZE_5";

        public Dictionary<string, string> Tooltips = new Dictionary<string, string>
        {
            { FT_Skill_Level_1, "Rookie" },
            { FT_Skill_Level_2, "Regular" },
            { FT_Skill_Level_3, "Veteran" },
            { FT_Skill_Level_4, "Elite" },
            { FT_Skill_Level_5, "Legendary" },

            { FT_Crew_Size_1, "Tiny" },
            { FT_Crew_Size_2, "Small" },
            { FT_Crew_Size_3, "Medium" },
            { FT_Crew_Size_4, "Large" },
            { FT_Crew_Size_5, "Huge" }
        };

        public const string PT_MW_BOUNDS = "PLANET_MECHWARRIOR_BOUNDS";
        public const string PT_MECH_TECH_BOUNDS = "PLANET_MECH_TECH_BOUNDS";
        public const string PT_MED_TECH_BOUNDS = "PLANET_MED_TECH_BOUNDS";
        public const string PT_VEHICLE_BOUNDS = "PLANET_VEHICLE_CREW_BOUNDS";

        public Dictionary<string, string> PlanetStrings = new Dictionary<string, string>
        {
            { PT_MW_BOUNDS,         "<color=red>Hirable MechWarriors</color> - {0} to {1}" },
            { PT_MECH_TECH_BOUNDS,  "<color=red>Hirable MechTechs</color> - {0} to {1}" },
            { PT_MED_TECH_BOUNDS,   "<color=red>Hirable MedTechs</color> - {0} to {1}" },
            { PT_VEHICLE_BOUNDS,    "<color=red>Hirable Vehicle Crews</color> - {0} to {1}" },
        };

    }
}
