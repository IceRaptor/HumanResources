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
        public const string DT_CrewType_Aerospace = "CREW_TYPE_AEROSPACE";
        public const string DT_CrewType_MechTech = "CREW_TYPE_MECH_TECH";
        public const string DT_CrewType_MechWarrior = "CREW_TYPE_MECHWARRIOR";
        public const string DT_CrewType_MedTech = "CREW_TYPE_MED_TECH";
        public const string DT_CrewType_Vehicle = "CREW_TYPE_VEHICLE";

        public static Dictionary<string, string> Descriptions = new Dictionary<string, string>()
        {
            { DT_CrewType_Aerospace, "" },
            { DT_CrewType_MechTech, "" },
            { DT_CrewType_MechWarrior, "" },
            { DT_CrewType_MedTech, "" },
            { DT_CrewType_Vehicle, "" }
        };

        // === Labels text ====
        public const string LT_Crew_Skill_1 = "SKILL_LEVEL_1";
        public const string LT_Crew_Skill_2 = "SKILL_LEVEL_2";
        public const string LT_Crew_Skill_3 = "SKILL_LEVEL_3";
        public const string LT_Crew_Skill_4 = "SKILL_LEVEL_4";
        public const string LT_Crew_Skill_5 = "SKILL_LEVEL_5";

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

        public const string LT_Crew_Dossier_Name = "CREW_DOSSIER_NAME";
        public const string LT_Crew_Dossier_Contract_Term = "CREW_DOSSIER_CONTRACT_TERM";
        public const string LT_Crew_Dossier_Days_Remaining = "CREW_DOSSIER_DAYS_REMAINING";

        public const string LT_Crew_Dossier_Biography_Attitude = "CREW_DOSSIER_BIO_ATTITUDE";
        public const string LT_Crew_Dossier_Biography_Faction_Favored = "CREW_DOSSIER_BIO_FACTION_FAVORED";
        public const string LT_Crew_Dossier_Biography_Faction_Hated = "CREW_DOSSIER_BIO_FACTION_HATED";

        public const string LT_Crew_Attitude_Best = "CREW_ATTITUDE_BEST";
        public const string LT_Crew_Attitude_Good = "CREW_ATTITUDE_GOOD";
        public const string LT_Crew_Attitude_Average = "CREW_ATTITUDE_AVERAGE";
        public const string LT_Crew_Attitude_Poor = "CREW_ATTITUDE_POOR";
        public const string LT_Crew_Attitude_Worst = "CREW_ATTITUDE_WORST";

        public const string LT_Crew_Hazard_Pay = "CREW_HAZARD_PAY";
        public const string LT_Contract_Hazard_Pay = "CONTRACT_HAZARD_PAY";

        public Dictionary<string, string> Labels = new Dictionary<string, string>
        {
            { LT_Crew_Skill_1, "Rookie" },
            { LT_Crew_Skill_2, "Regular" },
            { LT_Crew_Skill_3, "Veteran" },
            { LT_Crew_Skill_4, "Elite" },
            { LT_Crew_Skill_5, "Legendary" },

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

            { LT_Crew_Dossier_Name, "Name" },
            { LT_Crew_Dossier_Contract_Term, "Contract End" },
            { LT_Crew_Dossier_Days_Remaining, "{0} days" },

            { LT_Crew_Dossier_Biography_Attitude, "<b>Attitude towards the Company:</b>  {0} ({1})" },
            { LT_Crew_Dossier_Biography_Faction_Favored, "<b>Favored Faction:</b>  {0}" },
            { LT_Crew_Dossier_Biography_Faction_Hated, "<b>Hated Faction:</b>  {0}" },

            { LT_Crew_Attitude_Best, "Loyal" },
            { LT_Crew_Attitude_Good, "Favored" },
            { LT_Crew_Attitude_Average, "Neutral" },
            { LT_Crew_Attitude_Poor, "Disliked" },
            { LT_Crew_Attitude_Worst, "Detested" },

            { LT_Crew_Hazard_Pay, "<b>Hazard Pay:</b>  {0}" },
            { LT_Contract_Hazard_Pay, "Crew Hazard Pay: {0}" }

        };

        // === Head Hunting text ====
        public const string ET_HeadHunted_Retention = "HEADHUNTED_RETENTION";
        public const string ET_HeadHunted_Buyout = "HEADHUNTED_CONTRACT_BUYOUT";

        public Dictionary<string, string> Events = new Dictionary<string, string>
        {
            { ET_HeadHunted_Retention, " Retention Bonus: {0}\n\n" },
            { ET_HeadHunted_Buyout, " Buyout Payment: {0}\n\n" }
        };

        // === Planetary text ====
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

