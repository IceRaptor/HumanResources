using HumanResources.Crew;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(Contract), "CompleteContract")]
    [HarmonyAfter("ca.jwolf.DropCostsEnhanced")]
    public static class Contract_CompleteContract
    {

        static void Postfix(Contract __instance)
        {
            try
            {
                List<Pilot> deployedPilots = __instance.PlayerUnitResults.Select(ur => ur.pilot).ToList();
                int hazardPaySum = 0;
                foreach (Pilot p in deployedPilots)
                {
                    CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                    hazardPaySum -= details.HazardPay;
                }
                Mod.Log.Debug?.Write($"Total hazard pay for contract is: {hazardPaySum}");

                int newResult = Mathf.FloorToInt(__instance.MoneyResults - hazardPaySum);
                Traverse.Create(__instance).Property("MoneyResults").SetValue(newResult);
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Write(e, "Failed to add hazard pay costs to contract");
            }
        }
    }
}
