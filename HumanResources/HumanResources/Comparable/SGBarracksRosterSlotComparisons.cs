using BattleTech.UI;
using HumanResources.Extensions;

namespace HumanResources.Comparable
{
    public class SGBarracksRosterSlotComparisons
    {
        public static int CompareByCrewDetailTypeAndExperience(SGBarracksRosterSlot slot1, SGBarracksRosterSlot slot2)
        {
            // Check nullity
            if (slot1 == null && slot2 == null) return 0;
            else if (slot1 != null && slot2 == null) return 1;
            else if (slot1 == null && slot2 != null) return -1;

            // Check pilot vs. non-pilot
            if (slot1.Pilot == null && slot2.Pilot == null) return 0;
            else if (slot1.Pilot != null && slot2.Pilot == null) return 1;
            else if (slot1.Pilot == null && slot2.Pilot != null) return -1;

            // Check pilot vs. non-pilot using pilotDefs
            if (slot1.Pilot.pilotDef == null && slot2.Pilot.pilotDef == null) return 0;
            else if (slot1.Pilot.pilotDef != null && slot2.Pilot.pilotDef == null) return 1;
            else if (slot1.Pilot.pilotDef == null && slot2.Pilot.pilotDef != null) return -1;

            // Check details
            CrewDetails cd1 = new CrewDetails(slot1.Pilot.pilotDef);
            CrewDetails cd2 = new CrewDetails(slot2.Pilot.pilotDef);

            // Compare by type
            int typeComparison = CrewDetails.CompareByType(cd1, cd2);
            if (typeComparison != 0) return typeComparison;

            // Compare by skill
            int skillComparison = CrewDetails.CompareBySkill(cd1, cd2);
            if (skillComparison != 0) return skillComparison;

            return 0;
        }
    }
}
