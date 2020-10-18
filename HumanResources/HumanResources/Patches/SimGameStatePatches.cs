﻿using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HumanResources.Extensions;
using HumanResources.Helper;
using Localize;
using SVGImporter;
using System;
using System.Text;
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

            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_Aerospace, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_MechTech, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_MedTech, null);
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.CrewPortrait_Vehicle, null);

            loadRequest.ProcessRequests();
            Mod.Log.Info?.Write("--  Done!");

            ModState.SimGameState = __instance;

            // Initialize company stats if they aren't present
            Statistic aeroSkillStat = __instance.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
            if (aeroSkillStat == null) { __instance.CompanyStats.AddStatistic(ModStats.Aerospace_Skill, 0); }
            Statistic companyRepStat = __instance.CompanyStats.GetStatistic(ModStats.Company_Reputation);
            if (companyRepStat == null) { __instance.CompanyStats.AddStatistic(ModStats.Company_Reputation, 0); }
            
            Statistic crewCountAerospaceStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
            if (crewCountAerospaceStat == null) { __instance.CompanyStats.AddStatistic(ModStats.CrewCount_Aerospace, 0); }
            Statistic crewCountMechWarriorsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
            if (crewCountMechWarriorsStat == null) { __instance.CompanyStats.AddStatistic(ModStats.CrewCount_MechWarriors, 0); }
            Statistic crewCountMechTechsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
            if (crewCountMechTechsStat == null) { __instance.CompanyStats.AddStatistic(ModStats.CrewCount_MechTechs, 0); }
            Statistic crewCountMedTechsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
            if (crewCountMedTechsStat == null) { __instance.CompanyStats.AddStatistic(ModStats.CrewCount_MedTechs, 0); }
            Statistic crewCountVehicleCrewsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
            if (crewCountVehicleCrewsStat == null) { __instance.CompanyStats.AddStatistic(ModStats.CrewCount_VehicleCrews, 0); }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "OnDayPassed")]
    static class SimGameState_OnDayPassed
    {
        static void Postfix(SimGameState __instance, int timeLapse, SimGameInterruptManager ___interruptQueue,
            SimGameEventTracker ___companyEventTracker, SimGameEventTracker ___mechWarriorEventTracker,
            SimGameEventTracker ___deadEventTracker, SimGameEventTracker ___moraleEventTracker)
        {
            Mod.Log.Debug?.Write($"OnDayPassed called with timeLapse: {timeLapse}");

            if (!___interruptQueue.IsOpen)
            {
                // Force fire our test event - works
                Mod.Log.Debug?.Write("FIRING EVENT");
                //SimGameEventDef companyEventDef = __instance.DataManager.SimGameEventDefs.Get("event_hr_co_smearCampaign");
                //___companyEventTracker.ActivateEvent(companyEventDef, null);

                foreach (Pilot pilot in __instance.PilotRoster)
                {

                    Mod.Log.Debug?.Write($"Pilot: {pilot.Name} has tags:");
                    // Enumerate tags
                    foreach (string tag in pilot.pilotDef.PilotTags)
                    {
                        Mod.Log.Debug?.Write($" -- {tag}");
                    }

                    CrewDetails details = new CrewDetails(pilot.pilotDef);
                    if (details.ContractEndDay <= __instance.DaysPassed)
                    {
                        Mod.Log.Debug?.Write($"CONTRACT FOR PILOT: {pilot.Name} HAS ELAPSED, FIRING EVENT");

                        __instance.Context.SetObject(GameContextObjectTagEnum.TargetMechWarrior, pilot);
                        SimGameEventDef crewEventDef = __instance.DataManager.SimGameEventDefs.Get("event_hr_mw_contractExpired");
                        BaseDescriptionDef baseDescDef = crewEventDef.Description;
                        
                        StringBuilder sb = new StringBuilder(baseDescDef.Details);
                        sb.Append("\n");
                        sb.Append("<margin=5em>\n");
                        sb.Append($" Hiring Bonus: {SimGameState.GetCBillString(details.AdjustedBonus)}\n\n");
                        sb.Append($" Monthly Salary: {SimGameState.GetCBillString(details.AdjustedSalary)}\n\n");
                        sb.Append("</margin>\n");

                        Mod.Log.Debug?.Write($"Setting baseDefDesc to: {sb}");

                        Traverse baseDescDefDetailsSetT = Traverse.Create(baseDescDef).Property("Details");
                        baseDescDefDetailsSetT.SetValue(sb.ToString());

                        Traverse crewEventDescT = Traverse.Create(crewEventDef).Property("Description");
                        crewEventDescT.SetValue(baseDescDef);

                        ___mechWarriorEventTracker.ActivateEvent(crewEventDef);

                    }
                }
            }
            else
            {
                Mod.Log.Debug?.Write("EVENT CONDITIONALS FAILED");
            }
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
            if (details.IsAerospaceCrew)
            {
                // Track our skill points
                Statistic aerospaceSkill = __instance.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
                if (aerospaceSkill == null)
                    aerospaceSkill = __instance.CompanyStats.AddStatistic<int>(ModStats.Aerospace_Skill, 0);

                if (aerospaceSkill.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                            ModStats.Aerospace_Skill,
                            StatCollection.StatOperation.Int_Subtract, details.AerospacePoints);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_Aerospace, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_Aerospace,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsMechTechCrew)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MechTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.MechTechPoints);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechTechs, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MechTechs,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsMechWarrior)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MechTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.MechTechPoints);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechWarriors, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MechWarriors,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsMedTechCrew)
            {
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MedTech_Skill,
                    StatCollection.StatOperation.Int_Subtract, details.MedTechPoints);

                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MedTechs, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_MedTechs,
                        StatCollection.StatOperation.Int_Subtract, 1);
            }
            else if (details.IsVehicleCrew)
            {
                // Track the crew count
                Statistic crewCount = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
                if (crewCount == null)
                    crewCount = __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_VehicleCrews, 0);

                if (crewCount.Value<int>() > 0)
                    __instance.CompanyStats.ModifyStat<int>(null, -1,
                        ModStats.CrewCount_VehicleCrews,
                        StatCollection.StatOperation.Int_Subtract, 1);
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
            Mod.Log.Trace?.Write($"Removing {vanillaCosts} costs x {expenditureCostModifier} expenditureMulti " +
                $"= {vanillaTotal} total vanilla costs.");
            __result -= vanillaTotal;

            // Add the new costs
            __result += newCosts;
        }
    }

    // Upgrade all the existing pilots w/ a contract time
    [HarmonyPatch(typeof(SimGameState), "OnCareerModeCharacterCreationComplete")]
    static class SimGameState_OnCareerModeCharacterCreationComplete
    {
        static void Postfix(SimGameState __instance)
        {
            foreach (Pilot pilot in __instance.PilotRoster)
            {
                if (pilot.pilotDef.IsFree && pilot.pilotDef.IsImmortal) continue; // player character, skip

                // Determine contract length
                int contractLength = PilotHelper.RandomContractLength(Mod.Config.HiringHall.MechWarriors);
                pilot.pilotDef.PilotTags.Add($"{ModTags.Tag_Crew_ContractTerm_Prefix}{contractLength}");
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetMechWarriorValue")]
    static class SimGameState_GetMechWarriorValue
    {
        static bool Prefix(SimGameState __instance, PilotDef def, ref int __result)
        {
            CrewDetails details = new CrewDetails(def);
            __result = details.Salary;

            return false;
        }
    }
}
