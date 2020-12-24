using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HumanResources.Extensions;
using HumanResources.Helper;
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

            // Reinitialize state
            ModState.Reset();
            ModState.SimGameState = __instance;
        }
    }

    // Initialize all the stats we need. This is invoked after both the load-from-save and new game paths.
    [HarmonyPatch(typeof(SimGameState), "InitCompanyStatValidators")]
    static class SimGameState_InitCompanyStatValidators
    {
        static void Postfix(SimGameState __instance)
        {
            // Initialize company stats if they aren't present
            Statistic aeroSkillStat = __instance.CompanyStats.GetStatistic(ModStats.Aerospace_Skill);
            if (aeroSkillStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.Aerospace_Skill, 0); }

            Statistic companyRepStat = __instance.CompanyStats.GetStatistic(ModStats.Company_Reputation);
            if (companyRepStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.Company_Reputation, 0); }

            Statistic crewCountAerospaceStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_Aerospace);
            if (crewCountAerospaceStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_Aerospace, 0); }

            Statistic crewCountMechWarriorsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechWarriors);
            if (crewCountMechWarriorsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechWarriors, 0); }

            Statistic crewCountMechTechsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MechTechs);
            if (crewCountMechTechsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MechTechs, 0); }

            Statistic crewCountMedTechsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_MedTechs);
            if (crewCountMedTechsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_MedTechs, 0); }

            Statistic crewCountVehicleCrewsStat = __instance.CompanyStats.GetStatistic(ModStats.CrewCount_VehicleCrews);
            if (crewCountVehicleCrewsStat == null) { __instance.CompanyStats.AddStatistic<int>(ModStats.CrewCount_VehicleCrews, 0); }
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
                // Mod.Log.Debug?.Write("FIRING EVENT");
                //SimGameEventDef companyEventDef = __instance.DataManager.SimGameEventDefs.Get("event_hr_co_smearCampaign");
                //___companyEventTracker.ActivateEvent(companyEventDef, null);

                // TODO: Only do check if not traveling and not at planet with no_population tag
                foreach (Pilot pilot in __instance.PilotRoster)
                {

                    Mod.Log.Debug?.Write($"Pilot: {pilot.Name} has tags:");
                    // Enumerate tags
                    foreach (string tag in pilot.pilotDef.PilotTags)
                    {
                        Mod.Log.Debug?.Write($" -- {tag}");
                    }

                    CrewDetails details = ModState.GetCrewDetails(pilot.pilotDef);
                    if (details.ExpirationDay <= __instance.DaysPassed)
                    {
                        Mod.Log.Debug?.Write($"CONTRACT FOR PILOT: {pilot.Name} HAS ELAPSED, FIRING EVENT");
                        ModState.ExpiredContracts.Enqueue((pilot, details));
                    }
                }

                // Fire the first event, if there are more they will be fired from OnEventDismissed
                if (ModState.ExpiredContracts.Count > 0)
                {
                    (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                    SimGameEventDef newEvent = EventHelper.ModifyContractExpirationEventForPilot(expired.Pilot, expired.Details);
                    Mod.Log.Info?.Write($"Contract expiration event fired.");
                    ModState.SimGameState.OnEventTriggered(newEvent, EventScope.MechWarrior, ___mechWarriorEventTracker);
                }
            }

        }
    }

    // Check for any other events that need to be fired.
    [HarmonyPatch(typeof(SimGameState), "OnEventDismissed")]
    static class SimGameState_OnEventDismissed
    {
        static void Postfix(SimGameEventTracker ___mechWarriorEventTracker)
        {
            // First, dequeue to remove any existing pilot
            if (ModState.ExpiredContracts.Count > 0)
                ModState.ExpiredContracts.Dequeue();

            // If there are any other events left after that, fire them here.
            if (ModState.ExpiredContracts.Count > 0)
            {
                (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                SimGameEventDef newEvent = EventHelper.ModifyContractExpirationEventForPilot(expired.Pilot, expired.Details);
                Mod.Log.Info?.Write($"Contract expiration event fired.");
                ModState.SimGameState.OnEventTriggered(newEvent, EventScope.MechWarrior, ___mechWarriorEventTracker);
            }
        }
    }


    [HarmonyPatch(typeof(SimGameState), "AddPilotToRoster")]
    [HarmonyPatch(new Type[] { typeof(PilotDef), typeof(bool), typeof(bool) })]
    [HarmonyAfter(new string[] { "us.tbone.TisButAScratch" })]
    static class SimGameState_AddPilotToRoster
    {
        static void Postfix(SimGameState __instance, PilotDef def)
        {

            Mod.Log.Info?.Write($"Adding new pilot def: {def} to roster.");
            CrewDetails details = ModState.GetCrewDetails(def);

            // Add any mechtech, medtech, or aerospace points
            if (details.IsAerospaceCrew)
            {
                // Track our skill points
                Mod.Log.Info?.Write($"  -- crew is aerospace, adding: {details.Value} aerospace skill");
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                     ModStats.Aerospace_Skill,
                     StatCollection.StatOperation.Int_Add, details.Value);

                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_Aerospace,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsMechTechCrew)
            {
                Mod.Log.Info?.Write($"  -- crew is mechtech, adding: {details.Value} mechtech skill");
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MechTech_Skill,
                    StatCollection.StatOperation.Int_Add, details.Value);

                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_MechTechs,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsMechWarrior)
            {
                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_MechWarriors,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsMedTechCrew)
            {
                Mod.Log.Info?.Write($"  -- crew is medtech, adding: {details.Value} medtech skill");
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.HBS_Company_MedTech_Skill,
                    StatCollection.StatOperation.Int_Add, details.Value);

                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_MedTechs,
                    StatCollection.StatOperation.Int_Add, 1);
            }
            else if (details.IsVehicleCrew)
            {
                // Track the crew count
                __instance.CompanyStats.ModifyStat<int>(null, -1,
                    ModStats.CrewCount_VehicleCrews,
                    StatCollection.StatOperation.Int_Add, 1);
            }

            if (ModState.IsHiringFlow)
            {
                // We are a real hire, not from initial spawn
                Mod.Log.Debug?.Write("  -- performing hiring checks.");

                // An actual hire versus start-of-game initialization. Revert the costs previously applied in HirePilot
                int purchaseCostAfterReputationModifier = __instance.CurSystem.GetPurchaseCostAfterReputationModifier(__instance.GetMechWarriorHiringCost(def));
                __instance.AddFunds(purchaseCostAfterReputationModifier, null, true, true);

                // Apply their hiring bonus
                Mod.Log.Info?.Write($"Hiring crew for bonus: {details.AdjustedBonus}");
                __instance.AddFunds(-details.AdjustedBonus, null, true, true);

                // Only refresh if we're actually in game
                __instance.RoomManager.RefreshTimeline(false);
                __instance.RoomManager.RefreshDisplay();
            }

            // DEBUG
            Mod.Log.Debug?.Write($"ITERATING TAGS ON DEF");
            foreach (string tag in def.PilotTags)
            {
                Mod.Log.Debug?.Write($" -- tag: {tag}");
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

            CrewDetails details = ModState.GetCrewDetails(p.pilotDef);

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
                            StatCollection.StatOperation.Int_Subtract, details.Value);

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
                    StatCollection.StatOperation.Int_Subtract, details.Value);

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
                    StatCollection.StatOperation.Int_Subtract, details.Value);

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
                CrewDetails details = ModState.GetCrewDetails(def);
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

                CrewDetails details = ModState.GetCrewDetails(def);
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

                // Initialize new details for the pre-generated pilots
                CrewDetails details = new CrewDetails(pilot.pilotDef, CrewType.MechWarrior);
                ModState.UpdateOrCreateCrewDetails(pilot.pilotDef, details);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GetMechWarriorValue")]
    static class SimGameState_GetMechWarriorValue
    {
        static bool Prefix(SimGameState __instance, PilotDef def, ref int __result)
        {
            CrewDetails details = ModState.GetCrewDetails(def);
            __result = details != null ? details.Salary : 0;

            return false;
        }
    }

    [HarmonyPatch(typeof(SGEventPanel), "OnOptionSelected")]
    static class SimGameState_OnOptionSelected
    {
        public static bool isExpiredContractEvent = false;

        // Prefix here to set stat values for money?
        static void Prefix(SGEventPanel __instance, SimGameEventOption option,
            SimGameInterruptManager.EventPopupEntry ___thisEntry)
        {
            if (___thisEntry != null && ___thisEntry.parameters != null &&
                ___thisEntry.parameters[0] is SimGameEventDef eventDef &&
                ModConsts.Event_ContractExpired.Equals(eventDef.Description.Id))
            {
                isExpiredContractEvent = true;
            }
            else
            {
                isExpiredContractEvent = false;
            }
        }

        static void Postfix(SGEventPanel __instance, SimGameEventOption option, 
            SimGameInterruptManager.EventPopupEntry ___thisEntry)
        {

            if (___thisEntry != null && ___thisEntry.parameters != null &&
                ___thisEntry.parameters[0] is SimGameEventDef eventDef &&
                ModConsts.Event_ContractExpired.Equals(eventDef.Description.Id))
            {

                // Handle updating the contract length in the def; funds are handled by the event.
                if (ModConsts.Event_ContractExpired_Option_Hire_NoBonus.Equals(option.Description.Id) ||
                    ModConsts.Event_ContractExpired_Option_Hire_Bonus.Equals(option.Description.Id))
                {
                    (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();

                    Mod.Log.Debug?.Write($"Pilot {expired.Pilot.Name} was re-hired w/o a bonus");
                    expired.Details.ExpirationDay = ModState.SimGameState.DaysPassed + expired.Details.ContractTerm;
                    ModState.UpdateOrCreateCrewDetails(expired.Pilot.pilotDef, expired.Details);
                }
            }

            isExpiredContractEvent = false;
        }
    }

    // Select an expired pilot and populate the fields with their data
    [HarmonyPatch(typeof(SGEventPanel), "SetEvent")]
    static class SGEventPanel_SetEvent
    {
        // Populate the target mechwarrior before the event
        static void Prefix(SGEventPanel __instance, SimGameEventDef evt)
        {
            Mod.Log.Debug?.Write("SGEP:SetEvent:PRE");
            if (ModConsts.Event_ContractExpired.Equals(evt.Description.Id) && ModState.ExpiredContracts.Count > 0)
            {
                (Pilot Pilot, CrewDetails Details) expired = ModState.ExpiredContracts.Peek();
                Mod.Log.Trace?.Write($"SGEventPanel details setting targetMechwarrior: {expired.Pilot.Name}");
                ModState.SimGameState.Context.SetObject(GameContextObjectTagEnum.TargetMechWarrior, expired.Pilot);
            }

        }

    }
}
