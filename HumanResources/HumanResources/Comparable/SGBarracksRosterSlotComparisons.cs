using BattleTech.UI;
using HumanResources.Crew;

namespace HumanResources.Comparable
{
    public class SGBarracksRosterSlotComparisons
    {
        public static int CompareByCrewTypeAndValue(SGBarracksRosterSlot slot1, SGBarracksRosterSlot slot2)
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
            CrewDetails cd1 = ModState.GetCrewDetails(slot1.Pilot.pilotDef);
            CrewDetails cd2 = ModState.GetCrewDetails(slot2.Pilot.pilotDef);

            // Compare by type
            int typeComparison = CrewDetails.CompareByType(cd1, cd2);
            if (typeComparison != 0) return typeComparison;

            // Compare by skill
            int skillComparison = CrewDetails.CompareByValue(cd1, cd2);
            if (skillComparison != 0) return skillComparison;

            return 0;
        }
    }
}
