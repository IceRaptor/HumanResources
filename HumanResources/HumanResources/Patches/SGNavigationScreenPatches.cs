using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.Patches.UI
{
    [HarmonyPatch(typeof(SGSystemViewPopulator), "UpdateRoutedSystem")]
    static class SGSystemViewPopulator_UpdateRoutedSystem
    {
        static void Postfix(SGSystemViewPopulator __instance, List<LocalizableText> ___SystemDescriptionFields)
        {
            Mod.Log.Debug?.Write("Updating system description with scarcity");

            int mechWarriorsUpperBound = 0;
            int vehicleCrewsUpperBound = 0;
            int mechTechsUpperBound = 0;
            int medTechsUpperBound = 0;

            StarSystem selectedSystem = ModState.SimGameState.Starmap.CurSelected.System;
            foreach (string tag in selectedSystem.Tags)
            {
                Mod.Config.HiringHall.ScarcityByPlanetTag.TryGetValue(tag, out CrewScarcity scarcity);
                if (scarcity != null)
                {
                    Mod.Log.Debug?.Write($" tag: {tag} has scarcity =>  " +
                        $"mechwarriors: {scarcity.MechWarriors}  mechTechs: {scarcity.MechTechs}  medTechs: {scarcity.MedTechs}  vehicleCrews: {scarcity.VehicleCrews}");
                    mechWarriorsUpperBound += scarcity.MechWarriors;
                    mechTechsUpperBound += scarcity.MechTechs;
                    medTechsUpperBound += scarcity.MedTechs;
                    vehicleCrewsUpperBound += scarcity.VehicleCrews;
                }
            }
            Mod.Log.Debug?.Write($"Final scarcity bound for planet {selectedSystem.Name} => " +
                $"mechwarriors: {mechWarriorsUpperBound}  mechTechs: {mechTechsUpperBound}  medTechs: {medTechsUpperBound}  vehicleCrews: {vehicleCrewsUpperBound}");

            StringBuilder sb = new StringBuilder(selectedSystem.Def.Description.Details);
            sb.Append("\n");

            if (mechWarriorsUpperBound > 0)
            {
                int mechWarriorsLowerBound = Math.Max(0, mechWarriorsUpperBound / 2);
                Mod.Log.Debug?.Write($"  MechWarriors lowerBound: {mechWarriorsLowerBound}  upperBound: {mechWarriorsUpperBound}");
                string mechWarriorsBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_MW_BOUNDS],
                    new object[] { mechWarriorsLowerBound, mechWarriorsUpperBound }).ToString();
                sb.Append("\n");
                sb.Append(mechWarriorsBoundsText);
            }

            if (vehicleCrewsUpperBound > 0)
            {
                int vehicleCrewsLowerBound = Math.Max(0, vehicleCrewsUpperBound / 2);
                Mod.Log.Debug?.Write($"  VehicleCrews lowerBound: {vehicleCrewsLowerBound}  upperBound: {vehicleCrewsUpperBound}");
                string vehicleCrewBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_VEHICLE_BOUNDS],
                    new object[] { vehicleCrewsLowerBound, vehicleCrewsUpperBound }).ToString();
                sb.Append("\n");
                sb.Append(vehicleCrewBoundsText);
            }

            if (mechTechsUpperBound > 0)
            {
                int mechTechLowerBound = Math.Max(0, mechTechsUpperBound / 2);
                Mod.Log.Debug?.Write($"  MechTechs lowerBound: {mechTechLowerBound}  upperBound: {mechTechsUpperBound}");
                string mechTechBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_MECH_TECH_BOUNDS],
                    new object[] { mechTechLowerBound, mechTechsUpperBound }).ToString();
                sb.Append("\n");
                sb.Append(mechTechBoundsText);
            }

            if (medTechsUpperBound > 0)
            {
                int medTechsLowerBound = Math.Max(0, medTechsUpperBound / 2);
                Mod.Log.Debug?.Write($"  MedTechs lowerBound: {medTechsLowerBound}  upperBound: {medTechsUpperBound}");
                string medTechBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_MED_TECH_BOUNDS],
                    new object[] { medTechsLowerBound, medTechsUpperBound }).ToString();
                sb.Append("\n");
                sb.Append(medTechBoundsText);
            }

            __instance.SetField(___SystemDescriptionFields, sb.ToString());

        }
    }
}
