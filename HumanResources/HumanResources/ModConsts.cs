﻿

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

        public const string Event_HeadHunting = "event_hr_mw_headhunting";
        public const string Event_Option_HeadHunting_Retained = "outcome_0";
        public const string Event_Option_HeadHunting_Leaves = "outcome_1";

    }

    // All of these tags are pilot tags
    public static class ModTags
    {

        // A unique GUID for me to use to associate CompanyStats to specific pilots
        public const string Tag_GUID = "HR_GUID_";

        public const string Tag_Crew_Loyalty_Dedicated = "HR_CREW_LOYALTY_DEDICATED";
        public const string Tag_Crew_Loyalty_Comfortable = "HR_CREW_LOYALTY_COMFORTABLE";
        public const string Tag_Crew_Loyalty_Neutral = "HR_CREW_LOYALTY_NEUTRAL";
        public const string Tag_Crew_Loyalty_Unhappy = "HR_CREW_LOYALTY_UNHAPPY";
        public const string Tag_Crew_Loyalty_Hated = "HR_CREW_LOYALTY_HATED";

        // Applied if you ignored their complains 
        public const string Tag_Crew_Loyalty_Faction_Warning = "HR_CREW_LOYALTY_FACTION_WARNING";

        // CU Vehicles - pilot_vehicle_crew allow tank piloting,
        public const string Tag_CU_Vehicle_Crew = "pilot_vehicle_crew";
        public const string Tag_CU_NoMech_Crew = "pilot_nomech_crew";
    }

    public static class ModStats
    {
        public const string HBS_Company_MechTech_Skill = "MechTechSkill";
        public const string HBS_Company_MedTech_Skill = "MedTechSkill";
        public const string HBS_Company_Funds = "Funds";

        public const string Aerospace_Skill = "HR_AerospaceSkill";
        
        public const string Company_Reputation = "HR_CompanyRep";
        public const string Company_CrewDetail_Prefix = "HR_CrewDetail_";
        public const string Company_Poaching_Record = "HR_Poaching_Record";

        public const string CrewCount_Aerospace = "HR_CrewCount_Aerospace";
        public const string CrewCount_MechTechs = "HR_CrewCount_MechTechs";
        public const string CrewCount_MechWarriors = "HR_CrewCount_MechWarriors";
        public const string CrewCount_MedTechs = "HR_CrewCount_MedTechs";
        public const string CrewCount_VehicleCrews = "HR_CrewCount_VehicleCrews";
    }

}
