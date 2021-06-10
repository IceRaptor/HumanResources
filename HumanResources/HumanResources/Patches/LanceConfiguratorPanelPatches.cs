using BattleTech;
using BattleTech.UI;
using Harmony;
using HumanResources.Crew;
using System.Collections.Generic;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(LanceConfiguratorPanel), "SetData")]
    static class LanceConfiguratorPanel_SetData
    {
        static void Prefix(LanceConfiguratorPanel __instance, ref List<Pilot> pilots)
        {
            // Remove any pilots who are aerospace, mechtechs, or medtechs
            Mod.Log.Debug?.Write("Filtering pilots for LanceConfiguratorPanel");
            List<Pilot> selectablePilots = new List<Pilot>();
            foreach (Pilot p in pilots)
            {
                CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                if (details.IsMechWarrior || details.IsVehicleCrew)
                {
                    Mod.Log.Debug?.Write($"Pilot {p.Name} is a mechwarrior or vehicle crew, adding as option");
                    selectablePilots.Add(p);
                }
                    
            }

            pilots.Clear();
            pilots.AddRange(selectablePilots);
        }
    }
}
