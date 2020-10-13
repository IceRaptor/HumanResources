using BattleTech;
using BattleTech.UI;
using HumanResources.Extensions;
using System.Collections.Generic;

namespace HumanResources.Patches
{
    static class LanceConfiguratorPanel_SetData
    {
        static void Prefix(LanceConfiguratorPanel __instance, List<Pilot> pilots)
        {
            // Remove any pilots who are aerospace, mechtechs, or medtechs
            List<Pilot> selectablePilots = new List<Pilot>();
            foreach (Pilot p in pilots)
            {
                CrewDetails details = new CrewDetails(p.pilotDef);
                if (details.IsMechWarrior || details.IsVehicleCrew)
                    selectablePilots.Add(p);
            }

            pilots.Clear();
            pilots.AddRange(selectablePilots);
        }
    }
}
