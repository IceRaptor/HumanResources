using BattleTech;
using Harmony;
using HumanResources.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SGTravelManager), "OnArrivedAtPlanet")]
    static class SGTravelManager_OnArrivedAtPlanet
    {
        static void Postfix(SGTravelManager __instance)
        {
            HeadHuntingHelper.UpdateNextDayOnSystemEntry();
        }
    }
}
