using BattleTech.Framework;
using BattleTech.UI;
using HumanResources.Crew;
using Localize;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(AAR_ContractObjectivesWidget), "FillInObjectives")]
    [HarmonyAfter("ca.jwolf.DropCostsEnhanced")]
    public static class AAR_ContractObjectivesWidget_FillInObjectives
    {

        static void Postfix(AAR_ContractObjectivesWidget __instance)
        {
            try
            {
                List<Pilot> deployedPilots = __instance.theContract.PlayerUnitResults.Select(ur => ur.pilot).ToList();
                int hazardPaySum = 0;
                foreach (Pilot p in deployedPilots)
                {
                    CrewDetails details = ModState.GetCrewDetails(p.pilotDef);
                    hazardPaySum -= details.HazardPay;
                }
                Mod.Log.Debug?.Write($"Total hazard pay for mission is: {hazardPaySum}");

                string hazardPayTitleS = new Text(Mod.LocalizedText.Labels[ModText.LT_Contract_Hazard_Pay],
                    new object[] { SimGameState.GetCBillString(hazardPaySum) }).ToString();
                string guid = Guid.NewGuid().ToString();

                MissionObjectiveResult hazardPayObjective = new MissionObjectiveResult(hazardPayTitleS, guid, false, true, ObjectiveStatus.Succeeded, false);

                __instance.AddObjective(hazardPayObjective);
            }
            catch (Exception e)
            {
                Mod.Log.Warn?.Write(e, "Failed to build hazard pay for contract!");
            }
        }
    }
}
