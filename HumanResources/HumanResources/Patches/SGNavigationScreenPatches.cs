using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using HumanResources.Helper;
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
            
            StarSystem selectedSystem = ModState.SimGameState.Starmap.CurSelected.System;
            PilotScarcity scarcity = PilotHelper.GetScarcityForPlanet(selectedSystem);

            StringBuilder sb = new StringBuilder(selectedSystem.Def.Description.Details);
            sb.Append("\n");

            if (scarcity.MechWarriors.Upper > 0)
            {
                Mod.Log.Debug?.Write($"  MechWarriors lowerBound: {scarcity.MechWarriors.Lower}  upperBound: {scarcity.MechWarriors.Upper}");
                string mechWarriorsBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_MW_BOUNDS],
                    new object[] { scarcity.MechWarriors.Lower, scarcity.MechWarriors.Upper }).ToString();
                sb.Append("\n");
                sb.Append(mechWarriorsBoundsText);
            }

            if (Mod.Config.HiringHall.VehicleCrews.Enabled && scarcity.Vehicles.Lower > 0)
            {
                Mod.Log.Debug?.Write($"  Vehicles lowerBound: {scarcity.Vehicles.Lower}  upperBound: {scarcity.Vehicles.Upper}");
                string vehicleCrewBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_VEHICLE_BOUNDS],
                    new object[] { scarcity.Vehicles.Lower, scarcity.Vehicles.Upper }).ToString();
                sb.Append("\n");
                sb.Append(vehicleCrewBoundsText);
            }

            if (Mod.Config.HiringHall.AerospaceWings.Enabled && scarcity.Aerospace.Lower > 0)
            {
                Mod.Log.Debug?.Write($"  Aerospace lowerBound: {scarcity.Aerospace.Lower}  upperBound: {scarcity.Aerospace.Upper}");
                string mechTechBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_AERO_BOUNDS],
                    new object[] { scarcity.Aerospace.Lower, scarcity.Aerospace.Upper }).ToString();
                sb.Append("\n");
                sb.Append(mechTechBoundsText);
            }

            if (Mod.Config.HiringHall.MechTechCrews.Enabled && scarcity.MechTechs.Lower > 0)
            {
                Mod.Log.Debug?.Write($"  MechTechs lowerBound: {scarcity.MechTechs.Lower}  upperBound: {scarcity.MechTechs.Upper}");
                string mechTechBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_MECH_TECH_BOUNDS],
                    new object[] { scarcity.MechTechs.Lower, scarcity.MechTechs.Upper }).ToString();
                sb.Append("\n");
                sb.Append(mechTechBoundsText);
            }

            if (Mod.Config.HiringHall.MedTechCrews.Enabled && scarcity.MedTechs.Lower > 0)
            {
                Mod.Log.Debug?.Write($"  MedTechs lowerBound: {scarcity.MedTechs.Lower}  upperBound: {scarcity.MedTechs.Upper}");
                string medTechBoundsText = new Text(Mod.LocalizedText.PlanetStrings[ModText.PT_MED_TECH_BOUNDS],
                    new object[] { scarcity.MedTechs.Lower, scarcity.MedTechs.Upper }).ToString();
                sb.Append("\n");
                sb.Append(medTechBoundsText);
            }

            __instance.SetField(___SystemDescriptionFields, sb.ToString());

        }
    }
}
