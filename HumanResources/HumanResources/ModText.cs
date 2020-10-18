using System.Collections.Generic;

namespace HumanResources
{
    public class ModCrewNames
    {
        public List<string> Aerospace = new List<string>();
        public List<string> MechTech = new List<string>();
        public List<string> MedTech = new List<string>();
        public List<string> Vehicle = new List<string>();
    }

    public class ModText
    {
        public const string LT_Crew_Skill_Level_1 = "SKILL_LEVEL_1";
        public const string LT_Crew_Skill_Level_2 = "SKILL_LEVEL_2";
        public const string LT_Crew_Skill_Level_3 = "SKILL_LEVEL_3";
        public const string LT_Crew_Skill_Level_4 = "SKILL_LEVEL_4";
        public const string LT_Crew_Skill_Level_5 = "SKILL_LEVEL_5";

        public const string LT_Skill_Aerospace_Points = "SKILL_AEROSPACE_POINTS";
        public const string LT_Skill_MechTech_Points = "SKILL_MECHTECH_POINTS";
        public const string LT_Skill_MedTech_Points = "SKILL_MEDTECH_POINTS";

        public const string LT_Crew_Size_1 = "CREW_SIZE_1";
        public const string LT_Crew_Size_2 = "CREW_SIZE_2";
        public const string LT_Crew_Size_3 = "CREW_SIZE_3";
        public const string LT_Crew_Size_4 = "CREW_SIZE_4";
        public const string LT_Crew_Size_5 = "CREW_SIZE_5";

        public const string LT_Crew_Size = "CREW_SIZE_LABEL";
        public const string LT_Crew_Berths_Used = "CREW_BERTHS_USED";

        public const string LT_Crew_Hire_Button = "CREW_HIRE_BUTTON";
        public const string LT_Crew_Bonus_Label = "CREW_HIRE_BONUS_LABEL";
        public const string LT_Crew_Salary_Label = "CREW_HIRE_SALARY_LABEL";

        public const string LT_Crew_Contract_Term = "CREW_CONTRACT_TERM";
        public const string LT_Crew_Name_Format = "CREW_NAME_FORMAT";

        public const string LT_Crew_Dossier_Contract_Term = "CREW_DOSSIER_CONTRACT_TERM";
        public const string LT_Crew_Dossier_Days_Remaining = "CREW_DOSSIER_DAYS_REMAINING";

        public Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            { LT_Crew_Skill_Level_1, "Rookie" },
            { LT_Crew_Skill_Level_2, "Regular" },
            { LT_Crew_Skill_Level_3, "Veteran" },
            { LT_Crew_Skill_Level_4, "Elite" },
            { LT_Crew_Skill_Level_5, "Legendary" },

            { LT_Skill_Aerospace_Points, "Aerospace Points: +{0}" },
            { LT_Skill_MechTech_Points, "MechTech Points: +{0}" },
            { LT_Skill_MedTech_Points, "MedTech Points: +{0}" },

            { LT_Crew_Size_1, "Tiny" },
            { LT_Crew_Size_2, "Small" },
            { LT_Crew_Size_3, "Medium" },
            { LT_Crew_Size_4, "Large" },
            { LT_Crew_Size_5, "Huge" },

            { LT_Crew_Size, "{0} Crew ({1} berths)" },
            { LT_Crew_Berths_Used, "{0} / {1} Berths" },

            { LT_Crew_Hire_Button, "Hire for {0}?" },
            { LT_Crew_Bonus_Label, "{0} Bonus" },
            { LT_Crew_Salary_Label, "{0} / Mo" },

            { LT_Crew_Contract_Term, "The contract term for this mercenary is {0} days. " },

            { LT_Crew_Name_Format, "{0} {1}" },
            
            { LT_Crew_Dossier_Contract_Term, "Contract End" },
            { LT_Crew_Dossier_Days_Remaining, "{0} days" }

        };

        public const string PT_AERO_BOUNDS = "PLANET_AEROSPACE_BOUNDS";
        public const string PT_MW_BOUNDS = "PLANET_MECHWARRIOR_BOUNDS";
        public const string PT_MECH_TECH_BOUNDS = "PLANET_MECH_TECH_BOUNDS";
        public const string PT_MED_TECH_BOUNDS = "PLANET_MED_TECH_BOUNDS";
        public const string PT_VEHICLE_BOUNDS = "PLANET_VEHICLE_CREW_BOUNDS";

        public Dictionary<string, string> PlanetStrings = new Dictionary<string, string>
        {
            { PT_AERO_BOUNDS,       "<color=red>Hirable Aerospace</color> - {0} to {1}" },
            { PT_MECH_TECH_BOUNDS,  "<color=red>Hirable MechTechs</color> - {0} to {1}" },
            { PT_MW_BOUNDS,         "<color=red>Hirable MechWarriors</color> - {0} to {1}" },
            { PT_MED_TECH_BOUNDS,   "<color=red>Hirable MedTechs</color> - {0} to {1}" },
            { PT_VEHICLE_BOUNDS,    "<color=red>Hirable Vehicle Crews</color> - {0} to {1}" },
        };

    }
}
