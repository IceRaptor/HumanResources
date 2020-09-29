using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SG_HiringHall_Screen), "OnPilotSelected")]
    static class SG_HiringHall_Screen_OnPilotSelected
    {
        static bool Prefix(SG_HiringHall_Screen __instance, Pilot p, 
            Pilot ___selectedPilot, GameObject ___DescriptionAreaObject, SG_HiringHall_DetailPanel ___DetailPanel, SG_HiringHall_MWSelectedPanel ___MWSelectedPanel)
        {
            ___selectedPilot = p;
            ___DescriptionAreaObject.SetActive(value: true);
            ___DetailPanel.SetPilot(___selectedPilot);
            ___MWSelectedPanel.SetPilot(___selectedPilot);
            __instance.WarningsCheck();
            __instance.UpdateMoneySpot();

            //__instance.ForceRefreshImmediate();

            return false;
        }
    }
}
