using BattleTech;
using BattleTech.Data;
using Harmony;
using HumanResources.Extensions;
using Localize;
using SVGImporter;
using System;
using UnityEngine;

namespace HumanResources.Patches
{
    [HarmonyPatch(typeof(SimGameState), "_OnInit")]
    static class SimGameState__OnInit
    {
        static void Postfix(SimGameState __instance, GameInstance game, SimGameDifficulty difficulty)
        {
            if (Mod.Log.IsTrace) Mod.Log.Trace?.Write("SGS:_OI entered.");

            DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
            LoadRequest loadRequest = dm.CreateLoadRequest();

            // Need to load each unique icon
            Mod.Log.Info?.Write("-- Loading HUD icons");
            
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_MechTech, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_MedTech, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_Vehicle, null);

            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.GroupPortrait, null);

            loadRequest.ProcessRequests();
            Mod.Log.Info?.Write("--  Done!");

            ModState.SimGameState = __instance;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    static class SimGameState_OnDayPassed
    {
        static void Postfix(SimGameState __instance, int timeLapse)
        {
            Mod.Log.Debug?.Write($"OnDayPassed called with timeLapse: {timeLapse}");
        }
    }

    [HarmonyPatch(typeof(SimGameState), "DismissPilot")]
    [HarmonyPatch(new Type[] { typeof(Pilot) })]
    static class SimGameState_DismissPilot
    {
        static void Prefix(SimGameState __instance, Pilot p)
        {
            if (p == null || !__instance.PilotRoster.Contains(p)) return;

            Mod.Log.Debug?.Write($"Removing pilot: {p.Name} from company.");

            CrewDetails details = p.pilotDef.Evaluate();

            // Remove any mechtech, medtech, or aerospace points
            if (details.IsMechTechCrew)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MechTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.MechTechPoints);
            }
            else if (details.IsMedTechCrew)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MedTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.MedTechPoints);
            }
            else if (details.IsAerospaceCrew)
            {
                Statistic aerospaceSkill = __instance.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
                if (aerospaceSkill == null)
                    __instance.CompanyStats.AddStatistic<int>(ModStats.Aerospace_Skill, 0);

                __instance.CompanyStats.ModifyStat<int>(null, -1,
                     ModStats.Aerospace_Skill,
                     StatCollection.StatOperation.Int_Subtract, details.AerospacePoints);
            }
            else if (details.IsVehicleCrew)
            {

            }

            __instance.RoomManager.RefreshTimeline(false);
            __instance.RoomManager.RefreshDisplay();
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetMechWarriorSalary")]
    static class SimGameState_GetMechWarriorSalary
    {
        static bool Prefix(SimGameState __instance, PilotDef def, ref string __result)
        {
            string salaryLabel = "------";
            if (!def.IsFree)
            {
                CrewDetails details = new CrewDetails(def);
                string cbillString = SimGameState.GetCBillString(Mathf.RoundToInt(details.AdjustedSalary));
                salaryLabel = new Text(Mod.LocalizedText.Labels[ModText.LT_Crew_Salary_Label],
                    new object[] { cbillString })
                    .ToString();
            }
            __result = salaryLabel;

            return false;
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetExpenditures")]
    [HarmonyPatch(new Type[] { typeof(EconomyScale), typeof(bool) })]
    [HarmonyAfter(new string[] { "de.morphyum.MechMaintenanceByCost", "us.frostraptor.IttyBittyLivingSpace" })]
    public static class SimGameState_GetExpenditures
    {
        public static void Postfix(SimGameState __instance, ref int __result, EconomyScale expenditureLevel, bool proRate)
        {
            Mod.Log.Trace?.Write($"SGS:GE entered with {__result}");

            // Evaluate old versus new cost
            int vanillaCosts = 0;
            int newCosts = 0;
            for (int i = 0; i < __instance.PilotRoster.Count; i++)
            {
                PilotDef def = __instance.PilotRoster[i].pilotDef;
                vanillaCosts += __instance.GetMechWarriorValue(def);

                CrewDetails details = new CrewDetails(def);
                newCosts += details.AdjustedSalary;
            }

            // Multiply old costs by expenditure. New has that built in. Then subtract them from the running total
            float expenditureCostModifier = __instance.GetExpenditureCostModifier(expenditureLevel);
            int vanillaTotal = Mathf.CeilToInt((float)vanillaCosts * expenditureCostModifier);
            Mod.Log.Debug?.Write($"Removing {vanillaCosts} costs x {expenditureCostModifier} expenditureMulti " +
                $"= {vanillaTotal} total vanilla costs.");
            __result -= vanillaTotal;

            // Add the new costs
            __result += newCosts;
        }
    }
}
