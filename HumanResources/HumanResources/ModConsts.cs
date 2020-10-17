

namespace HumanResources
{
    public static class ModConsts
    {

        public const string GO_HBS_Profile_Layout_Title = "layout_title";
        public const string GO_HBS_Profile_Stats_Block = "mw_Stats";
        public const string GO_HBS_Profile_Layout_Stats = "layout_Stats-Or-HiringCost";
        
        public const string GO_HBS_Barracks_ServicePanel_BattleStats = "obj-BattleStats";
        public const string GO_HBS_Barracks_Dossier_LastName = "text_lastName";

        public const string GO_Profile_Override = "hr_crew_profile";
        public const string GO_Crew_Block = "hr_crew_block";
        public const string GO_Crew_Skill = "hr_crew_skill_text";
        public const string GO_Crew_Size = "hr_crew_size_text";

        public const string Tooltip_NotEnoughBerths = "HRTooltipNotEnoughBerths";
    }

    // All of these tags are pilot tags
    public static class ModTags
    {
        public const string Tag_Crew_Type_Aerospace = "HR_CREW_AEROSPACE";
        public const string Tag_Crew_Type_MedTech = "HR_CREW_MED_TECH";
        public const string Tag_Crew_Type_MechTech = "HR_CREW_MECH_TECH";
        public const string Tag_Crew_Type_Vehicle = "HR_CREW_VEHICLE";

        public const string Tag_Crew_Size_Template = "HR_CREW_SIZE_{0}";
        public const string Tag_Crew_Size_Prefix = "HR_CREW_SIZE_";

        public const string Tag_Crew_Skill_Template = "HR_CREW_SKILL_{0}";
        public const string Tag_Crew_Skill_Prefix = "HR_CREW_SKILL_";

        public const string Tag_Crew_Bonus_Prefix = "HR_CREW_BONUS_";
        public const string Tag_Crew_Salary_Prefix = "HR_CREW_SALARY_";

        public const string Tag_Crew_ContractEndDay_Prefix = "HR_CREW_CONTRACT_END_DATE_";
        public const string Tag_Crew_ContractTerm_Prefix = "HR_CREW_CONTRACT_TERM_";

        public const string Tag_Crew_LoyaltyLevel_Prefix = "HR_CREW_LOYALTY_LEVEL_";

        public const string Tag_Crew_Loyalty_Dedicated = "HR_CREW_LOYALTY_DEDICATED";
        public const string Tag_Crew_Loyalty_Comfortable = "HR_CREW_LOYALTY_COMFORTABLE";
        public const string Tag_Crew_Loyalty_Neutral = "HR_CREW_LOYALTY_NEUTRAL";
        public const string Tag_Crew_Loyalty_Unhappy = "HR_CREW_LOYALTY_UNHAPPY";
        public const string Tag_Crew_Loyalty_Hated = "HR_CREW_LOYALTY_HATED";

        // CU Vehicles - pilot_vehicle_crew allow tank piloting, 
        // if also has pilot_nomech_crew than its tank only
    }

    public static class ModStats
    {
        public const string HBS_Company_MechTech_Skill = "MechTechSkill";
        public const string HBS_Company_MedTech_Skill = "MedTechSkill";

        public const string Aerospace_Skill = "HR_AerospaceSkill";
        public const string Company_Reputation = "HR_CompanyRep";
    }

}
