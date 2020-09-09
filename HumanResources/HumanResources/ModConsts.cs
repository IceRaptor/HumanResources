

namespace PitCrew
{
    public static class ModConsts
    {

        public const string GO_HBS_MW_Stats_Block = "mw_Stats";
        public const string GO_HBS_Profile_Layout_Stats = "layout_Stats-Or-HiringCost";

        public const string GO_Profile_Override = "hr_crew_profile";
        public const string GO_Crew_Block = "hr_crew_block";
        public const string GO_Crew_Skill = "hr_crew_skill_text";
        public const string GO_Crew_Size = "hr_crew_size_text";
    }

    // All of these tags are pilot tags
    public static class ModTags
    {
        public const string Tag_CrewType_MedTech = "HR_CREW_MED_TECH";
        public const string Tag_CrewType_MechTech = "HR_CREW_MECH_TECH";
        public const string Tag_CrewType_Vehicle = "HR_CREW_VEHICLE";
        public const string Tag_CrewSize_Prefix = "HR_CREW_SIZE_";

        // CU Vehicles - pilot_vehicle_crew allow tank piloting, 
        // if also has pilot_nomech_crew than its tank only
    }

    public static class ModStats
    {

    }
}
