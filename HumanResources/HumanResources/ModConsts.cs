

using System.Collections.Generic;

namespace HumanResources
{
    public static class ModConsts
    {
        public const string GO_HBS_Profile_Layout_Title = "layout_title";
        public const string GO_HBS_Profile_Stats_Block = "mw_Stats";
        public const string GO_HBS_Profile_Layout_Stats = "layout_Stats-Or-HiringCost";

        public const string GO_HBS_Barracks_ServicePanel_BattleStats = "obj-BattleStats";
        public const string GO_HBS_Barracks_Dossier_FirstName = "text_firstName";
        public const string GO_HBS_Barracks_Dossier_LastName = "text_lastName";

        public const string GO_HBS_Barracks_Skill_Button = "uixPrfBttn_BASE_TabMedium-MANAGED-skills";

        public const string GO_Profile_Override = "hr_crew_profile";
        public const string GO_Crew_Block = "hr_crew_block";
        public const string GO_Crew_Skill = "hr_crew_skill_text";
        public const string GO_Crew_Size = "hr_crew_size_text";

        public const string Tooltip_NotEnoughBerths = "HRTooltipNotEnoughBerths";
        public const string Tooltip_TooManyOfType = "HRTooltipTooManyOfType";

        public const string Event_ContractExpired = "event_hr_mw_contractExpired";
        public const string Event_Option_ContractExpired_Hire_NoBonus = "outcome_0";
        public const string Event_Option_ContractExpired_Hire_Bonus = "outcome_1";

        public const string Event_HeadHunting = "event_hr_mw_headhunted";
        public const string Event_Option_HeadHunting_Persuaded = "outcome_0";
        public const string Event_Option_HeadHunting_Retained = "outcome_1";
        public const string Event_Option_HeadHunting_Leaves = "outcome_2";
        public const string Event_Option_HeadHunting_Deserts = "outcome_3";

        public const string Skill_Gunnery = "Gunnery";
        public const string Skill_Guts = "Guts";
        public const string Skill_Piloting = "Piloting";
        public const string Skill_Tactics = "Tactics";

    }

    // All of these tags are pilot tags
    public static class ModTags
    {

        // A unique GUID for me to use to associate CompanyStats to specific pilots
        public const string Tag_GUID = "HR_GUID_";

        public const string Tag_Attitude_Best = "HR_CREW_ATTITUDE_BEST";
        public const string Tag_Attitude_Good = "HR_CREW_ATTITUDE_GOOD";
        public const string Tag_Attitude_Neutral = "HR_CREW_ATTITUDE_NEUTRAL";
        public const string Tag_Attitude_Poor = "HR_CREW_ATTITUDE_POOR";
        public const string Tag_Attitude_Worst = "HR_CREW_ATTITUDE_WORST";
        public static List<string> Tags_Attitude_All = new List<string>
        {
            ModTags.Tag_Attitude_Best, ModTags.Tag_Attitude_Good, ModTags.Tag_Attitude_Neutral, ModTags.Tag_Attitude_Worst, ModTags.Tag_Attitude_Poor
        };

        public const string Tag_CrewType_Aerospace = "HR_CREWTYPE_AEROSPACE";
        public const string Tag_CrewType_MechTech = "HR_CREWTYPE_MECH_TECH";
        public const string Tag_CrewType_MedTech = "HR_CREWTYPE_MED_TECH";

        // CU Vehicles - pilot_vehicle_crew allow tank piloting,
        public const string Tag_CU_NoMech_Crew = "pilot_nomech_crew";
        public const string Tag_CU_Vehicle_Crew = "pilot_vehicle_crew";

        // Tags to force certain Ronin behaviors
        public const string Tag_Prefix_Ronin_Faction_Favored = "HR_RONIN_FACTION_FAVORED_";
        public const string Tag_Prefix_Ronin_Faction_Hated = "HR_RONIN_FACTION_HATED_";

        public const string Tag_Prefix_Ronin_Support_Size = "HR_RONIN_SUPPORT_SIZE_";
        public const string Tag_Prefix_Ronin_Support_Skill = "HR_RONIN_SUPPORT_SKILL_";

        public const string Tag_Prefix_Ronin_Salary_Multi = "HR_RONIN_SALARY_MULTI_";
        public const string Tag_Prefix_Ronin_Salary_Exp = "HR_RONIN_SALARY_EXP_";
        public const string Tag_Prefix_Ronin_Salary_Variance = "HR_RONIN_SALARY_VARIANCE_";
        public const string Tag_Prefix_Ronin_Bonus_Variance = "HR_RONIN_BONUS_VARIANCE_";

        // Founder tag - 
        public const string Tag_Founder = "HR_FOUNDER"; // has no salary, takes a profit share instead

        // Headhunting tags
        public const string Tag_HeadHunting_Devoted = "HR_HEADHUNT_DEVOTED"; // never headhunted
        public const string Tag_HeadHunting_Fickle = "HR_HEADHUNT_FICKLE"; // always headhunted

        // Used during cleanup; should contain all tag names
        public static List<string> Tags_All = new List<string>
        {
            ModTags.Tag_Attitude_Best, ModTags.Tag_Attitude_Good, ModTags.Tag_Attitude_Neutral, ModTags.Tag_Attitude_Worst, ModTags.Tag_Attitude_Poor,
            Tag_CrewType_Aerospace, Tag_CrewType_MechTech, Tag_CrewType_MedTech,
            Tag_CU_NoMech_Crew, Tag_CU_Vehicle_Crew
        };
    }

    public static class ModStats
    {
        public const string HBS_Company_MechTech_Skill = "MechTechSkill";
        public const string HBS_Company_MedTech_Skill = "MedTechSkill";
        public const string HBS_Company_Funds = "Funds";

        public const string Aerospace_Skill = "HR_AerospaceSkill";

        public const string Company_Reputation = "HR_CompanyRep";
        public const string Company_CrewDetail_Prefix = "HR_CrewDetail_";

        public const string Company_HeadHunting_TestOnDay = "HR_HeadHunting_TestOnDay";

        public const string CrewCount_Aerospace = "HR_CrewCount_Aerospace";
        public const string CrewCount_MechTechs = "HR_CrewCount_MechTechs";
        public const string CrewCount_MechWarriors = "HR_CrewCount_MechWarriors";
        public const string CrewCount_MedTechs = "HR_CrewCount_MedTechs";
        public const string CrewCount_VehicleCrews = "HR_CrewCount_VehicleCrews";

    }

}
